using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;
using System.Web;
using Gdk;
using Gtk;
using MonoDM.Core;
using MonoDM.Core.Common;
using MonoDM.Core.UI;
using MonoDM.Extension.AutoDownloads;
using ResourceLocation = MonoDM.Core.ResourceLocation;
using Window = Gtk.Window;

namespace MonoDM.App.UI
{
    public class DownloadList : NodeView
    {
        delegate void ActionDownloader(Downloader d, DownloadListNode item);

        Hashtable mapItemToDownload = new Hashtable();
        Hashtable mapDownloadToItem = new Hashtable();
        Hashtable mapDownloadToDialog = new Hashtable();

        DownloadListNode lastSelection;
        AutoDownloadsExtension scheduler;

        private NodeStore _store;

        private NodeStore Store {
            get
            {
                if(_store == null)
                    _store = new NodeStore(typeof(DownloadListNode));
                return _store;
            }
        }

        private Timer timer;

        public DownloadList()
        {
            NodeStore = Store;
            // working around bug #51688 (https://bugzilla.xamarin.com/show_bug.cgi?id=51688)
            typeof(NodeView).GetField("store",BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(this, Store);
            
            // initialize model
            AppendColumn("File Name", new CellRendererText(), "text", 0);
            AppendColumn("Size", new CellRendererText(), "text", 1);
            AppendColumn("Transferred", new CellRendererText(), "text", 2);
            AppendColumn("Progress", new CellRendererText(), "text", 3);
            AppendColumn("Time Left", new CellRendererText(), "text", 4);
            AppendColumn("Transfer Rate", new CellRendererText(), "text", 5);
            AppendColumn("Date Added", new CellRendererText(), "text", 6);
            AppendColumn("State", new CellRendererText(), "text", 7);
            AppendColumn("Supports Resume", new CellRendererText(), "text", 8);
            AppendColumn("Url", new CellRendererText(), "text", 9);
            AppendColumn("Save To", new CellRendererText(), "text", 10);
            // end initializing model

            for (int i = 0; i < DownloadManager.Instance.Downloads.Count; i++)
            {
                AddDownload(DownloadManager.Instance.Downloads[i]);
            }

            // add events
            Realized += DownloadList_Load;
            Events |= EventMask.ButtonPressMask;
            PopupMenu += PopupMenuAt;
            
            DownloadManager.Instance.DownloadAdded += Instance_DownloadAdded;
            DownloadManager.Instance.DownloadRemoved += Instance_DownloadRemoved;
            DownloadManager.Instance.DownloadEnded += Instance_DownloadEnded;
            ShowAll();

            timer = new Timer{Enabled = true, Interval = 500};
            timer.Elapsed += (sender, args) => UpdateList();
            timer.Start();
            Destroyed += (sender, args) => timer.Stop();
        }

        protected override bool OnButtonPressEvent(EventButton args)
        {
            var res = base.OnButtonPressEvent(args);
            if(args.Button != 3)
                return res;
                
            if (lastSelection == null && NodeSelection.SelectedNodes.Length != 1)
            {
                GetPathAtPos(Convert.ToInt32(args.X), Convert.ToInt32(args.Y), out var treePath, out var viewColumn);
                if (treePath == null)
                    return res;
                
                NodeSelection.SelectPath(treePath);
            }

            PopupMenuAt(args);

            return res;
        }

        private void PopupMenuAt(object o, PopupMenuArgs args)
        {
            PopupMenuAt(null);
        }

        private void PopupMenuAt(EventButton args)
        {
            var menu = new Menu();
            var menuitem = new MenuItem("Open");
            menuitem.Activated += BtnOpenFile_Click;
            menu.Append(menuitem);
            
            menuitem = new MenuItem("Open Folder");
            menuitem.Activated += BtnOpenFolder_Click;
            menu.Append(menuitem);
            
            menuitem = new MenuItem("Properties");
            menuitem.Activated += BtnProperties_Click;
            menu.Append(menuitem);
            menu.ShowAll();
            
            menu.Popup(null, null, null, args?.Button ?? 0, args?.Time ?? 0);
        }

        private void BtnProperties_Click(object sender, EventArgs e)
        {
            using (var dlg = new DownloadPropertiesDialog())
            {
                dlg.SetDownloadData(SelectedDownloaders[0]);
                if (dlg.Run() == (int)ResponseType.Ok)
                {
                    dlg.Destroy();
                }
            }
        }

        private void DownloadList_Load(object sender, EventArgs e)
        {
            DownloadManager.Instance.BeginAddBatchDownloads += Instance_BeginAddBatchDownloads;
            DownloadManager.Instance.EndAddBatchDownloads += Instance_EndAddBatchDownloads;

            scheduler = App.Instance.GetExtensionByType<AutoDownloadsExtension>();
        }

        void Instance_EndAddBatchDownloads(object sender, EventArgs e)
        {
            QueueDraw();
        }

        void Instance_BeginAddBatchDownloads(object sender, EventArgs e)
        {
            
        }

        public void ImportFromTextFile()
        {
            using (FileChooserDialog fc = new FileChooserDialog(
                "Import from text",
                (Window)Toplevel,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept))
            {
                fc.ShowAll();
                if (fc.Run() == (int)ResponseType.Accept)
                {
                    string[] lines = File.ReadAllLines(fc.Filename);
                    
                    using (var batchDlg = new AddMultipleDownloadDialog())
                    {
                        var list = lines.Where(s => ResourceLocation.IsURL(s)).ToArray();
                        foreach (var item in list)
                        {
                            batchDlg.Store.AddNode(new BatchDownloadNode{Checked = true, Url = item});
                        }

                        batchDlg.Run();
                        batchDlg.Destroy();
                    }
                }
                fc.Destroy();
            }
        }
        
        public void ImportFromEF2File()
        {
            Regex r = new Regex(@"^<\n(.*)$\n[^<]*\n^>",RegexOptions.Multiline);
            using (FileChooserDialog fc = new FileChooserDialog("Import from EF2",(Window)Toplevel,FileChooserAction.Open,"Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept))
            {
                fc.ShowAll();
                if (fc.Run() == (int)ResponseType.Accept)
                {
                    string lines = File.ReadAllText(fc.Filename);
                    var m = r.Matches(lines);
                    var list = new List<Match>();
                    for (int i =0;i<m.Count;i++)
                    {
                        list.Add(m[i]);
                    }
                    
                    using (var batchDlg = new AddMultipleDownloadDialog())
                    {   
                        foreach (var item in list)
                        {
                            batchDlg.Store.AddNode(new BatchDownloadNode{Checked = true, Url = item.Groups[0].Value});
                        }

                        batchDlg.Run();
                        batchDlg.Destroy();
                    }
                }
                fc.Destroy();
            }
        }

        public void ExportToTextFile()
        {
            using (FileChooserDialog fc = new FileChooserDialog("Export to text", (Window) Toplevel,
                FileChooserAction.Save, "Cancel", ResponseType.Cancel,
                "Save", ResponseType.Accept))
            {
                fc.ShowAll();
                if (fc.Run() == (int) ResponseType.Accept)
                {
                    string target = fc.Filename;
                    List<string> export = new List<string>();
                    var downloadList = SelectedCount > 0
                        ? (IEnumerable<Downloader>) SelectedDownloaders
                        : DownloadManager.Instance.Downloads;
                    foreach (var downloader in downloadList)
                    {
                        export.Add(downloader.ResourceLocation.URL);
                    }

                    File.WriteAllText(target, string.Join("\n", export));
                }
                fc.Destroy();
            }
        }

        public void ExportToEF2File()
        {
            using (FileChooserDialog fc = new FileChooserDialog("Export to EF2", (Window) Toplevel,
                FileChooserAction.Save, "Cancel", ResponseType.Cancel,
                "Save", ResponseType.Accept))
            {
                fc.ShowAll();
                if (fc.Run() == (int) ResponseType.Accept)
                {
                    string target = fc.Filename;
                    List<string> export = new List<string>();
                    var downloadList = SelectedCount > 0
                        ? (IEnumerable<Downloader>) SelectedDownloaders
                        : DownloadManager.Instance.Downloads;

                    foreach (var downloader in downloadList)
                    {
                        export.Add("<\n");
                        export.Add(downloader.ResourceLocation.URL);
                        export.Add("\n>");
                    }

                    File.WriteAllText(target, string.Join("\n", export));
                }
                fc.Destroy();
            }
        }

        public void StartScheduler(bool enabled)
        {
            scheduler.Active = enabled;
        }

        public bool SchedulerStarted()
        {
            return scheduler.Active;
        }

        public void StartSelections()
        {
            DownloadsAction(
                delegate(Downloader d, DownloadListNode item)
                {
                    if (d.State == DownloaderState.Ended && Math.Abs(d.Progress - 100.0) < 0.000001)
                        return; // do not re-download completed downloads
                    
                    d.Start();
                    ShowProgressDialog(d);
                }
            );
        }

        public void Pause(bool closeDlg)
        {
            DownloadsAction(
                delegate(Downloader d, DownloadListNode item)
                {
                    if(closeDlg)
                        HideProgressDialog(d);
                    
                    d.Pause();
                }
            );
        }

        public void PauseAll(bool closeDlg)
        {
            foreach (var d in DownloadManager.Instance.Downloads)
            {
                if(d.State != DownloaderState.Working) continue;
                if(closeDlg)
                    HideProgressDialog(d);
                
                d.Pause();
            }
            //DownloadManager.Instance.PauseAll();
            UpdateList();
        }

        public void RemoveSelections()
        {
            if(NodeSelection.SelectedNodes.Length == 0)
                return;
            
            using (var dlg = new MessageDialog((Window) Toplevel, 0,
                MessageType.Question,
                ButtonsType.YesNo,
                "Are you sure that you want to remove selected downloads?"))
            {
                dlg.Response += (o, args) =>
                {
                    ((Dialog)o).Destroy();
                };
                if (dlg.Run() == (int) ResponseType.Yes)
                {
                    try
                    {
                        NodeSelection.Changed -= lvwDownloads_ItemSelectionChanged;
                        DownloadManager.Instance.DownloadRemoved -= Instance_DownloadRemoved;

                        DownloadsAction(
                            delegate(Downloader d, DownloadListNode item)
                            {
                                NodeStore.RemoveNode(item);
                                DownloadManager.Instance.RemoveDownload(d);
                            }
                        );
                    }
                    finally
                    {
                        NodeSelection.Changed += lvwDownloads_ItemSelectionChanged;
                        lvwDownloads_ItemSelectionChanged(null, null);

                        DownloadManager.Instance.DownloadRemoved += Instance_DownloadRemoved;
                    }
                }
            }
        }

        public new void SelectAll()
        {
            using (DownloadManager.Instance.LockDownloadList(false))
            {
                try
                {
                    NodeSelection.Changed -= lvwDownloads_ItemSelectionChanged;
                    NodeSelection.SelectAll();
                }
                finally
                {
                    NodeSelection.Changed += lvwDownloads_ItemSelectionChanged;
                    lvwDownloads_ItemSelectionChanged(null, null);
                }
            }
        }

        public void RemoveCompleted()
        {
            DownloadManager.Instance.ClearEnded();
            UpdateList();
        }

        public void AddDownloadURLs(
            ResourceLocation[] args,
            int segments,
            string path,
            int nrOfSubfolders)
        {
            if (args == null) return;

            if (path == null)
            {
                path = PathHelper.GetWithBackslash(Core.Settings.Default.DownloadFolder);
            }
            else
            {
                path = PathHelper.GetWithBackslash(path);
            }

            try
            {
                DownloadManager.Instance.OnBeginAddBatchDownloads();

                foreach (ResourceLocation rl in args)
                {
                    Uri uri = new Uri(rl.URL);

                    string fileName = uri.Segments[uri.Segments.Length - 1];
                    fileName = HttpUtility.UrlDecode(fileName)?.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());

                    DownloadManager.Instance.Add(
                        rl,
                        null,
                        path + fileName,
                        segments,
                        false);
                }
            }
            finally
            {
                DownloadManager.Instance.OnEndAddBatchDownloads();
            }
        }

        private void lvwDownloads_ItemSelectionChanged(object sender, EventArgs e)
        {
            //UpdateSegments();

            UpdateUI();
        }

        public void UpdateUI()
        {
            OnSelectionChange();
        }

        public event EventHandler SelectionChange;

        protected virtual void OnSelectionChange()
        {
            if (SelectionChange != null)
            {
                SelectionChange(this, EventArgs.Empty);
            }
        }

        
        
        public void BtnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedDownloaders[0].Progress.Equals(1.0))
                {
                    OsUtils.OpenFolder(SelectedDownloaders[0].LocalFile);
                }
            }
            catch
            {
            }
        }

        public void BtnOpenFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedDownloaders[0].Progress.Equals(1.0))
                {
                    Process.Start(SelectedDownloaders[0].LocalFile);                    
                }
            }
            catch
            {
            }
        }

        public int SelectedCount => NodeSelection.SelectedNodes.Length; 

        public Downloader[] SelectedDownloaders
        {
            get
            {
                if (NodeSelection.SelectedNodes.Length > 0)
                {
                    Downloader[] downloaders = new Downloader[NodeSelection.SelectedNodes.Length];
                    for (int i = 0; i < downloaders.Length; i++)
                    {
                        downloaders[i] = mapItemToDownload[NodeSelection.SelectedNodes[i]] as Downloader;
                    }
                    return downloaders;
                }

                return null;
            }
        }

        void Instance_DownloadRemoved(object sender, DownloaderEventArgs e)
        {
            Application.Invoke((s, ea) =>
                {
                    if (mapDownloadToItem[e.Downloader] is DownloadListNode item)
                    {
                        if (lastSelection == item)
                        {
                            lastSelection = null;
                        }

                        mapDownloadToItem[e.Downloader] = null;
                        mapItemToDownload[item] = null;

                        NodeStore.RemoveNode(item);
                    }
                }
            );
        }

        void Instance_DownloadAdded(object sender, DownloaderEventArgs e)
        {
            AddDownload(e.Downloader);
        }

        private void AddDownload(Downloader d)
        {
            d.RestartingSegment += download_RestartingSegment;
            d.SegmentStoped += download_SegmentEnded;
            d.SegmentFailed += download_SegmentFailed;
            d.SegmentStarted += download_SegmentStarted;
            d.InfoReceived += download_InfoReceived;
            d.SegmentStarting += Downloader_SegmentStarting;

            string ext = System.IO.Path.GetExtension(d.LocalFile);

            DownloadListNode item = new DownloadListNode(d);

            mapDownloadToItem[d] = item;
            mapItemToDownload[item] = d;

            NodeStore.AddNode(item);
        }

        public static string GetResumeStr(Downloader d)
        {
            return (d.RemoteFileInfo != null && d.RemoteFileInfo.AcceptRanges ? "Yes" : "No");
        }

        public void UpdateList()
        {
            foreach (DownloadListNode item in NodeStore)
            {
                if (item == null) return;
                if (!(mapItemToDownload[item] is Downloader d)) return;

                DownloaderState state;

                if (item.Tag == null) state = DownloaderState.Working;
                else state = (DownloaderState)item.Tag;

                if (state != d.State ||
                    state == DownloaderState.Working ||
                    state == DownloaderState.WaitingForReconnect)
                {
                    item.Update(d);
                }
            }
            

            //UpdateSegments();
        }
   

        private void DownloadsAction(ActionDownloader action)
        {
            if (NodeSelection.SelectedNodes.Length > 0)
            {
                try
                {
                    NodeSelection.Changed -= lvwDownloads_ItemSelectionChanged;

                    for (int i = NodeSelection.SelectedNodes.Length - 1; i >= 0; i--)
                    {
                        DownloadListNode item = (DownloadListNode)NodeSelection.SelectedNodes[i];
                        action((Downloader)mapItemToDownload[item], item);
                    }

                    NodeSelection.Changed += lvwDownloads_ItemSelectionChanged;
                    lvwDownloads_ItemSelectionChanged(null, null);
                }
                finally
                {
                    Application.Invoke((s,e)=>
                    {
                        QueueDraw();
                    });
                    //UpdateSegments();
                }
            }            
        }


        void download_InfoReceived(object sender, EventArgs e)
        {
            Downloader d = (Downloader)sender;
            
            UpdateList();

            Log(
                d,
                String.Format(
                "Connected to: {2}. File size = {0}, Resume = {1}",
                ByteFormatter.ToString(d.FileSize),
                d.RemoteFileInfo.AcceptRanges,
                d.ResourceLocation.URL),
                LogMode.Information);
        }

        void Downloader_SegmentStarting(object sender, SegmentEventArgs e)
        {
            UpdateList();

            Log(
                e.Downloader,
                String.Format(
                "Starting segment for {3}, start position = {0}, end position {1}, segment size = {2}",
                ByteFormatter.ToString(e.Segment.InitialStartPosition),
                ByteFormatter.ToString(e.Segment.EndPosition),
                ByteFormatter.ToString(e.Segment.TotalToTransfer),
                e.Downloader.ResourceLocation.URL),
                LogMode.Information);
        }

        void download_SegmentStarted(object sender, SegmentEventArgs e)
        {
            UpdateList();
            
            Log(
                e.Downloader,
                String.Format(
                "Started segment for {3}, start position = {0}, end position {1}, segment size = {2}",
                ByteFormatter.ToString(e.Segment.InitialStartPosition),
                ByteFormatter.ToString(e.Segment.EndPosition),
                ByteFormatter.ToString(e.Segment.TotalToTransfer),
                e.Downloader.ResourceLocation.URL),
                LogMode.Information);
        }

        void download_SegmentFailed(object sender, SegmentEventArgs e)
        {
            UpdateList();

            Log(
                e.Downloader,
                String.Format(
                "Download segment ({0}) failed for {2}, reason = {1}",
                e.Segment.Index,
                e.Segment.LastError.Message,
                e.Downloader.ResourceLocation.URL),
                LogMode.Error);
        }

        void download_SegmentEnded(object sender, SegmentEventArgs e)
        {
            UpdateList();

            Log(
                e.Downloader,
                String.Format(
                "Download segment ({0}) ended for {1}",
                e.Segment.Index,
                e.Downloader.ResourceLocation.URL),
                LogMode.Information);
        }

        void download_RestartingSegment(object sender, SegmentEventArgs e)
        {
            UpdateList();

            Log(
                e.Downloader,
                String.Format(
                "Download segment ({0}) is restarting for {1}",
                e.Segment.Index,
                e.Downloader.ResourceLocation.URL),
                LogMode.Information);
        }

        void Instance_DownloadEnded(object sender, DownloaderEventArgs e)
        {
            UpdateList();
            
            // close download progress dialog
            HideProgressDialog(e.Downloader);
            
            // check if should show finish dialog
            if (e.Downloader.State != DownloaderState.EndedWithError)
            {
                if (MonoDM.App.Settings.Default.ShowFinishedDialog)
                {
                    using (DownloadFinishedDialog dlg = new DownloadFinishedDialog())
                    {
                        dlg.Url = e.Downloader.ResourceLocation.URL;
                        dlg.SaveTo = e.Downloader.LocalFile;
                        dlg.Downloader = e.Downloader;
                        dlg.RefreshUI();
                        dlg.Run();
                    }
                }
            }
            else
            {
                GtkUtils.ShowMessageBox(null, "Error downloading " + System.IO.Path.GetFileName(e.Downloader.LocalFile) + "\n"+e.Downloader.LastError?.Message,
                    MessageType.Error);
            }

            Log(
                e.Downloader,
                String.Format(
                "Download ended {0}",
                e.Downloader.ResourceLocation.URL),
                LogMode.Information);
        }

        enum LogMode
        {
            Error,
            Information
        }

        void Log(Downloader downloader, string msg, LogMode m)
        {
            try
            {
                Application.Invoke(
                    (s, ea)=>
                  {
                      File.AppendAllText(GetLogFile(downloader.LocalFile), string.Format("[{0}] {1} {2}\n",
                          DateTime.Now , (m == LogMode.Error ? "E/" : "I/"),  msg));
                  }
              );
            }
            catch { }
        }

        private string GetLogFile(string downloaderLocalFile)
        {
            return "download_log_"+System.IO.Path.GetFileName(downloaderLocalFile)+ DateTime.Today.ToFileTime() + ".txt";
        }

        public void BtnNewDownload_Click(object sender, EventArgs e)
        {
            //NewFileDownload(null, true);
            AddNewFromURL();
        }

        public void BtnStartSelected_Click(object sender, EventArgs e)
        {
            StartSelections();
        }

        public void BtnPause_Click(object sender, EventArgs e)
        {
            Pause(false);
        }

        public void BtnRemoveSelected_Click(object sender, EventArgs e)
        {
            RemoveSelections();
        }

        private void OnPopupMenuOpening(object sender, CancelEventArgs e)
        {
            UpdateUI();
        }

        public void OnNodeDoubleClick(object sender, EventArgs e)
        {
            UpdateUI();

            //openFileToolStripMenuItem_Click(sender, e);
            BtnOpenFile_Click(sender,e);
        }

        public void StopAll()
        {
            PauseAll(true);
        }
        public void AddNewFromURL()
        {
            using (AddUrlDialog dd = new AddUrlDialog())
            {
                if (dd.Run() == (int) ResponseType.Ok && dd.ResourceLocation != null)
                {
                    using (AddDownloadDialog dlg = new AddDownloadDialog())
                    {
                        dlg.DownloadLocation = dd.ResourceLocation;
                        ResponseType t = (ResponseType)dlg.Run();
                        if (t == ResponseType.Ok)
                        {
                            var d = dlg.AddDownload(false);
                            ShowProgressDialog(d);
                        }else if (t == ResponseType.Apply)
                        {
                            dlg.AddDownload(true);
                        }
                        
                        dlg.Destroy();
                    }
                }
            }
        }

        public void AddNewFromURL(string s)
        {
            if(Uri.TryCreate(s, UriKind.Absolute, out Uri uri))
                using (AddUrlDialog dd = new AddUrlDialog())
                {
                    dd.Url = uri;
                    if (dd.Run() == (int) ResponseType.Ok)
                    {
                        using (AddDownloadDialog dlg = new AddDownloadDialog())
                        {
                            dlg.DownloadLocation = dd.ResourceLocation;
                            ResponseType t = (ResponseType)dlg.Run();
                            if (t == ResponseType.Ok)
                            {
                                var d = dlg.AddDownload(false);
                                ShowProgressDialog(d);
                            }else if (t == ResponseType.Apply)
                            {
                                dlg.AddDownload(true);
                            }

                            dlg.Destroy();
                        }
                    }
                }
        }

        public void ShowProgressDialog(Downloader d)
        {
            if (d.State == DownloaderState.Ended)
                return; // only show progress dialog for uncompleted downloads
            
            GetDownloadProgressWindow(d,true).Show();
        }

        public void HideProgressDialog(Downloader d)
        {
            GetDownloadProgressWindow(d,false).Hide();
        }

        private DownloadProgressWindow GetDownloadProgressWindow(Downloader d, bool createIfNotExists)
        {
            // check if there is a dialog
            if (!mapDownloadToDialog.ContainsKey(d) && createIfNotExists)
            {
                mapDownloadToDialog[d] = new DownloadProgressWindow(d);
            }

            return (DownloadProgressWindow)mapDownloadToDialog[d];
        }
    }
}
