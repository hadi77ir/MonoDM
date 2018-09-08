using System;
using System.Linq;
using Gtk;
using MonoDM.Core;
using MonoDM.Core.Common;
using MonoDM.Core.UI;

namespace MonoDM.App.UI
{
    public class AddDownloadDialog : Gtk.Dialog
    {
        public AddDownloadDialog()
        {
            InitializeComponent();
        }

        private Entry txtUrl;
        private Entry txtFilename;
        void InitializeComponent()
        {
            txtUrl = new Entry();
            txtFilename = new Entry();
            txtUrl.Sensitive = false;
                        
            VBox.PackStart(txtUrl, true, true, 10);
            HBox locBox = new HBox();
            locBox.PackStart(new Label("Save to:"), false,false, 10);
            locBox.PackStart(txtFilename,true,true,10);

            Button btnBrowse = new Button("Browse");
            btnBrowse.Activated += BtnBrowseOnActivated;
            locBox.PackEnd(btnBrowse,false,false,10);
            VBox.PackStart(locBox);
            
            AddButton("Cancel", ResponseType.Cancel);
            AddButton("Start Download", ResponseType.Ok);
            AddButton("Download Later", ResponseType.Apply);
            DefaultResponse = ResponseType.Cancel;

            Response += (o, args) =>
            {
                
                if (args.ResponseId == ResponseType.Ok)
                {
                    btnStart_Click(o, args);
                }
                else if (args.ResponseId == ResponseType.Apply)
                {
                    btnLater_Click(o, args);
                }
                else
                {
                    btnCancel_Click(o, args);
                }
                //Destroy();
            };
            ShowAll();
        }

        private void BtnBrowseOnActivated(object sender, EventArgs e)
        {
            using (FileChooserDialog fc = new FileChooserDialog("Save to...", (Window) Toplevel,
                FileChooserAction.Save, "Cancel", ResponseType.Cancel,
                "Save", ResponseType.Accept))
            {
                if (fc.Run() == (int) ResponseType.Accept)
                {
                    txtFilename.Text = fc.Filename;
                }
            }
        }

        private ResourceLocation rl;
        public ResourceLocation DownloadLocation
        {
            get { return rl; }
            set
            {
                rl = value;
                if (value != null && txtFilename != null)
                {
                    txtFilename.Text = System.IO.Path.GetFullPath(System.IO.Path.Combine(GetDownloadFolder(),
                        PathHelper.GetFileNameFromUrl(rl.URL)));
                }
                if (value != null && txtUrl != null)
                {
                    txtUrl.Text = rl.URL;
                }
            }
        }

        private string GetDownloadFolder()
        {
            if(!string.IsNullOrEmpty(MonoDM.Core.Settings.Default.DownloadFolder) && MonoDM.Core.Settings.Default.DownloadFolder.Trim('/') != "")
                return MonoDM.Core.Settings.Default.DownloadFolder;
            
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
        }

        public string LocalFile
        {
            get
            {
                return System.IO.Path.GetFullPath(txtFilename.Text);
            }
        }

        private int getSegments()
        {
            return MonoDM.Core.Settings.Default.MaxSegments;
        }

        private void btnStart_Click(object o, ResponseArgs args)
        {
            //AddDownload(false);
        }
        
        private void btnLater_Click(object o, ResponseArgs args)
        {
            //AddDownload(true);
        }

        public Downloader AddDownload(bool later)
        {
             return DownloadManager.Instance.Add(
                rl,
                new ResourceLocation[0],
                this.LocalFile,
                getSegments(),
                !later);
        }

        private void btnCancel_Click(object o, ResponseArgs args)
        {
            
        }
        
    }
}
