using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using MonoDM.Core.Extensions;
using MonoDM.Core;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using MonoDM.Core.Instrumentation;
using System.Diagnostics;

namespace MonoDM.Extension.PersistedList
{
    public enum PersistState
    {
        SaveFromDispose,
        SaveFromTimer,
        SaveFromListChange
    }

    public class PersistedListExtension: IExtension, IDisposable
    {
        [Serializable]
        public class DownloadItem
        {
            public ResourceLocation rl;

            public ResourceLocation[] mirrors;

            [XmlAttribute("lf")]
            public string LocalFile;

            public RemoteFileInfo remoteInfo;

            [XmlAttribute("segCnt")]
            public int requestedSegments;

            [XmlAttribute("dt")]
            public DateTime createdDateTime;

            public SegmentItem[] Segments;

            public SerializableDictionary<string, object> extendedProperties;
        }

        [Serializable]
        public class SegmentItem
        {
            [XmlAttribute("i")]
            public int Index;

            [XmlAttribute("isp")]
            public long InitialStartPositon;

            [XmlAttribute("sp")]
            public long StartPositon;

            [XmlAttribute("ep")]
            public long EndPosition;
        }

        private const int SaveListIntervalInSeconds = 120;

        private XmlSerializer serializer;
        private System.Threading.Timer timer;

        #region IExtension Members

        public string Name
        {
            get { return "Persisted Download List"; }
        }

        public IUIExtension UIExtension
        {
            get { return null; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            DownloadManager.Instance.PauseAll();

            PersistList(PersistState.SaveFromDispose);
        }

        #endregion

        #region Methods

        private void PersistList(object state)
        {
            List<DownloadItem> downloadsToSave = new List<DownloadItem>();

            using (DownloadManager.Instance.LockDownloadList(false))
            {
                IList<Downloader> downloads = DownloadManager.Instance.Downloads;

                for (int i = 0; i < downloads.Count; i++)
                {
                    // don't save ended downloads
                    /* if (downloads[i].State == DownloaderState.Ended)
                    {
                        continue;
                    } */
                                        
                    Downloader downloader = downloads[i];

                    DownloadItem di = new DownloadItem();
                    di.LocalFile = downloader.LocalFile;
                    di.rl = downloader.ResourceLocation;
                    di.mirrors = downloader.Mirrors.ToArray();
                    di.remoteInfo = downloader.RemoteFileInfo;
                    di.requestedSegments = downloader.RequestedSegments;
                    di.createdDateTime = downloader.CreatedDateTime;
                    di.extendedProperties = new SerializableDictionary<string,object>(downloader.ExtendedProperties);

                    using (downloader.LockSegments())
                    {
                        di.Segments = new SegmentItem[downloader.Segments.Count];

                        for (int j = 0; j < downloader.Segments.Count; j++)
                        {
                            SegmentItem si = new SegmentItem();
                            Segment seg = downloader.Segments[j];

                            si.Index = seg.Index;
                            si.InitialStartPositon = seg.InitialStartPosition;
                            si.StartPositon = seg.StartPosition;
                            si.EndPosition = seg.EndPosition;

                            di.Segments[j] = si;
                        }
                    }

                    downloadsToSave.Add(di);                    
                }
            }

            SaveObjects(downloadsToSave);
        }

        private string GetDatabaseFile()
        {
            string file = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + Path.DirectorySeparatorChar + "downloads.xml";
            return file;
        }

        private void LoadSavedList()
        {
            if (File.Exists(GetDatabaseFile()))
            {
                try
                {
                    using (FileStream fs = new FileStream(GetDatabaseFile(), FileMode.Open))
                    {
                        DownloadItem[] downloads = (DownloadItem[])serializer.Deserialize(fs);

                        LoadPersistedObjects(downloads);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        private void SaveObjects(List<DownloadItem> downloadsToSave)
        {
            using (new MyStopwatch("Saving download list"))
            {
                try
                {
                    using (FileStream fs = new FileStream(GetDatabaseFile(), FileMode.Create))
                    {
                        serializer.Serialize(fs, downloadsToSave.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        private static void LoadPersistedObjects(DownloadItem[] downloads)
        {
            for (int i = 0; i < downloads.Length; i++)
            {
                List<Segment> segments = new List<Segment>();

                for (int j = 0; j < downloads[i].Segments.Length; j++)
                {
                    Segment seg = new Segment();
                    seg.Index = downloads[i].Segments[j].Index;
                    seg.InitialStartPosition = downloads[i].Segments[j].InitialStartPositon;
                    seg.StartPosition = downloads[i].Segments[j].StartPositon;
                    seg.EndPosition = downloads[i].Segments[j].EndPosition;

                    segments.Add(seg);
                }

                Downloader d = DownloadManager.Instance.Add(
                    downloads[i].rl,
                    downloads[i].mirrors,
                    downloads[i].LocalFile,
                    segments,
                    downloads[i].remoteInfo,
                    downloads[i].requestedSegments,
                    false,
                    downloads[i].createdDateTime);

                if (downloads[i].extendedProperties != null)
                {
                    SerializableDictionary<string, object>.Enumerator e = downloads[i].extendedProperties.GetEnumerator();

                    while (e.MoveNext())
                    {
                        d.ExtendedProperties.Add(e.Current.Key, e.Current.Value);
                    }
                }
            }
        } 

        private void PersistOnListChange(object sender, EventArgs e)
        {
            PersistList(PersistState.SaveFromListChange);
        }
        
        #endregion

        #region Constructor

        public PersistedListExtension()
        {
            serializer = new XmlSerializer(typeof(DownloadItem[]));

            LoadSavedList();
            
            // make a timer for saving at some time
            TimerCallback refreshCallBack = new TimerCallback(PersistList);
            TimeSpan refreshInterval = TimeSpan.FromSeconds(SaveListIntervalInSeconds);
            timer = new Timer(refreshCallBack, PersistState.SaveFromTimer, new TimeSpan(-1), refreshInterval);
            
            // persist when list changed
            DownloadManager.Instance.DownloadAdded += PersistOnListChange;
            DownloadManager.Instance.DownloadEnded+= PersistOnListChange;
            DownloadManager.Instance.DownloadRemoved += PersistOnListChange;
            DownloadManager.Instance.EndAddBatchDownloads += PersistOnListChange;
        }

        #endregion
    }
}