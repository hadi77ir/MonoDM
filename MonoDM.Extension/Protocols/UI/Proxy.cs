using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using Gtk;
using MonoDM.Core.Common;

namespace MonoDM.Extension.Protocols.UI
{
    public class Proxy : BaseWidget
    {
        public Proxy()
        {
            InitializeComponent();

            UseProxy = Protocols.Settings.Default.UseProxy;
            ProxyAddress = Protocols.Settings.Default.ProxyAddress;
            ProxyPort = Protocols.Settings.Default.ProxyPort;
            ProxyByPassOnLocal = Protocols.Settings.Default.ProxyByPassOnLocal;
            ProxyUserName = Protocols.Settings.Default.ProxyUserName;
            ProxyPassword = Protocols.Settings.Default.ProxyPassword;

            UpdateControls();
        }

        void InitializeComponent()
        {
            var box = new VBox();
            var bbox = new HButtonBox();
            
            _btnGetFromEnv = new Button("Get from environment");
            _btnGetFromEnv.Activated += BtnGetFromEnvOnActivated;
            bbox.PackEnd(_btnGetFromEnv, false, false, 0);
            
            box.PackStart(bbox, false, false, 0);
            _cbUseProxy = new CheckButton("Use proxy?");
            _cbUseProxy.Toggled += chkUseProxy_CheckedChanged;
            _cbBypassLocal = new CheckButton("Bypass local addresses?");
            box.PackStart(_cbUseProxy, false, false, 0);
            box.PackStart(_cbBypassLocal, false, false, 0);
            Table table = new Table(2, 4, false);

            table.Attach(new Label("Host"), 0, 1, 0, 1);
            table.Attach(new Label("Port"), 1, 2, 0, 1);
            table.Attach(new Label("Username"), 2, 3, 0, 1);
            table.Attach(new Label("Password"), 3, 4, 0, 1);
            
            _txtProxyHost = new Entry();
            _txtProxyPort = new SpinButton(0,65535,1);
            _txtProxyUser = new Entry();
            _txtProxyPassword = new Entry();
            _txtProxyPassword.Visibility = false;
            
            table.Attach(_txtProxyHost, 0, 1, 1, 2);
            table.Attach(_txtProxyPort, 1, 2, 1, 2);
            table.Attach(_txtProxyUser, 2, 3, 1, 2);
            table.Attach(_txtProxyPassword, 3, 4, 1, 2);
            box.PackStart(table, false, false, 0);
            
            
            Child = box;
            ShowAll();
        }

        private void BtnGetFromEnvOnActivated(object sender, EventArgs e)
        {
            var d = Environment.GetEnvironmentVariables();
            if (string.IsNullOrEmpty((string)d["HTTP_PROXY"]))
            {
                UseProxy = true;
                string p = (string)d["HTTP_PROXY"];
                if (!p.Contains("://"))
                {
                    p = "http://" + p;
                }
                Uri u = new Uri(p);
                ProxyAddress = u.Host;
                ProxyPort = u.Port;
                var uinfo = u.UserInfo.Split(':');
                ProxyUserName = uinfo[0];
                uinfo[0] = "";
                ProxyPassword = string.Join(":", uinfo).TrimStart(':');
            }
        }

        private Button _btnGetFromEnv;
        private CheckButton _cbUseProxy;

        private CheckButton _cbBypassLocal;

        private Entry _txtProxyHost;
        private SpinButton _txtProxyPort;
        private Entry _txtProxyUser;
        private Entry _txtProxyPassword;


        public bool UseProxy
        {
            get { return _cbUseProxy.Active; }
            set { _cbUseProxy.Active = value; }
        }

        public string ProxyAddress
        {
            get { return _txtProxyHost.Text; }
            set { _txtProxyHost.Text = value; }
        }

        public int ProxyPort
        {
            get { return (int) _txtProxyPort.Value; }
            set { _txtProxyPort.Value = value; }
        }

        public bool ProxyByPassOnLocal
        {
            get { return _cbBypassLocal.Active; }
            set { _cbBypassLocal.Active = value; }
        }

        public string ProxyUserName
        {
            get { return _txtProxyUser.Text; }
            set { _txtProxyUser.Text = value; }
        }

        public string ProxyPassword
        {
            get { return _txtProxyPassword.Text; }
            set { _txtProxyPassword.Text = value; }
        }

        public override string Text => "Proxy";

        private void chkUseProxy_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            _cbBypassLocal.Sensitive = UseProxy;
            _txtProxyHost.Sensitive = UseProxy;
            _txtProxyPort.Sensitive = UseProxy;
            _txtProxyUser.Sensitive = UseProxy;
            _txtProxyPassword.Sensitive = UseProxy;
        }
    }
}