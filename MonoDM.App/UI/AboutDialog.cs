using System;
using Gtk;
using MonoDM.Core.UI;

namespace MonoDM.App
{
    public partial class AboutDialog : Gtk.AboutDialog
    {
        public AboutDialog() : base()
        {
            DeleteEvent += (o, args) => {HideOnDelete();}; // prevent deletion and disposal on dialog close
            Logo = new Gdk.Pixbuf("icon.png");
            ProgramName = "MonoDM";
            Version = typeof(AboutDialog).Assembly.GetName().Version.ToString();
            Comments = "A multi-threaded segmented download manager for Mono/.NET";
            License = "MIT";
            Authors = new[] { "Mohammad Hadi Hosseinpour" };
            this.Response += (o, args) =>
            {
                if (args.ResponseId == ResponseType.Cancel) Destroy();
            };
        }
    }
}
