using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Gtk;
using MonoDM.App.UI;
using MonoDM.Core;
using MonoDM.Core.Common;
using MonoDM.Core.UI;
using MonoDM.Extension.SpeedLimit;
using Image = Gtk.Image;
using Diag = System.Diagnostics;

namespace MonoDM.App 
{
	public class MainWindow : BaseWindow
    {
        public MainWindow() :
                base(Gtk.WindowType.Toplevel)
        {
            // Initialize UI
            InitializeComponent();

	        // load settings
	        LoadUISettings();
	        
            // Timer to Refresh list
            RefreshTimer = new Timer();
            RefreshTimer.Elapsed += RefreshTimer_Elapsed;
            RefreshTimer.Interval = 1000;
            RefreshTimer.Start();

            // Get Speed limiter extension (if available)
            speedLimit = App.Instance.GetExtensionByType<SpeedLimitExtension>();
	        DeleteEvent += (o, args) => { args.RetVal = HideOnDelete(); };

        }

	    private void LoadUISettings()
	    {
			LoadToolbarIcons(MonoDM.App.Settings.Default.UseIdmIcons);
		    SetTrayIconVisibility(MonoDM.App.Settings.Default.ShowTrayIcon);
	    }

	    private void SetTrayIconVisibility(bool status)
	    {
		    App.Instance.TrayIcon.Visible = status;
	    }

	    private void LoadToolbarIcons(bool defaultUseIdmIcons)
	    {
		    if(defaultUseIdmIcons)
				foreach (var c in MainToolbar.Children)
				{
					ToolButton item = (ToolButton) c;
					if (item != null)
					{
						//if (item.IconWidget is Image iconWidget)
						//{
						item.IconWidget = new Gtk.Image(GtkUtils.GetIdmToolbarIcon((string)item.Data["iconKey"]));
						//iconWidget.Pixbuf = GtkUtils.GetIdmToolbarIcon((string)item.Data["iconKey"]);
						//}
					}
				}
		    else
			    foreach (var c in MainToolbar.Children)
			    {
				    ToolButton item = (ToolButton) c;
				    if (item != null)
				    {
					    //if (item.IconWidget is Image iconWidget)
					    //{
					    item.IconWidget = new Gtk.Image(GtkUtils.GetGtkToolbarIcon(this, (string)item.Data["iconKey"]));
					    //}
				    }
			    }
		    MainToolbar.ShowAll();
	    }

	    public Toolbar MainToolbar;
		public MenuBar MainMenubar;
	    public Timer RefreshTimer;
	    public SpeedLimitExtension speedLimit;
	    public DownloadList downloadList;
	    public CategoryTreeView catList;
		void InitializeComponent()
		{
			DefaultSize = new Gdk.Size(900, 500);
			
			var mainVbox = new VBox(false, 0);

			MainMenubar = new MenuBar();
			InitMenu(MainMenubar);
                
			MainToolbar = new Toolbar();
			InitToolbar(MainToolbar);

			var splitCont = new HPaned();
			
			// list of downloads
			downloadList = new DownloadList();
			var scrolledWindow = new ScrolledWindow();
			scrolledWindow.Child = downloadList;
			splitCont.Add2(scrolledWindow);
			nodeModel = downloadList.Model;

			// categories list
			catList = new CategoryTreeView();
			splitCont.Add1(catList);
			
			catList.Selection.Changed+= CategorySelectionOnChanged;
			
			mainVbox.PackStart(MainMenubar, false, true, 0);
			mainVbox.PackStart(MainToolbar, false, false, 0);
			mainVbox.PackStart(splitCont);

			Add(mainVbox);
			ShowAll();
		}

	    private TreeModel nodeModel;
	    private bool FilterUnfinished(TreeModel model, TreeIter iter)
	    {
		    string val = model.GetValue (iter, 3).ToString ();
		    if (val.ToLower() != "ended")
			    return true;
		    else
			    return false;
	    }
	    private bool FilterFinished(TreeModel model, TreeIter iter)
	    {
		    string val = model.GetValue (iter, 3).ToString ();
		    if (val.ToLower() == "ended")
			    return true;
		    else
			    return false;
	    }
	    private void CategorySelectionOnChanged(object sender, EventArgs e)
	    {
		    int curcat = catList.CurrentCategory;
		    if (curcat == 0)
		    {
			    downloadList.Model = nodeModel;
		    }

		    TreeModelFilter filter = new Gtk.TreeModelFilter (nodeModel, null);
 
			// Specify the function that determines which rows to filter out and which ones to display
		    if(curcat == 1)
				filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterFinished);
		    else
				filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc (FilterUnfinished);
		   
			// Assign the filter as our tree's model
		    downloadList.Model = filter;
	    }

	    void InitToolbar(Toolbar toolbar)
		{
			toolbar.IconSize = IconSize.LargeToolbar;
			toolbar.ToolbarStyle = ToolbarStyle.Both;
			ToolButton toolButton;

            toolButton = new ToolButton(Stock.Add);
			toolButton.Data["iconKey"] = "add";
            toolButton.Label = "Add URL";
            toolButton.IsImportant = true;
			toolButton.Clicked += BtnAddUrl_Click;
			toolbar.Add(toolButton);

            toolButton = new ToolButton(Stock.Execute);
			toolButton.Data["iconKey"] = "resume";
            toolButton.Label = "Resume";
			toolButton.IsImportant = true;
			toolButton.Clicked += BtnResume_Click;
            toolbar.Add(toolButton);         

			toolButton = new ToolButton(Stock.Stop);
			toolButton.Data["iconKey"] = "stop";
            toolButton.Label = "Stop";
			toolButton.IsImportant = true;
			toolButton.Clicked += BtnStop_Click;
            toolbar.Add(toolButton);

			toolButton = new ToolButton(Stock.Stop);
			toolButton.Data["iconKey"] = "stopall";
            toolButton.Label = "Stop All";
			toolButton.IsImportant = true;
			toolButton.Clicked += BtnStopAll_Click;
            toolbar.Add(toolButton);
            
            toolButton = new ToolButton(Stock.Remove);
			toolButton.Data["iconKey"] = "delete";
            toolButton.Label = "Remove";
			toolButton.IsImportant = true;
			toolButton.Clicked += BtnDelete_Click;
            toolbar.Add(toolButton);

            toolButton = new ToolButton(Stock.Delete);
			toolButton.Data["iconKey"] = "deletecomp";
            toolButton.Label = "Remove Completed";
			toolButton.IsImportant = true;
			toolButton.Clicked += BtnDeleteCompleted_Click;
            toolbar.Add(toolButton);

			toolButton = new ToolButton(Stock.Preferences);
			toolButton.Data["iconKey"] = "options";
			toolButton.Label = "Options";
            toolButton.IsImportant = true;
			toolButton.Clicked += BtnOptions_Click;
            toolbar.Add(toolButton);

            toolButton = new ToolButton(Stock.MediaPlay);
			toolButton.Data["iconKey"] = "startqueue";
            toolButton.Label = "Start Queue";
            toolButton.IsImportant = true;
			toolButton.Clicked += BtnStartQueue_Click;
			toolbar.Add(toolButton);

            toolButton = new ToolButton(Stock.MediaStop);
			toolButton.Data["iconKey"] = "stopqueue";
            toolButton.Label = "Stop Queue";
            toolButton.IsImportant = true;
			toolButton.Clicked += BtnStopQueue_Click;
            toolbar.Add(toolButton);
		}

		void InitMenu(MenuBar menubar)
		{
			// ReSharper disable JoinDeclarationAndInitializer
			Menu menu, submenu;
			MenuItem menuItem;
			// ReSharper restore JoinDeclarationAndInitializer

            // tasks menu
			menuItem = new MenuItem("Tasks"); 
            menu = new Menu();
			menuItem.Submenu = menu;
			menubar.Add(menuItem);
            
            // subitems of tasks menu
			menuItem = new MenuItem("Add new download");
			menuItem.Activated += BtnAddUrl_Click;
            menu.Add(menuItem);
           
			menuItem = new MenuItem("Add batch download");
			menuItem.Activated += BtnAddBatch_Click;
            menu.Add(menuItem);
           
			menuItem = new MenuItem("Add batch download from clipboard");
			menuItem.Activated += mnuAddBatchClipboard_Click;
            menu.Add(menuItem);
           
			menuItem = new MenuItem("Export");
            menu.Add(menuItem);
			// it has a submenu
			submenu = new Menu();
			menuItem.Submenu = submenu;

			menuItem = new MenuItem("To text file");
			menuItem.Activated += mnuExportTxt_Click;
			submenu.Add(menuItem);

            menuItem = new MenuItem("To EF2 file");
			menuItem.Activated += mnuExportEF2_Click;
            submenu.Add(menuItem);

            menuItem = new MenuItem("Import");
			menu.Add(menuItem);
            // it has a submenu
            submenu = new Menu();
            menuItem.Submenu = submenu;

            menuItem = new MenuItem("From text file");
			menuItem.Activated += mnuImportTxt_Click;
            submenu.Add(menuItem);

            menuItem = new MenuItem("From EF2 file");
			menuItem.Activated += mnuImportEF2_Click;
			submenu.Add(menuItem);
                     

            menuItem = new MenuItem("Exit");
			menuItem.Activated += mnuExit_Click;
            menu.Add(menuItem);

            // file menu         
			menuItem = new MenuItem("File");
            menu = new Menu();
            menuItem.Submenu = menu;
			menubar.Add(menuItem);

            menuItem = new MenuItem("Stop Download");
			menuItem.Activated += BtnStop_Click;
			menu.Add(menuItem);

            menuItem = new MenuItem("Remove");
			menuItem.Activated += BtnDelete_Click;
			menu.Add(menuItem);

            menuItem = new MenuItem("Download Now");
			menuItem.Activated += BtnResume_Click;
			menu.Add(menuItem);

            menuItem = new MenuItem("Redownload");
			menuItem.Activated += mnuRedownload_Click;
			menu.Add(menuItem);

            // Downloads menu         
            menuItem = new MenuItem("Downloads");
            menu = new Menu();
            menuItem.Submenu = menu;
			menubar.Add(menuItem);

            menuItem = new MenuItem("Pause All");
			menuItem.Activated += mnuPauseAll_Click;
			menu.Add(menuItem);

            menuItem = new MenuItem("Stop All");
			menuItem.Activated += BtnStopAll_Click;
			menu.Add(menuItem);

            menuItem = new MenuItem("Delete All Completed");
			menuItem.Activated += BtnDeleteCompleted_Click;
			menu.Add(menuItem);

            menuItem = new MenuItem("Scheduler");
			menuItem.Activated += mnuScheduler_Click;
			menu.Add(menuItem);

            menuItem = new MenuItem("Start Queue");
			menuItem.Activated += BtnStartQueue_Click;
			menu.Add(menuItem);

            menuItem = new MenuItem("Stop Queue");
			menuItem.Activated += BtnStopQueue_Click;
			menu.Add(menuItem);

			if (speedLimit != null)
			{
				menuItem = new MenuItem("Speed Limiter");
				menu.Add(menuItem);
				submenu = new Menu();
				menuItem.Submenu = submenu;

				menuItem = new RadioMenuItem("On");
				((RadioMenuItem) menuItem).Active = speedLimit.CurrentEnabled;
				menuItem.Activated += mnuSpeedLimiterOn_Click;
				submenu.Add(menuItem);

				menuItem = new RadioMenuItem((RadioMenuItem) menuItem, "Off");
				((RadioMenuItem) menuItem).Active = !speedLimit.CurrentEnabled;
				menuItem.Activated += mnuSpeedLimiterOff_Click;
				submenu.Add(menuItem);
			}

			menuItem = new MenuItem("Options");
			menuItem.Activated += BtnOptions_Click;
            menu.Add(menuItem);

            // View menu         
            menuItem = new MenuItem("View");
            menu = new Menu();
            menuItem.Submenu = menu;
            menubar.Add(menuItem);

			menuItem = new MenuItem("Toolbar");
			submenu = new Menu();
			menuItem.Submenu = submenu;
			menu.Add(menuItem);

			RadioMenuItem radioMenuItem;
			radioMenuItem = new RadioMenuItem("IDM style");
			radioMenuItem.Active = MonoDM.App.Settings.Default.UseIdmIcons;
			radioMenuItem.Activated += mnuToolbarIconIdm_Click;
			submenu.Add(radioMenuItem);

			radioMenuItem = new RadioMenuItem(radioMenuItem, "GTK style");
			radioMenuItem.Active = !MonoDM.App.Settings.Default.UseIdmIcons;
			radioMenuItem.Activated += mnuToolbarIconGtk_Click;
            submenu.Add(radioMenuItem);

			menuItem = new CheckMenuItem("Tray Icon");
			((CheckMenuItem)menuItem).Active = MonoDM.App.Settings.Default.ShowTrayIcon;
			menuItem.Toggled += mnuTrayIcon_Click;
			menu.Add(menuItem);

            // help menu
			menuItem = new MenuItem("Help");
			menu = new Menu();
			menuItem.Submenu = menu;
			menubar.Add(menuItem);

			menuItem = new MenuItem("Documentation");
			menuItem.Activated += mnuDocumentation_Click;			
			menu.Add(menuItem);
			
			menuItem = new MenuItem("About");
			menuItem.Activated += mnuAbout_Click;		
			menu.Add(menuItem);
		}

	    #region UI Events
        protected void BtnAddUrl_Click(object sender, EventArgs e)
        {
	        downloadList.BtnNewDownload_Click(sender, e);
        }

        protected void BtnResume_Click(object sender, EventArgs e)
        {
	        downloadList.BtnStartSelected_Click(sender, e);
        }

        protected void BtnStop_Click(object sender, EventArgs e)
        {
	        downloadList.BtnPause_Click(sender, e);
        }

        protected void BtnStopAll_Click(object sender, EventArgs e)
        {
	        downloadList.StopAll();
        }

        protected void BtnDelete_Click(object sender, EventArgs e)
        {
	        downloadList.BtnRemoveSelected_Click(sender, e);
        }

        protected void BtnDeleteCompleted_Click(object sender, EventArgs e)
        {
	        downloadList.RemoveCompleted();
        }

        protected void BtnOptions_Click(object sender, EventArgs e)
        {
	        using (var a = new OptionsDialog())
	        {
		        a.Run();
	        }
        }

        protected void BtnStartQueue_Click(object sender, EventArgs e)
        {
	        downloadList.StartScheduler(true);
        }

        protected void BtnStopQueue_Click(object sender, EventArgs e)
        {
	        downloadList.StartScheduler(false);
        }

        protected void BtnAddBatch_Click(object sender, EventArgs e)
        {
	        using (var batchDlg = new CreateBatchDownloadDialog())
	        {
		        batchDlg.Run();
		        batchDlg.Destroy();
	        }
        }

        protected void mnuAddBatchClipboard_Click(object sender, EventArgs e)
        {
	        List<string> lst = ClipboardHelper.GetURLListFromClipboard();
	        if(lst.Count == 0)
		        return;
	        
	        using (var batchDlg = new AddMultipleDownloadDialog())
	        {
		        foreach (var item in lst)
		        {
			        batchDlg.nodeView.NodeStore.AddNode(new BatchDownloadNode{Checked = true, Url = item});
		        }

		        batchDlg.Run();
		        batchDlg.Destroy();
	        }

        }

        protected void mnuImportTxt_Click(object sender, EventArgs e)
        {
	        downloadList.ImportFromTextFile();
        }

        protected void mnuImportEF2_Click(object sender, EventArgs e)
        {
	        downloadList.ImportFromEF2File();
        }

        protected void mnuExportTxt_Click(object sender, EventArgs e)
        {
	        downloadList.ExportToTextFile();
        }

        protected void mnuExportEF2_Click(object sender, EventArgs e)
        {
	        downloadList.ExportToEF2File();	        
        }

        protected void mnuExit_Click(object sender, EventArgs e)
        {
	        Destroy();
	        Application.Quit();
        }

        protected void mnuRedownload_Click(object sender, EventArgs e)
        {
	        //todo
	        foreach (var d in downloadList.SelectedDownloaders)
	        {
		        // pause download
		        d.Pause();
		        // clear segments
		        d.Segments.Clear();
		        // restart
			    d.DefaultDownloadProvider.GetFileInfo(d.ResourceLocation, out Stream stream);
		        d.StartSegments(MonoDM.Core.Settings.Default.MaxSegments, stream);
	        }
        }

        protected void mnuPauseAll_Click(object sender, EventArgs e)
        {
	        foreach (var d in DownloadManager.Instance.Downloads)
	        {
		        d.Pause();
	        }
        }


        protected void mnuScheduler_Click(object sender, EventArgs e)
        {
	        //todo
	        //show options dialog, tab of autodownloads
	        BtnOptions_Click(sender, e);
        }
	    

        protected void mnuSpeedLimiterOn_Click(object sender, EventArgs e)
        {
	        speedLimit.Parameters.Enabled = true;
        }

        protected void mnuSpeedLimiterOff_Click(object sender, EventArgs e)
        {
	        speedLimit.Parameters.Enabled = false;
        }

	    private void mnuAbout_Click(object sender, EventArgs e)
	    {
		    BtnAbout_Click(sender, e);
	    }

	    private void mnuDocumentation_Click(object sender, EventArgs e)
	    {
		    Diag.Process.Start("https://github.com/hadi77ir/monodm/wiki");
	    }

	    private void mnuTrayIcon_Click(object sender, EventArgs e)
	    {
		    SetTrayIconVisibility(((CheckMenuItem)sender).Active);
	    }

	    private void mnuToolbarIconGtk_Click(object sender, EventArgs e)
	    {
		    LoadToolbarIcons(false);
		    MonoDM.App.Settings.Default.UseIdmIcons = false;
		    MonoDM.App.Settings.Default.Save();
	    }

	    private void mnuToolbarIconIdm_Click(object sender, EventArgs e)
	    {
		    LoadToolbarIcons(true);
		    MonoDM.App.Settings.Default.UseIdmIcons = true;
		    MonoDM.App.Settings.Default.Save();
	    }

        protected void BtnAbout_Click(object sender, EventArgs e)
        {
	        new AboutDialog().Run();
        }

        void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // update transfer rate
            string strRate;

            if (speedLimit != null && speedLimit.CurrentEnabled)
            {
                strRate = String.Format("[{0:0.##} kbps] {1:0.##} kbps",
                    speedLimit.CurrentMaxRate / 1024.0,
                    DownloadManager.Instance.TotalDownloadRate / 1024.0);
            }
            else
            {
                strRate = String.Format("{0:0.##} kbps", DownloadManager.Instance.TotalDownloadRate / 1024.0);
            }

            App.Instance.TrayIcon.Tooltip = String.Concat(this.Title, "\n", strRate);

            // update queue status
            UpdateQueueStatus();
        }

	    private void UpdateQueueStatus()
	    { //todo
	    }

	    #endregion

        public void OnArgsReceived(string[] args)
        {
            if (args.Length > 0 && Uri.IsWellFormedUriString(args[0], UriKind.Absolute))
                downloadList.AddNewFromURL(args[0]);
        }
    }
}
