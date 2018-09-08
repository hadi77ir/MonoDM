using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using Gtk;
using MonoDM.Core.Common;
namespace MonoDM.Extension.AntiVirus.UI
{
    public class AVOptions : BaseWidget
    {
        public AVOptions()
        {
            InitializeComponent();

            CheckFileWithAV = AntiVirus.Settings.Default.CheckFileWithAV;
            AVFileName = AntiVirus.Settings.Default.AVFileName;
            FileTypes = AntiVirus.Settings.Default.FileTypes;
            AVParameter = AntiVirus.Settings.Default.AVParameter;

            UpdateControls();
        }

        private CheckButton chkAllowAV;
        private Entry txtAVFileName;
        private Entry txtFileTypes;
        private Entry txtParameter;
        private Button btnSelectAV;
        private void InitializeComponent()
        {
            VBox vbox = new VBox();
            
            chkAllowAV = new CheckButton("Enable?");
            chkAllowAV.Toggled += chkAllowAV_CheckedChanged;
            vbox.PackStart(chkAllowAV, false,false,0);

            Table tbl = new Table(3, 2, false);
            
            tbl.Attach(new Label("AntiVirus executable:"), 0,1,0,1,AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
            HBox hb = new HBox();
            
            txtAVFileName = new Entry();
            hb.PackStart(txtAVFileName, true, true, 0);
            
            btnSelectAV = new Button("Browse");
            btnSelectAV.Activated += BtnSelectAvOnActivated;
            hb.PackEnd(btnSelectAV, true, false, 0);
            
            tbl.Attach(hb, 1,2,0,1);

            
            tbl.Attach(new Label("Command-line arguments:"), 0,1,1,2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
            txtParameter = new Entry();
            tbl.Attach(txtParameter, 1,2,1,2);

            tbl.Attach(new Label("File types:"), 0,1,2,3, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
            txtFileTypes = new Entry();
            tbl.Attach(txtFileTypes, 1,2,2,3);
            
            vbox.PackStart(chkAllowAV, false, false, 0);
            vbox.PackStart(tbl,false,false,0);
            this.PackStart(vbox, false,false, 0);
            ShowAll();
        }

        private void BtnSelectAvOnActivated(object sender, EventArgs e)
        {
            using (FileChooserDialog fc = new FileChooserDialog("Select AntiVirus Program",(Window)Toplevel,FileChooserAction.Open,"Cancel", ResponseType.Cancel,
                "Select", ResponseType.Accept))
            {
                if (fc.Run() == (int)ResponseType.Accept)
                {
                    txtAVFileName.Text = fc.Filename;
                }
            }
        }

        public override string Text => "AntiVirus";

        public bool CheckFileWithAV
        {
            get
            {
                return chkAllowAV.Active;
            }
            set
            {
                chkAllowAV.Active = value;
            }
        }

        public string AVFileName
        {
            get
            {
                return txtAVFileName.Text;
            }
            set
            {
                txtAVFileName.Text = value;
            }
        }

        public string FileTypes
        {
            get
            {
                return txtFileTypes.Text;
            }
            set
            {
                txtFileTypes.Text = value;
            }
        }

        public string AVParameter
        {
            get
            {
                return txtParameter.Text;
            }
            set
            {
                txtParameter.Text = value;
            }
        }

        private void chkAllowAV_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            btnSelectAV.Sensitive = chkAllowAV.Active;
            txtAVFileName.Sensitive= chkAllowAV.Active;
            txtFileTypes.Sensitive= chkAllowAV.Active;
            txtParameter.Sensitive = chkAllowAV.Active;
        }
    }
}
