using System.IO;
using Gdk;
using GLib;
using Gtk;
using MonoDM.Core;
using MonoDM.Core.Common;

namespace MonoDM.App.UI
{
    public class DownloadFinishedDialog : Dialog
    {
        private Label lblStatus;
        private Label lblSize;
        private Entry txtUrl;
        private Entry txtSaveTo;
        public DownloadFinishedDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Response += (o, args) =>
            {
                if (args.ResponseId == ResponseType.Ok)
                {
                    BtnOpenFile_Click();
                }

                if (args.ResponseId == ResponseType.Apply)
                {
                    BtnOpenFolder_Click();
                }
                //if(args.ResponseId == ResponseType.Close)
                Destroy();
            };
            
            DefaultResponse = ResponseType.Close;
            AddButton("Close", ResponseType.Close);
            AddButton("Open Folder", ResponseType.Apply);
            AddButton("Open File", ResponseType.Ok);

            lblSize = new Label();
            lblStatus = new Label("Download Completed.");
            
            txtUrl = new Entry();
            txtSaveTo = new Entry();

            txtUrl.Sensitive = false;
            txtSaveTo.Sensitive = false;
            
            VBox.PackStart(lblStatus, true,true,20);
            VBox.PackStart(lblSize, true,true,20);
            VBox.PackStart(txtUrl, true,true,20);
            VBox.PackStart(txtSaveTo, true,true,20);
            
            DefaultSize = new Size(400,200);
            Resizable = false;
            
            ShowAll();
        }

        public string SaveTo
        {
            get { return txtSaveTo.Text; }
            set { txtSaveTo.Text = System.IO.Path.GetFullPath(value); }
        }
        
        public string Url
        {
            get { return txtUrl.Text; }
            set { txtUrl.Text = value; }
        }

        public Downloader Downloader { get; set; }

        public void RefreshUI()
        {
            lblSize.Text = string.Format("{0} ({1} bytes) of {2} ({3} bytes), {4:P}",
                ByteFormatter.ToString(Downloader.Transfered),
                Downloader.Transfered,
                ByteFormatter.ToString(Downloader.RemoteFileInfo?.FileSize ?? 0), 
                Downloader.RemoteFileInfo?.FileSize.ToString() ?? "N/a",
                Downloader.Progress);
        }

    private void BtnOpenFolder_Click()
        {
            try
            {
                OsUtils.OpenFolder(SaveTo);
            }
            catch
            {
            }
        }

        private void BtnOpenFile_Click()
        {
            try
            {
                System.Diagnostics.Process.Start(SaveTo);
            }catch{}
        }
    }
}