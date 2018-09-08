using Gtk;

namespace MonoDM.Core.UI
{
    public class GtkInputBox : MessageDialog
    {
        public GtkInputBox(Window parent, DialogFlags flags, MessageType type, ButtonsType bt, string format, string defaultText,
            params object[] args) :
            base(parent, flags, type, bt, format, args)
        {
            InitializeComponent(defaultText);
        }

        private void InitializeComponent(string defaultText)
        {
            TextEntry = new Entry();
            TextEntry.Text = defaultText;
            VBox.PackEnd(TextEntry, true,true,0);
            ShowAll();
        }

        public Entry TextEntry;
    }
}