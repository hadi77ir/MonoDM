using System;
using System.Diagnostics;
using Gdk;
using IO = System.IO;
using Gtk;
using MonoDM.Core;
using MonoDM.Core.Common;
using MonoDM.Core.UI;
using Window = Gtk.Window;

namespace MonoDM.App.UI
{
    public class DownloadPropertiesDialog : Dialog
    {
        public DownloadPropertiesDialog()
        {
            InitializeComponent();
        }

        private Downloader _downloader;
        private Entry _txtUrl;
        private Entry _txtDest;
        private Label _lblStatus;
        private Label _lblSize;
        private Label _lblType;
        private Entry _txtUser;
        private Entry _txtPassword;
        private CheckButton _cbAuth;
        private Label _lblFileName;

        private void InitializeComponent()
        {
            Resizable = false;
            DefaultSize = new Size(500, 400);
            DefaultResponse = ResponseType.Cancel;
            AddButton("OK", ResponseType.Ok);
            _txtUrl = new Entry();
            _txtDest = new Entry();
            _lblSize = new Label();
            _lblStatus = new Label();
            _lblType = new Label();
            _lblFileName = new Label();

            VBox.PackStart(_lblFileName, false,false, 30);
            
            VBox.PackStart(new HSeparator(), true,true, 10);

            var tbl = new Table(6, 2, false);
            
            tbl.Attach(new Label("Type:"), 0, 1, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            tbl.Attach(_lblType, 1, 2, 0, 1);
            
            tbl.Attach(new Label("Status:"), 0, 1, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            tbl.Attach(_lblStatus, 1, 2, 1, 2);

            tbl.Attach(new Label("Size:"), 0, 1, 2, 3, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            tbl.Attach(_lblSize, 1, 2, 2, 3);

            tbl.Attach(new Label("Save To:"), 0, 1, 3, 4, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            
            Box box = new HBox();
            Button btn = new Button {Label = "Browse..."};
            btn.Activated += BtnOnActivated;
            
            box.PackStart(_txtDest, true, true, 10);
            box.PackStart(btn, false, false, 10);
            tbl.Attach(box, 1, 2, 3, 4);

            tbl.Attach(new Label("URL:"), 0, 1, 4, 5, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            tbl.Attach(_txtUrl, 1, 2, 4, 5);
            
            VBox.PackStart(tbl, true, true, 0);
            
            _txtUser = new Entry();
            _txtPassword = new Entry();
            _cbAuth = new CheckButton();

            _txtPassword.Visibility = false;
		    
            _cbAuth.Toggled += CbAuthOnToggled;
            _cbAuth.Label = "Use Authentication?";

            Frame authFrame = new Frame();
            authFrame.Label = "Authentication";
            VBox authFrameBox = new VBox();
		    
            authFrameBox.PackStart(_cbAuth, false, false, 10);
            HBox credentialsBox = new HBox();
		    
            credentialsBox.PackStart(new Label("Username"), false,false, 10);
            credentialsBox.PackStart(_txtUser, true,true, 10);
            credentialsBox.PackStart(new Label("Password"), false,false, 10);
            credentialsBox.PackStart(_txtPassword, true,true, 10);
            authFrameBox.PackStart(credentialsBox);
            authFrame.Add(authFrameBox);
            
            VBox.PackStart(authFrame, true, true, 10);
            
            Response += OnResponse;
            ShowAll();
        }

        private void OnResponse(object o, ResponseArgs args)
        {
            if (args.ResponseId == ResponseType.Apply)
            {
                Process.Start(_downloader.LocalFile);
                return;
            }

            if (args.ResponseId == ResponseType.Ok)
            {
                SaveChanges();
            }

            Destroy();
        }

        private void BtnOnActivated(object sender, EventArgs e)
        {
                using (var dlg = new FileChooserDialog("Save To...", null,
                    FileChooserAction.Save, "Cancel", ResponseType.Cancel,
                    "Save", ResponseType.Accept))
                {
                    dlg.Response += (o, args) => { dlg.Destroy(); };
                    if (dlg.Run() == (int) ResponseType.Accept)
                    {
                        _txtDest.Text = dlg.Filename;
                    }
                }
        }

        private void CbAuthOnToggled(object sender, EventArgs e)
        {
            _txtUser.Sensitive = _cbAuth.Active;
            _txtPassword.Sensitive = _cbAuth.Active;
        }

        public void SetDownloadData(Downloader d)
        {
            _downloader = d;

            _lblFileName.Text = IO.Path.GetFileName(d.LocalFile);
            
            string size = d.RemoteFileInfo == null ? "N/a" : ByteFormatter.ToString(d.RemoteFileInfo.FileSize);
            _lblSize.Text = string.Format("({0} {1:P})", size, _downloader.Progress / 100.0);

            _lblStatus.Text = _downloader.State.ToString();
            
            string fileType = IO.Path.GetExtension(_downloader.LocalFile)?.Trim('.').ToUpper();
            _lblType.Text = string.IsNullOrEmpty(fileType) ? "N/a" : string.Format("{0} File", fileType);

            _txtDest.Text = _downloader.LocalFile;
            _txtDest.Sensitive = false;

            if (_downloader.IsWorking())
                _txtUrl.Sensitive = false;
            _txtUrl.Text = _downloader.ResourceLocation.URL;

            _cbAuth.Active = _downloader.ResourceLocation.Authenticate;
            CbAuthOnToggled(null, null);
            _txtUser.Text = _downloader.ResourceLocation.Login ?? "";
            _txtPassword.Text = _downloader.ResourceLocation.Password ?? "";


            if (_downloader.State == DownloaderState.Ended)
                AddButton("Open", ResponseType.Apply);
        }

        public void SaveChanges()
        {
            if (!ResourceLocation.IsURL(_txtUrl.Text))
            {
                GtkUtils.ShowMessageBox(null, "Invalid URL.", MessageType.Error);
                return;
            }

            if (!_downloader.IsWorking())
            {
                _downloader.ResourceLocation.URL = _txtUrl.Text;
                _downloader.ResourceLocation.Authenticate = _cbAuth.Active;
                _downloader.ResourceLocation.Login = _txtUser.Text;
                _downloader.ResourceLocation.Password = _txtPassword.Text;
            }
            
            if(_txtDest.Text != _downloader.LocalFile)
                _downloader.MoveTo(_txtDest.Text);
        }
    }
}