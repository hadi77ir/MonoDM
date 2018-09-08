using System;
using MonoDM.Core;
using MonoDM.Core.Common;

namespace MonoDM.App.UI
{
    [Gtk.TreeNode(ListOnly = true), Serializable]
    public class SegmentListNode : Gtk.TreeNode
    {

        public SegmentListNode(int segmentId)
        {
            SegmentId = segmentId;
        }
        public SegmentListNode(int segmentId, Segment s)
        {
            SegmentId = segmentId;
            Fill(s);
        }
        public SegmentListNode()
        {
            
        }
        [Gtk.TreeNodeValue(Column = 0)]
        public int SegmentId { get; set; }
        [Gtk.TreeNodeValue(Column = 1)]
        public int CurrentTry { get; set; }
        [Gtk.TreeNodeValue(Column = 2)]
        public string Progress { get; set; }
        [Gtk.TreeNodeValue(Column = 3)]
        public int ProgressInt { get; set; }
        [Gtk.TreeNodeValue(Column = 4)]
        public string Transferred { get; set; }
        [Gtk.TreeNodeValue(Column = 5)]
        public string TotalToTransfer { get; set; }
        [Gtk.TreeNodeValue(Column = 6)]
        public string StateMessage { get; set; }
        [Gtk.TreeNodeValue(Column = 7)]
        public string TransferRate { get; set; }
        [Gtk.TreeNodeValue(Column = 8)]
        public string InitialStartPosition { get; set; }
        [Gtk.TreeNodeValue(Column = 9)]
        public string EndPosition { get; set; }
        [Gtk.TreeNodeValue(Column = 10)]
        public string TimeLeft { get; set; }
        [Gtk.TreeNodeValue(Column = 11)]
        public string CurrentUrl { get; set; }
        
        private void Fill(Segment s) {
            CurrentTry = s.CurrentTry;
            Progress = String.Format("{0:0.##}%", s.Progress);
            ProgressInt = (int)s.Progress;
            Transferred = ByteFormatter.ToString(s.Transfered);
            TotalToTransfer = ByteFormatter.ToString(s.TotalToTransfer);
            InitialStartPosition = ByteFormatter.ToString(s.InitialStartPosition);
            EndPosition = ByteFormatter.ToString(s.EndPosition);
            TransferRate = TransferRateFormatter.ToString(s.Rate);
            TimeLeft = TimeSpanFormatter.ToString(s.Left);
            if (s.LastError != null)
            {
                StateMessage = s.State.ToString() + ", " + s.LastError.Message;
            }
            else
            {
                StateMessage = s.State.ToString();
            }
            CurrentUrl = s.CurrentURL;
        }

        public void Update()
        {
            this.OnChanged();
        }

        public void Update(Segment s)
        {
            Fill(s);
            this.OnChanged();
        }
    }
}