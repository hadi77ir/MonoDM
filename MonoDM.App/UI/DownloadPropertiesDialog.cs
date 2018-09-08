using System.Diagnostics;
using Gtk;
using MonoDM.Core;

namespace MonoDM.App.UI
{
    public class DownloadPropertiesDialog : Dialog
    {
        public DownloadPropertiesDialog() : base()
        {
            InitializeComponent();
        }

        private Downloader _downloader;
        private Entry _txtUrl;
        private Entry _txtDest;
        private Label _lblStatus;
        private Label _lblSize;
        private Label _lblType;
        
        private void InitializeComponent()
        {
            AddButton("Open", ResponseType.Apply);
            AddButton("OK", ResponseType.Ok);

            
            _txtUrl = new Entry();
            _txtDest = new Entry();
            _lblSize = new Label();
            _lblStatus = new Label();
            _lblType = new Label();
            
            
            Gtk.Box box;
            box = new HBox(false, 30);
            VBox.PackStart(box);
            
            VBox.PackStart(new HSeparator());

            Table tbl;
            tbl = new Table(6, 2, false);
            
            tbl.Attach(new Label("Type:"), 0, 1, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            tbl.Attach(_lblType, 1, 2, 0, 1, AttachOptions.Fill, AttachOptions.Shrink, 20,0);

            
            tbl.Attach(new Label("Status:"), 0, 1, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            tbl.Attach(_lblStatus, 1, 2, 1, 2, AttachOptions.Fill, AttachOptions.Shrink, 20,0);

            tbl.Attach(new Label("Size:"), 0, 1, 2, 3, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            tbl.Attach(_lblSize, 1, 2, 2, 3, AttachOptions.Fill, AttachOptions.Shrink, 20,0);

            tbl.Attach(new Label("Save To:"), 0, 1, 3, 4, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            
            box = new HBox();
            Button btn = new Button();
            btn.Label = "Browse...";
            
            box.PackStart(_txtDest);
            box.PackEnd(btn);
            tbl.Attach(box, 1, 2, 3, 4, AttachOptions.Fill, AttachOptions.Shrink, 20,0);

            tbl.Attach(new Label("URL:"), 0, 1, 4, 5, AttachOptions.Shrink, AttachOptions.Shrink, 20,0);
            tbl.Attach(_txtUrl, 1, 2, 4, 5, AttachOptions.Fill, AttachOptions.Shrink, 20,0);
            VBox.PackStart(tbl);
            
            ShowAll();
        }

        public void SetDownloadData(Downloader d)
        {
            _downloader = d;
            
        }

        public void SaveChanges()
        {
        }

    }
}