using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gtk;

namespace MonoDM.Extension.SpeedLimit.UI
{
    public class SetSpeedLimitDialog : Gtk.Dialog
    {
        public SetSpeedLimitDialog()
        {
            InitializeComponent();
        }

        public LimitCfg LimitCfg;
        void InitializeComponent()
        {
            DefaultResponse = ResponseType.Cancel;

            AddButton("OK", ResponseType.Accept);
            AddButton("Cancel", ResponseType.Cancel);

            LimitCfg = new LimitCfg();
            
            VBox.PackStart(LimitCfg);
            ShowAll();
        }
        
    }
}