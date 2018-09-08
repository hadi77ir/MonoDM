using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Gdk;
using Gtk;

using MonoDM.App;
using MonoDM.App.UI;

using MonoDM.Core;
using MonoDM.Core.Common;
using MonoDM.Core.Extensions;
using MonoDM.Core.UI;

using MonoDM.IPC;
using MonoDM.IPC.SingleInstancing;

namespace MonoDM.App
{
    [Serializable]
    public class App : IApp
    {
        #region Singleton

        private static App instance = new App();

        public static App Instance
        {
            get
            {
                return instance;
            }
        }

        private App()
        {
            AppManager.Instance.Initialize(this);

			extensions = ExtensionsManager.LoadAllExtensions(System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        #endregion

        #region Fields
        
        private List<IExtension> extensions;
        private SingleInstanceTracker<IPCConnectionWebSocket> tracker = null;
        private bool disposed = false;

        #endregion

        #region Properties

        public BaseWindow MainWindow
        {
            get; internal set;
        }

        public StatusIcon TrayIcon
        {
            get; internal set;
        }

        public List<IExtension> Extensions
        {
            get
            {
                return extensions;
            }
        } 

        #endregion

        #region Methods

        public IExtension GetExtensionByType(Type type)
        {
            for (int i = 0; i < this.extensions.Count; i++)
            {
                if (this.extensions[i].GetType() == type)
                {
                    return this.extensions[i];
                }
            }

            return null;
        }

        public T GetExtensionByType<T>() where T : IExtension
        {
            var type = typeof(T);
            for (int i = 0; i < this.extensions.Count; i++)
            {
                if (this.extensions[i].GetType() == type)
                {
                    return (T)this.extensions[i];
                }
            }

            return default(T);
        }

        public void InitExtensions()
        {
            for (int i = 0; i < Extensions.Count; i++)
            {
                if (Extensions[i] is IInitializable)
                {
                    ((IInitializable)Extensions[i]).Init();                   
                }
            }

        }
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                for (int i = 0; i < Extensions.Count; i++)
                {
                    if (Extensions[i] is IDisposable)
                    {
                        try
                        {
                            ((IDisposable)Extensions[i]).Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }
        public void Quit()
        {
            // stop all downloads

            // store any pending changes

            // dispose
            MainWindow.Dispose();
            TrayIcon.Dispose();
            tracker.IPC.Dispose();

            // quit
            Application.Quit();
        }
        internal StatusIcon CreateTray(Pixbuf icon){
            // Creation of the Icon
            var trayIcon = new StatusIcon(icon);

            // Show/Hide the window (even from the Panel/Taskbar) when the TrayIcon has been clicked.
            EventHandler restoreLambda = (s,e) => { MainWindow.Show(); };

            trayIcon.Activate += restoreLambda; 
            // Add menu to tray icon
            Menu menu = new Menu();

            // Add menu items
            MenuItem item;
            
            item = new MenuItem("Restore");
            item.Activated += restoreLambda;
            menu.Add(item);

            item = new MenuItem("Exit");
            item.Activated += (sender, e) => {
                Quit();
            };
            menu.Add(item);


            trayIcon.PopupMenu += (sender, e) => {
                menu.ShowAll();
                menu.Popup();
            };

            // A Tooltip for the Icon
            trayIcon.Tooltip = "MonoDM";
            trayIcon.Visible = true;

            return trayIcon;
        }
        public void Start(string[] args)
        {
            // Initialize IPC
            try
            {
                tracker = new SingleInstanceTracker<IPCConnectionWebSocket>();
                tracker.MessageReceived += (sender, e) => {
                    if(e.Message.Parameters.Length > 0 && e.Message.Parameters[0] == "newargs"){
                        MainWindow.OnArgsReceived(e.Message.Parameters.Skip(1).ToArray());
                    }
                };
                if(tracker.IsFirstInstance)
                {
                    // Initialize GTK#
                    Application.Init();

                    TrayIcon = CreateTray(new Pixbuf(System.IO.File.ReadAllBytes("icon.png")));

                    // Create a Window with title
                    MainWindow = new MainWindow();

                    // Show the main window and start the application.
                    MainWindow.ShowAll();

                    if (Array.IndexOf<string>(args, "/as") >= 0)
                    {
                        MainWindow.Visible = false;
                    }

                    MainWindow.Realized += (sender, ea) =>
                    {
                        InitExtensions();

                        if (args.Length > 0)
                        {
                            ((MainWindow)MainWindow).OnArgsReceived(args);
                        }
                    };

                    MainWindow.DeleteEvent += (sender, ea) =>
                    {
                        MainWindow.HideOnDelete();
                    };
                    MainWindow.Destroyed += (sender, eventArgs) => { Settings.Default.Save(); Application.Quit(); }; // persist settings and quit
                    Application.Run();
                }else{
                    tracker.SendMessageToFirstInstance(args);
                }
            }catch(SingleInstancingException ex){
                var dlg = new Gtk.MessageDialog(null, 0, MessageType.Error, ButtonsType.Ok, "Could not create a SingleInstanceTracker object:\n" + ex.Message + "\nInner Exception:\n" + ex.InnerException?.ToString() + "\nApplication will now terminate.");
                dlg.DeleteEvent += (o, ea) => {
                    Application.Quit();
                };
                dlg.Show();
                Application.Run();
            }finally{
                Dispose();
            }
        }
        #endregion
    }
}
