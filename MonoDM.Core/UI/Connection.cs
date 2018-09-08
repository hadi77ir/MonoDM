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
    public class Connection : BaseWidget
    {
        public Connection()
        {
            InitializeComponent();

            numRetryDelay.Value = Core.Settings.Default.RetryDelay;
            numMaxRetries.Value = Core.Settings.Default.MaxRetries;
            numMinSegSize.Value = Core.Settings.Default.MinSegmentSize;
            numMaxSegments.Value = Core.Settings.Default.MaxSegments;

            UpdateControls();
        }

        public override string Text => "Connection";

        void InitializeComponent(){
			Gtk.Table table = new Gtk.Table(4, 2, false);
            // num retry delay
			numRetryDelay = new Gtk.SpinButton(0, 10000, 1);
			numRetryDelay.Digits = 0; //int only
			table.Attach(new Gtk.Label("Delay between retries: "), 0, 1, 0, 1);
			table.Attach(numRetryDelay, 1, 2, 0, 1);

			numMaxRetries = new Gtk.SpinButton(0, 10, 1);
			numMaxRetries.Digits = 0; //int only
			table.Attach(new Gtk.Label("Maximum number of retries: "), 0, 1, 1, 2);
			table.Attach(numMaxRetries, 1, 2, 1, 2);

            numMaxSegments = new SpinButton(0,32,1);
			table.Attach(new Gtk.Label("Maximum number of segments: "), 0, 1, 2, 3);
			table.Attach(numMaxSegments, 1, 2, 2, 3);

			numMinSegSize = new Gtk.SpinButton(100, double.MaxValue, 1);
			table.Attach(new Gtk.Label("Minimum size of a segment (bytes): "), 0, 1, 3, 4);
			table.Attach(numMinSegSize, 1, 2, 3, 4);

			PackStart(table, false,false, 0);
		}

		public Gtk.SpinButton numRetryDelay;
		public Gtk.SpinButton numMaxRetries;
		public Gtk.SpinButton numMinSegSize;
		public Gtk.SpinButton numMaxSegments;

        public int RetryDelay
        {
            get
            {
                return (int)numRetryDelay.Value;
            }
        }

        public int MaxRetries
        {
            get
            {
                return (int)numMaxRetries.Value;
            }
        }

        public int MinSegmentSize
        {
            get
            {
                return (int)numMinSegSize.Value;
            }
        }

        public int MaxSegments
        {
            get
            {
                return (int)numMaxSegments.Value;
            }
        }

        private void numMinSegSize_ValueChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            //lblMinSize.Text = ByteFormatter.ToString((int)numMinSegSize.Value);
        }
    }
}
