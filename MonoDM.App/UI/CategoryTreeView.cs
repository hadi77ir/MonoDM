using System;
using GLib;
using Gtk;

namespace MonoDM.App.UI
{
    public class CategoryTreeView : TreeView
    {
        public CategoryTreeView()
        {
            ListStore = new ListStore(typeof(string));
            AppendColumn("Category", new CellRendererText(), "text",0);
            ListStore.AppendValues("All Downloads");
            ListStore.AppendValues("Finished");
            ListStore.AppendValues("Unfinished");
        }

        public int CurrentCategory
        {
            get
            {
                try
                {
                    TreeModel tm;
                    TreeIter ti;

                    if (!Selection.GetSelected(out tm, out ti))
                        return 0;

                    string response= (string) tm.GetValue(ti, 0);
                    switch (response)
                    {
                        case "Finished":
                            return 1;
                        case "Unfinished":
                            return 2;
                        default:
                            return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }
        }


        public ListStore ListStore
        {
            get { return (ListStore)Model; }
            set { Model = value; }
        }
    }
}