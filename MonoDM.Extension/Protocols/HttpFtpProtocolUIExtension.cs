using System;
using System.Collections.Generic;
using System.Text;
using MonoDM.Core.Extensions;
using System.Windows.Forms;
using Gtk;
using MonoDM.Core.Common;
using MonoDM.Extension.Protocols.UI;

namespace MonoDM.Extension.Protocols
{
    public class HttpFtpProtocolUIExtension : IUIExtension
    {
        #region IUIExtension Members

        public BaseWidget[] CreateSettingsView()
        {
            return new BaseWidget[] { new Proxy() };
        }

        public void PersistSettings(BaseWidget[] settingsView)
        {
            Proxy proxy = (Proxy)settingsView[0];

            Settings.Default.UseProxy = proxy.UseProxy;
            Settings.Default.ProxyAddress = proxy.ProxyAddress;
            Settings.Default.ProxyPort = proxy.ProxyPort;
            Settings.Default.ProxyByPassOnLocal = proxy.ProxyByPassOnLocal;
            Settings.Default.ProxyUserName = proxy.ProxyUserName;
            Settings.Default.ProxyPassword = proxy.ProxyPassword;
            Settings.Default.Save();
        }

        #endregion
    }
}
