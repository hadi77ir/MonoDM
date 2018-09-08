using System;
using MonoDM.Core;
using MonoDM.Core.Common;
namespace MonoDM.App.UI
{
    [Gtk.TreeNode(ListOnly = true), Serializable]
    public class DownloadListNode : Gtk.TreeNode
    {
        public DownloadListNode()
        {
        }
        public DownloadListNode(Downloader d)
        {
            Fill(d);
        }
        void Fill(Downloader d)
        {
            FileName = System.IO.Path.GetFileName(d.LocalFile);
            // size
            FileSize = ByteFormatter.ToString(d.FileSize);
            // completed
            Transferred = ByteFormatter.ToString(d.Transfered);
            // progress
            Progress = String.Format("{0:0.##}%", d.Progress);
            // left
            TimeLeft = TimeSpanFormatter.ToString(d.Left);
            // rate
            TransferRate = String.Format("{0:0.##}", d.Rate / 1024.0);
            // added
            DateAdded = d.CreatedDateTime.ToShortDateString() + " " + d.CreatedDateTime.ToShortTimeString();
            // resume
            SupportsResume = DownloadList.GetResumeStr(d);
            // url
            Url = d.ResourceLocation.URL;

            SaveTo = d.LocalFile;
            
            if (d.LastError != null)
            {
                StateMessage = d.State.ToString() + ", " + d.LastError.Message;
            }
            else
            {
                if (String.IsNullOrEmpty(d.StatusMessage))
                {
                    StateMessage = d.State.ToString();
                }
                else
                {
                    StateMessage = d.State.ToString() + ", " + d.StatusMessage;
                }
            }
            
            Tag = d.State;
        }
        public void Update(){
            // trigger node update
            this.OnChanged();
        }
        public void Update(Downloader d)
        {
            Fill(d);

            // trigger node update
            this.OnChanged();
        }

        [Gtk.TreeNodeValue(Column = 0)]
        public string FileName { get; set; }

        [Gtk.TreeNodeValue(Column = 1)]
        public string FileSize { get; set; }

        [Gtk.TreeNodeValue(Column = 2)]
        public string Transferred { get; set; }

        [Gtk.TreeNodeValue(Column = 3)]
        public string Progress { get; set; }

        [Gtk.TreeNodeValue(Column = 4)]
        public string TimeLeft { get; set; }

        [Gtk.TreeNodeValue(Column = 5)]
        public string TransferRate { get; set; }

        [Gtk.TreeNodeValue(Column = 6)]
        public string DateAdded { get; set; }

        [Gtk.TreeNodeValue(Column = 7)]
        public string StateMessage { get; set; }

        [Gtk.TreeNodeValue(Column = 8)]
        public string SupportsResume { get; set; }

        [Gtk.TreeNodeValue(Column = 9)]
        public string Url { get; set; }

        [Gtk.TreeNodeValue(Column = 10)]
        public string SaveTo { get; set; }

        public object Tag { get; set; }
    }

}
