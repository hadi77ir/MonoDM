using System;
using System.Collections.Generic;
using Gtk;

namespace MonoDM.App.UI
{
    public partial class CreateBatchDownloadDialog : Gtk.Dialog
    {
        public CreateBatchDownloadDialog()
        {
            InitializeComponent();
        }

        private Entry maskedUrl;
        private SpinButton numMin;
        private SpinButton numMax;
        private void InitializeComponent()
        {
            maskedUrl = new Entry();
            numMin = new SpinButton(double.MinValue,double.MaxValue,1);
            numMin.Digits = 1;
            numMax = new SpinButton(double.MinValue,double.MaxValue,1);
            numMax.Digits = 1;

            VBox.PackStart(maskedUrl,true,false,0);
            Table tbl = new Table(2, 2, false);
            tbl.Attach(new Label("From:"), 0,1,0, 1);
            tbl.Attach(new Label("To:"), 0,1,1, 2);
            tbl.Attach(numMin, 1, 2, 0, 1);
            tbl.Attach(numMax, 1, 2, 1, 2);
            
            VBox.PackStart(tbl,true,false,0);

            DefaultResponse = ResponseType.Cancel;
            AddButton("Cancel", ResponseType.Cancel);
            AddButton("Ok", ResponseType.Ok);
            
            Response += OnResponse;
        }

        private void OnResponse(object o, ResponseArgs args)
        {
            if (args.ResponseId == ResponseType.Ok)
            {
                List<string> list = new List<string>();
                for (int i = numMin.ValueAsInt; i < numMax.ValueAsInt; i++)
                {
                    list.Add(maskedUrl.Text.Replace("*",i.ToString()));
                }

                using (var batchDlg = new AddMultipleDownloadDialog())
                {
                    foreach (var item in list)
                    {
                        batchDlg.nodeView.NodeStore.AddNode(new BatchDownloadNode{Checked = true, Url = item});
                    }

                    batchDlg.Run();
                    batchDlg.Destroy();
                }

            }
        }
    }
}
