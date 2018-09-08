using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using Gtk;
using MonoDM.Core.Common;

namespace MonoDM.Extension.SpeedLimit.UI
{
    public partial class LimitCfg : BaseWidget
    {
        public LimitCfg()
        {
            InitializeComponent();
            
            chkEnableLimit.Active = SpeedLimit.Settings.Default.EnabledLimit;
            numMaxRate.Value = Math.Max(SpeedLimit.Settings.Default.MaxRate, 1024) / 1024.0;
            
            UpdateUI();
        }

        private CheckButton chkEnableLimit;
        private SpinButton numMaxRate;
        private void InitializeComponent()
        {
            chkEnableLimit = new CheckButton("Enable?");
            chkEnableLimit.StateChanged += ChkEnableLimitOnStateChanged;
            var tbl = new Table(2,2,true);
            tbl.Attach(chkEnableLimit, 0,1,0,1);
            numMaxRate = new SpinButton(1, Double.MaxValue, 0.25);
            numMaxRate.Digits = 3;
            tbl.Attach(new Label("Max transfer rate (KB/s):"), 0,1,1,2);
            tbl.Attach(numMaxRate, 1,2,1,2);

            PackStart(tbl, false,false, 0);
            ShowAll();
        }

        private void ChkEnableLimitOnStateChanged(object o, StateChangedArgs args)
        {
            UpdateUI();
        }

        public override string Text => "Speed Limit";

        public double MaxRate
        {
            get { return (numMaxRate.Value) * 1024; }
        }

        public bool EnableLimit
        {
            get { return chkEnableLimit.Active; }
        }

        private void UpdateUI()
        {
            numMaxRate.Sensitive = chkEnableLimit.Active;
        }
    }
}
