using System;
using System.Collections.Generic;
using System.Text;
using MonoDM.Core.Extensions;
using System.Windows.Forms;
using Gtk;
using MonoDM.Core.Common;
using MonoDM.Extension.SpeedLimit.UI;

namespace MonoDM.Extension.SpeedLimit
{
    public class SpeedLimitUIExtension: IUIExtension
    {
        #region IUIExtension Members

        public BaseWidget[] CreateSettingsView()
        {
            return new BaseWidget[] { new LimitCfg() };
        }

        public void PersistSettings(BaseWidget[] settingsView)
        {
            LimitCfg lmt = (LimitCfg)settingsView[0];

            Settings.Default.MaxRate = lmt.MaxRate;
            Settings.Default.EnabledLimit = lmt.EnableLimit;

            Settings.Default.Save();
        }

        #endregion

        public void ShowSpeedLimitDialog()
        {
            using (SetSpeedLimitDialog sld = new SetSpeedLimitDialog())
            {
                if (sld.Run() == (int) ResponseType.Accept)
                {
                    PersistSettings(new BaseWidget[] { sld.LimitCfg });
                }
            }
        }
    }
}