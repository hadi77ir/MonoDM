using System;
using System.Linq;
using System.Timers;
using Gdk;
using Gtk;
using MonoDM.App.Controls;
using MonoDM.Core;
using MonoDM.Core.Common;

namespace MonoDM.App.UI
{
    public class DownloadProgressWindow : Gtk.Dialog
    {
        private Downloader _downloader;

        public DownloadProgressWindow(Downloader d)
        {
            _downloader = d;
            InitializeComponent();
            RefreshDownloaderData();

            DeleteEvent += (o, args) => { args.RetVal = HideOnDelete(); };
        }

        void InitializeComponent()
        {
            DefaultSize = new Size(600,400);
            
            var mainVbox = new Gtk.VBox();
            progressBar = new ProgressBar();
            
            //blockedProgressBar = new BlockedProgressBarWidget();
            segmentList = new SegmentList(_downloader);
            //segmentList.QueueDraw();
            lblUrl = new Label("");
            mainVbox.PackStart(lblUrl, false, false, 0);

            Table tbl = new Table(6, 2, false);
            tbl.Attach(new Label("Status"), 0, 1, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 10, 5);
            lblStatus = new Label("");
            tbl.Attach(lblStatus, 1, 2, 0, 1);
            
            tbl.Attach(new Label("File Size"), 0, 1, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 10, 5);
            lblFileSize = new Label("");
            tbl.Attach(lblFileSize, 1, 2, 1, 2);
            
            tbl.Attach(new Label("Transferred"), 0, 1, 2, 3, AttachOptions.Shrink, AttachOptions.Shrink, 10, 5);
            lblTransferred = new Label("");
            tbl.Attach(lblTransferred, 1, 2, 2, 3);
            
            tbl.Attach(new Label("Transfer Rate"), 0, 1, 3, 4, AttachOptions.Shrink, AttachOptions.Shrink, 10, 5);
            lblTransferRate = new Label("");
            tbl.Attach(lblTransferRate, 1, 2, 3, 4);
            
            tbl.Attach(new Label("Time Left"), 0, 1, 4, 5, AttachOptions.Shrink, AttachOptions.Shrink, 10, 5);
            lblTimeLeft = new Label("");
            tbl.Attach(lblTimeLeft, 1, 2, 4, 5);
            
            tbl.Attach(new Label("Resumable?"), 0, 1, 5, 6, AttachOptions.Shrink, AttachOptions.Shrink, 10, 5);
            lblResumable = new Label("");
            tbl.Attach(lblResumable, 1, 2, 5, 6);
            
            mainVbox.PackStart(tbl, false, false, 10);
            mainVbox.PackStart(progressBar, false, false, 10);
            // mainVbox.PackStart(blockedProgressBar,false,false,10);

            HButtonBox hb = new HButtonBox();
            
            Button btn = new Button("Cancel");
            btn.Clicked += StopDownload;
            hb.PackEnd(btn, true, true, 0);
            
            toggleStateButton = new Button(_downloader.State == DownloaderState.Working ? "Pause" : "Resume");
            toggleStateButton.Clicked += ToggleDownloaderState;
            //toggleStateButton.Activated += ToggleDownloaderState;
            hb.PackEnd(toggleStateButton, true, true, 0);

            mainVbox.PackStart(hb);

            var scrollWindow = new ScrolledWindow();
            scrollWindow.Child = segmentList;
            mainVbox.PackStart(scrollWindow, true, true, 10);

            VBox.PackStart(mainVbox);
            ShowAll();

            // start a timer to refresh download info
            timer = new Timer {Interval = 500, Enabled = true};
            timer.Elapsed += RefreshCallback;
            //Realized += (sender, args) => SetSignals();
            Destroyed += (sender, args) =>
            {
                timer.Stop();
                //UnsetSignals();
            };
        }

        private void SetSignals()
        {
            _downloader.SegmentFailed += RefreshCallback;
            _downloader.SegmentStarted += RefreshCallback;
            _downloader.SegmentStarting += RefreshCallback;
            _downloader.SegmentStoped += RefreshCallback;
            _downloader.RestartingSegment += RefreshCallback;
            _downloader.InfoReceived += RefreshCallback;
            _downloader.Ending += RefreshCallback;
        }

        private void UnsetSignals()
        {
            _downloader.SegmentFailed -= RefreshCallback;
            _downloader.SegmentStarted -= RefreshCallback;
            _downloader.SegmentStarting -= RefreshCallback;
            _downloader.SegmentStoped -= RefreshCallback;
            _downloader.RestartingSegment -= RefreshCallback;
            _downloader.InfoReceived -= RefreshCallback;
            _downloader.Ending -= RefreshCallback;
        }

        private void RefreshCallback(object sender, EventArgs e)
        {
            Application.Invoke((s,a) => { 
                RefreshDownloaderData();
                RefreshBlockList();
            });
        }

        private void StopDownload(object sender, EventArgs e)
        {
            Application.Invoke((s, a) => { 
                RefreshDownloaderData();
                _downloader.Pause();
                this.Hide();
            });
        }

        private void ToggleDownloaderState(object sender, EventArgs e)
        {
            Application.Invoke((s, a) =>
            {
                if (_downloader.State == DownloaderState.Working)
                {
                    _downloader.Pause();
                    ((Button) sender).Label = "Resume";
                }
                else if (_downloader.State == DownloaderState.Paused)
                {
                    _downloader.Start();
                    ((Button) sender).Label = "Pause";
                }
            });
        }

        public void RefreshBlockList()
        {
            /*
            blockedProgressBar.BlockList = _downloader.Segments
                .Select(s => new Block(s.TotalToTransfer, (float) s.Progress)).ToList();
            blockedProgressBar.QueueDraw(); 
            */
        }


        public Controls.BlockedProgressBarWidget blockedProgressBar;
        public Gtk.ProgressBar progressBar;
        public Gtk.Label lblUrl;
        public Gtk.Label lblStatus;
        public Gtk.Label lblFileSize;
        public Gtk.Label lblTransferred;
        public Gtk.Label lblTransferRate;
        public Gtk.Label lblTimeLeft;
        public Gtk.Label lblResumable;
        public SegmentList segmentList;
        private Button toggleStateButton;
        private Timer timer;
        private readonly object _lockObj = new object();

        public void RefreshDownloaderData()
        {
            lock (_lockObj)
            {
                if (_downloader.State == DownloaderState.Preparing)
                {
                    lblStatus.Text = "Preparing";
                    return;
                }

                lblUrl.Text = _downloader.ResourceLocation.URL;
                lblStatus.Text = _downloader.LastError != null
                    ? _downloader.State.ToString() + ", " + _downloader.LastError.Message
                    : _downloader.State.ToString();

                toggleStateButton.Label = _downloader.State == DownloaderState.Working ? "Pause" : "Resume";

                progressBar.Fraction = _downloader.Progress / 100.0;
                lblTransferred.Text = string.Format("{0} ({1}%)", ByteFormatter.ToString(_downloader.Transfered),
                    _downloader.Progress);
                lblTransferRate.Text = TransferRateFormatter.ToString(_downloader.Rate);

                if (_downloader.RemoteFileInfo == null)
                    return;

                lblFileSize.Text = ByteFormatter.ToString(_downloader.RemoteFileInfo.FileSize);
                lblResumable.Text = _downloader.RemoteFileInfo == null
                    ? "N/A"
                    : (_downloader.RemoteFileInfo.AcceptRanges ? "Yes" : "No");
                lblTimeLeft.Text = TimeSpanFormatter.ToString(_downloader.Left);
            }
        }
    }
}