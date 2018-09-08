using System;
using System.Collections;
using System.Collections.Generic;
using Gtk;
using MonoDM.Core.Common;
using MonoDM.Core.Extensions;

namespace MonoDM.App.UI
{
    public class OptionsDialog : Gtk.Dialog
    {
        delegate void ProcessItemDelegate(IExtension extension, BaseWidget[] options);
        public OptionsDialog()
        {
            InitializeComponent();
        }

        private Notebook _notebook;
        private void InitializeComponent()
        {
            _notebook = new Notebook();

            for (int i = 0; i < App.Instance.Extensions.Count; i++)
            {
                IExtension extension = App.Instance.Extensions[i];
                IUIExtension uiExtension = extension.UIExtension;

                if (uiExtension == null)
                {
                    continue;
                }

                BaseWidget[] options = uiExtension.CreateSettingsView();
                
                foreach (var opt in options)
                {
                    opt.Extension = extension;
                    _notebook.AppendPage(opt, new Gtk.Label(opt.Text));                        
                }
            }
            
            VBox.Add(_notebook);
            this.DefaultResponse = ResponseType.Cancel;
            AddButton("Cancel", ResponseType.Cancel);
            AddButton("Ok", ResponseType.Ok);
            ShowAll();
            
            Response += (o, args) =>
            {
                if (args.ResponseId == ResponseType.Ok)
                    btnOK_Click(o, args);
                else
                    btnCancel_Click(o, args);

                Destroy();
            };
        }

        protected override void OnRealized()
        {
            base.OnRealized();
        }

        private void ProcessSettings(ProcessItemDelegate process)
        {
            Hashtable extensionToControlArray = new Hashtable();
            for (int i = 0; i < _notebook.NPages; i++)
            {
                BaseWidget node = (BaseWidget)_notebook.GetNthPage(i);
                if (!extensionToControlArray.ContainsKey(node.Extension))
                    extensionToControlArray[node.Extension] = new List<BaseWidget>();
                
                ((List<BaseWidget>)extensionToControlArray[node.Extension]).Add(node);
            }
            foreach(object optionsList in extensionToControlArray.Values)
            {
                List<BaseWidget> options = (List<BaseWidget>)optionsList;
                process(options[0].Extension, options.ToArray());
            }
        }

        public void ShowOptionsPage(BaseWidget n)
        {
            for (int i = 0; i < _notebook.NPages; i++)
            {
                if (_notebook.GetNthPage(i) == n)
                {
                    _notebook.CurrentPage = i;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ProcessSettings(
                delegate(IExtension extension, BaseWidget[] options)
                {
                    extension.UIExtension.PersistSettings(options);

                    for (int i = 0; i < options.Length; i++)
                    {
                        options[i].Dispose();
                    }
                }
                );
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ProcessSettings(
                delegate(IExtension extension, BaseWidget[] options)
                {
                    for (int i = 0; i < options.Length; i++)
                    {
                        options[i].Dispose();
                    }
                }
            );
        }
        
    }
}
