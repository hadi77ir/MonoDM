using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using MonoDM.Core.Common;
using Gtk;
namespace MonoDM.Extension.AutoDownloads.UI
{
    public class Jobs : BaseWidget
    {
        public Jobs()
        {
            InitializeComponent();

            _numMaxJobs.Value = AutoDownloads.Settings.Default.MaxJobs;
            _cbUseTime.Active = AutoDownloads.Settings.Default.WorkOnlyOnSpecifiedTimes;
            _timeGridWidget.SelectedTimes = new DayHourMatrix(AutoDownloads.Settings.Default.TimesToWork);
            _numMaxRate.Value = Math.Max(AutoDownloads.Settings.Default.MaxRateOnTime, 1024) / 1024.0;
            _cbAutoStart.Active = AutoDownloads.Settings.Default.AutoStart;

            UpdateUI();
        }

        public override string Text => "Auto-Downloads";

        private void InitializeComponent()
        {
            var box = new VBox();
            
            _cbAutoStart = new CheckButton("Auto-start?");
            box.PackStart(_cbAutoStart, false,false, 0);

            _cbUseTime = new CheckButton("Work only at specified times");
            box.PackStart(_cbUseTime, false,false, 0);
            
            _timeGridWidget = new TimeGridWidget();
            box.PackStart(_timeGridWidget, true,true, 0);
            
            _numMaxRate = new SpinButton(1, Double.MaxValue, 0.25);
            _numMaxRate.Digits = 3;
            var tbl = new Table(2,2,true);
            tbl.Attach(new Label("Max download rate (KB/s):"), 0,1,0,1);
            tbl.Attach(_numMaxRate, 1,2,0,1);

            _numMaxJobs = new SpinButton(1, Double.MaxValue, 1);
            _numMaxJobs.Digits = 1;
            tbl.Attach(new Label("Max simultaneous jobs:"), 0,1,1,2);
            tbl.Attach(_numMaxJobs, 1,2,1,2);
            box.PackStart(tbl, false, false, 0);
            
            PackStart(box, false,false, 0);
            ShowAll();

            _cbUseTime.Toggled += (s,ea) => UpdateUI();
        }
        
        // times
        private TimeGridWidget _timeGridWidget;
        // autostart (is this extension enabled?)
        private CheckButton _cbAutoStart;
        // work only in specified times
        private CheckButton _cbUseTime;
        // max rate in kb/s
        private SpinButton _numMaxRate;
        // max jobs
        private SpinButton _numMaxJobs;

        private void chkUseTime_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            _cbAutoStart.Sensitive = _cbUseTime.Active;
            _numMaxJobs.Sensitive = _cbUseTime.Active;
            _numMaxRate.Sensitive = _cbUseTime.Active;
            _timeGridWidget.Sensitive = _cbUseTime.Active;
        }

        public int MaxJobs
        {
            get { return (int)_numMaxJobs.Value; }
        }

        public double MaxRate
        {
            get { return ((double)_numMaxRate.Value) * 1024; }
        }

        public bool WorkOnlyOnSpecifiedTimes
        {
            get { return _cbUseTime.Active; }
        }

        public bool AutoStart
        {
            get { return _cbAutoStart.Active; }
        }

        public string TimesToWork
        {
            get { return _timeGridWidget.SelectedTimes.ToString(); }
        }	
    }
}
