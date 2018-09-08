using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using Gtk;
using MonoDM.Core.Common;

namespace MonoDM.Core.UI
{
    public partial class DownloadFolder : BaseWidget
    {
        private Entry txtFolder;
        public DownloadFolder()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Button btn = new Button("Browse");
            HBox h = new HBox();
            h.PackStart(new Label("Download folder:"));
            txtFolder = new Entry();
            txtFolder.Text = MonoDM.Core.Settings.Default.DownloadFolder;
            h.PackStart(txtFolder, true, false, 0);
            h.PackEnd(btn, false, false, 0);
            btn.Activated += BtnOnActivated;

            PackStart(h, false,false, 0);
        }

        private void BtnOnActivated(object sender, EventArgs e)
        {
            using (FileChooserDialog fc = new FileChooserDialog("Save to...", (Window) Toplevel,
                FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel,
                "Select", ResponseType.Accept))
            {
                if (fc.Run() == (int) ResponseType.Accept)
                {
                    txtFolder.Text = fc.CurrentFolder;
                }
            }
        }

        public override string Text => "Directory";
        
        public string Folder
        {
            get { return PathHelper.GetWithBackslash(txtFolder.Text); }
        }

    }
}
