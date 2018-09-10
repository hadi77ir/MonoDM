using System;
using System.Collections.Generic;
using Gdk;
using Gtk;

namespace MonoDM.App.UI
{
    public class CreateBatchDownloadDialog : Gtk.Dialog
    {
        public CreateBatchDownloadDialog()
        {
            InitializeComponent();
        }

        private Entry maskedUrl;
        private SpinButton numMin;
        private SpinButton numMax;
        private SpinButton numWildcardSize;

        private void InitializeComponent()
        {
            DefaultSize = new Size(450,200);
            maskedUrl = new Entry();
            numMin = new SpinButton(double.MinValue,double.MaxValue,1);
            numMin.Digits = 0;
            numMin.Value = 0;
            numMax = new SpinButton(double.MinValue,double.MaxValue,1);
            numMax.Digits = 0;
            numMax.Value = 1;
            numWildcardSize = new SpinButton(1,10,1);
            numWildcardSize.Digits = 0;
            numWildcardSize.Value = 1;

            VBox.PackStart(maskedUrl,true,false,0);
            Table tbl = new Table(3, 2, false);
            tbl.Attach(new Label("From:"), 0,1,0, 1);
            tbl.Attach(new Label("To:"), 0,1,1, 2);
            tbl.Attach(new Label("Wildcard Size:"), 0,1,2, 3);
            tbl.Attach(numMin, 1, 2, 0, 1);
            tbl.Attach(numMax, 1, 2, 1, 2);
            tbl.Attach(numWildcardSize, 1, 2, 2, 3);
            
            VBox.PackStart(tbl,true,false,0);

            DefaultResponse = ResponseType.Cancel;
            AddButton("Cancel", ResponseType.Cancel);
            AddButton("Ok", ResponseType.Ok);
            
            Response += OnResponse;
            ShowAll();
        }

        private void OnResponse(object o, ResponseArgs args)
        {
            if (args.ResponseId == ResponseType.Ok)
            {
                List<string> list = new List<string>();
                int min, max;
                if (numMin.ValueAsInt > numMax.ValueAsInt)
                {
                    min = numMax.ValueAsInt;
                    max = numMin.ValueAsInt;
                }
                else
                {
                    min = numMin.ValueAsInt;
                    max = numMax.ValueAsInt;
                }

                string wildcardSize = new string('0', numWildcardSize.ValueAsInt);
                for (int i = min; i < max; i++)
                {
                    string url = maskedUrl.Text.Replace("*", i.ToString(wildcardSize));
                    if(!list.Contains(url))
                        list.Add(url);
                }

                using (var batchDlg = new AddMultipleDownloadDialog())
                {
                    foreach (var item in list)
                    {
                        batchDlg.Store.AddNode(new BatchDownloadNode{Checked = true, Url = item});
                    }

                    batchDlg.Run();
                    batchDlg.Destroy();
                }
            }
        }
    }
}
