using System;
using System.Collections.Generic;
using System.Text;
using MonoDM.Core.Extensions;
using Gtk;
using MonoDM.Core.Common;

namespace MonoDM.Core.UI
{
    public class CoreUIExtention: IUIExtension
    {
        #region IUIExtension Members

        public BaseWidget[] CreateSettingsView()
        {
            return new BaseWidget[] { new Connection(), new DownloadFolder() };
        }

        public void PersistSettings(BaseWidget[] settingsView)
        {
            Connection connection = (Connection)settingsView[0];
            DownloadFolder downloadFolder = (DownloadFolder)settingsView[1];

            Settings.Default.MaxRetries = connection.MaxRetries;
            Settings.Default.MinSegmentSize = connection.MinSegmentSize;
            Settings.Default.RetryDelay = connection.RetryDelay;
            Settings.Default.MaxSegments = connection.MaxSegments;

            Settings.Default.DownloadFolder = downloadFolder.Folder;

            Settings.Default.Save();
        }

        #endregion
    }
}
