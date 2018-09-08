using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Gtk;
using MonoDM.App.Controls;
using MonoDM.Core;
using MonoDM.Core.Common;

namespace MonoDM.App.UI
{
    public class SegmentList : NodeView
    {
        private NodeStore _store;
        public NodeStore Store => _store ?? (_store = new NodeStore(typeof(SegmentListNode)));

        public SegmentList(Downloader d)
        {
            NodeStore = Store;
            typeof(NodeView).GetField("store",BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(this, Store);
            
            Downloader = d;

            AppendColumn("#", new CellRendererText(), "text", 0);
            AppendColumn("Current Try", new CellRendererText(), "text", 1);
            AppendColumn("Progress", new CellRendererProgress(), "value", 3, "text", 2);
            AppendColumn("Transferred", new CellRendererText(), "text", 4);
            AppendColumn("Size", new CellRendererText(), "text", 5);
            AppendColumn("Status", new CellRendererText(), "text", 6);
            AppendColumn("Transfer Rate", new CellRendererText(), "text", 7);
            AppendColumn("Start", new CellRendererText(), "text", 8);
            AppendColumn("End", new CellRendererText(), "text", 9);
            AppendColumn("Time Left", new CellRendererText(), "text", 10);
            AppendColumn("Current URL", new CellRendererText(), "text", 11);
            
            timer  = new Timer();
            timer.Interval = 50;
            timer.Enabled = true;
            timer.Tick += UpdateSegmentsCallback;
            
            SetSignals();
        }

        private Timer timer;

        public override void Destroy()
        {
            base.Destroy();
            UnsetSignals();
            timer.Dispose();
        }

        public Downloader Downloader { get; set; }

        public void SetSignals()
        {
            Downloader.SegmentStoped += UpdateSegmentsCallback;
            Downloader.SegmentStarted += UpdateSegmentsCallback;
            Downloader.SegmentStarting += UpdateSegmentsCallback;
            Downloader.SegmentFailed+=UpdateSegmentsCallback;
            Downloader.RestartingSegment += UpdateSegmentsCallback;
            
            Downloader.StateChanged += UpdateSegmentsCallback;
            Downloader.InfoReceived += UpdateSegmentsCallback;
            
            timer.Start();
        }


        private void UpdateSegmentsCallback(object sender, EventArgs e)
        {
            UpdateSegments();
        }

        public void UnsetSignals()
        {
            
            Downloader.SegmentStoped -= UpdateSegmentsCallback;
            Downloader.SegmentStarted -= UpdateSegmentsCallback;
            Downloader.SegmentStarting -= UpdateSegmentsCallback;
            Downloader.SegmentFailed -= UpdateSegmentsCallback;
            Downloader.RestartingSegment -= UpdateSegmentsCallback;
            
            Downloader.StateChanged -= UpdateSegmentsCallback;
            Downloader.InfoReceived -= UpdateSegmentsCallback;
        }

        private void UpdateSegmentsWithoutInsert()
        {
            foreach (SegmentListNode item in Store)
            {
                int i = item.SegmentId;
                item.Update(Downloader.Segments[i]);

                //this.blockedProgressBar1.BlockList[i].BlockSize = d.Segments[i].TotalToTransfer;
                //this.blockedProgressBar1.BlockList[i].PercentProgress = (float)d.Segments[i].Progress;
            }

            //this.blockedProgressBar1.Refresh();
        }

        private void UpdateSegmentsInserting()
        {
            Store.Clear();

            List<Block> blocks = new List<Block>();

            for (int i = 0; i < Downloader.Segments.Count; i++)
            {
                Store.AddNode(new SegmentListNode(i, Downloader.Segments[i]));

                //blocks.Add(new Block(d.Segments[i].TotalToTransfer, (float)d.Segments[i].Progress));
            }

            //this.blockedProgressBar1.BlockList = blocks;
        }

        private int Count()
        {
            int result = 0;
            IEnumerator enumerator = Store.GetEnumerator();
                
            while (enumerator.MoveNext()) result++;

            return result;
        }

        private void UpdateSegments()
        {
            try
            {
                if (Downloader != null)
                {
                    Downloader d = Downloader;
                    if (d.Segments.Count == Count())
                    {
                        UpdateSegmentsWithoutInsert();
                    }
                    else
                    {
                        UpdateSegmentsInserting();
                    }
                }
                else
                {
                    Store.Clear();
                }
            }
            finally
            {
                //QueueDraw();
            }
        }     
    }
}