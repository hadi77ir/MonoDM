using System;
using System.Reflection;
using Gtk;
using MonoDM.Core;
using MonoDM.Core.Common;
using MonoDM.Core.UI;
using ResourceLocation = MonoDM.Core.ResourceLocation;

namespace MonoDM.App.UI
{
    public partial class AddMultipleDownloadDialog : Gtk.Dialog
    {
        public AddMultipleDownloadDialog()
        {
	        InitializeComponent();
        }

	    public NodeView nodeView;
	    public CheckButton cbStartNow;
	    void InitializeComponent()
	    {
		    nodeView = new NodeView();
		    var store = new NodeStore(typeof(BatchDownloadNode));
		    nodeView.NodeStore = store;
		    // working around bug #xxxxxx
		    typeof(NodeView).GetField("store",BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(nodeView, store);

		    
		    var renderer = new CellRendererToggle();
		    renderer.Activatable = true;
		    renderer.Toggled += RendererOnToggled;
		    nodeView.AppendColumn("Checked", renderer, 0);
		    nodeView.AppendColumn("URL", new CellRendererText(), "text", 1);
		    VBox.PackStart(nodeView,true,true,0);
		    cbStartNow = new CheckButton();
		    cbStartNow.Label = "Start Now?";
		    VBox.PackStart(cbStartNow, false,false,0);
		    AddButton("Ok", ResponseType.Ok);
		    AddButton("Cancel", ResponseType.Cancel);
		    DefaultResponse = ResponseType.Cancel;
		    Response += (o, args) =>
		    {
			    if (args.ResponseId == ResponseType.Ok) BtnOk_Click(o, args);
			    else BtnCancel_Click(o, args);
		    }; 
		    ShowAll();
	    }

	    private void RendererOnToggled(object o, ToggledArgs args)
	    {
		    try
		    {
			    TreePath t = new TreePath(args.Path);
			    TreeIter ti;
			    nodeView.Model.GetIter(out ti, t);
			    nodeView.Model.SetValue(ti, 0, !((bool) nodeView.Model.GetValue(ti, 0)));
		    }catch{}
	    }

	    protected void BtnOk_Click(object sender, EventArgs e)
	    {
		    Gtk.TreeModel model = nodeView.Model;
		    Gtk.TreeIter iter;
		    if (model.GetIterFirst(out iter)) {
			    do {
				    
				    if ((bool)nodeView.Model.GetValue(iter, 0))
				    {
					    ResourceLocation rl = new ResourceLocation();
					    rl.URL = nodeView.Model.GetValue(iter, 1).ToString();
					    Downloader download = DownloadManager.Instance.Add(
						    rl,
						    new ResourceLocation[0], 
						    System.IO.Path.Combine(MonoDM.Core.Settings.Default.DownloadFolder, PathHelper.GetFileNameFromUrl(rl.URL)),
						    MonoDM.Core.Settings.Default.MaxSegments,
						    cbStartNow.Active);
				    }
			    } while (model.IterNext(ref iter));
		    }
		    Destroy();
	    }

		protected void BtnCancel_Click(object sender, EventArgs e)
		{
			Destroy();
		}
	}

	[Gtk.TreeNode(ListOnly = true), Serializable]
    public class BatchDownloadNode : Gtk.TreeNode
    {
        public BatchDownloadNode ()
        {
        }

        [Gtk.TreeNodeValue(Column = 0)]
        public bool Checked { get; set; }

        [Gtk.TreeNodeValue(Column = 1)]
        public string Url { get; set; }

    }
}
