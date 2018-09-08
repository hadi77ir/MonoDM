using System;
using System.Collections.Generic;
using System.Text;
using MonoDM.Core.Extensions;
using System.Windows.Forms;
using MonoDM.Core.Common;
using MonoDM.Extension.AntiVirus.UI;

namespace MonoDM.Extension.AntiVirus
{
    public class AntiVirusUIExtension: IUIExtension
    {
        #region IUIExtension Members

        public BaseWidget[] CreateSettingsView()
        {
            return new BaseWidget[] { new AVOptions() }; 
        }

        public void PersistSettings(BaseWidget[] settingsView)
        {
            AVOptions options = (AVOptions)settingsView[0];
            Settings.Default.AVParameter = options.AVParameter;
            Settings.Default.CheckFileWithAV = options.CheckFileWithAV;
            Settings.Default.FileTypes = options.FileTypes;
            Settings.Default.AVFileName = options.AVFileName;
            Settings.Default.Save();
        }

        #endregion
    }
}
