using System;
using System.IO;
using Gdk;
using Gtk;

namespace MonoDM.Core.UI
{
    public static class GtkUtils
    {
        public static Pixbuf GetIdmToolbarIcon(string key)
        {
            int index = 0;
            switch (key.Trim().ToLower())
            {
                case "add":
                    index = 0;
                    break;
                case "resume":
                    index = 1;
                    break;
                case "stop":
                    index = 2;
                    break;
                case "stopall":
                    index = 3;
                    break;
                case "delete":
                    index = 4;
                    break;
                case "deletecomp":
                    index = 5;
                    break;
                case "options":
                    index = 6;
                    break;
                // scheduler icon not used
                case "startqueue":
                    index = 8;
                    break;
                case "stopqueue":
                    index = 9;
                    break;
                // grabber icon not used
                // tell a friend icon not used
            }

            return GetIdmToolbarIcon(index);
        }

        public static Pixbuf GetIdmToolbarIcon(int index)
        {
            Pixbuf fullPixbuf = GetAllButtonPix();
            return new Pixbuf(fullPixbuf, (fullPixbuf.Width / 12) * index, 0, fullPixbuf.Width / 12, fullPixbuf.Height);
        }

        private static Pixbuf GetAllButtonPix()
        {
            var b = File.ReadAllBytes("Toolbar/3d_large_3_hdpi15.bmp");
            var pixbuf = new Pixbuf(b);
            // TODO: set alpha mask
            return pixbuf;
        }

        public static Pixbuf GetGtkToolbarIcon(Widget widget, string key)
        {
            string stock = "";
            switch (key.Trim().ToLower())
            {
                case "add":
                    stock = Stock.Add;
                    break;
                case "resume":
                    stock = Stock.Execute;
                    break;
                case "stop":
                    stock = Stock.Stop;
                    break;
                case "stopall":
                    stock = Stock.Stop;
                    break;
                case "delete":
                    stock = Stock.Remove;
                    break;
                case "deletecomp":
                    stock = Stock.Delete;
                    break;
                case "options":
                    stock = Stock.Preferences;
                    break;
                case "startqueue":
                    stock = Stock.MediaPlay;                    
                    break;
                case "stopqueue":
                    stock = Stock.MediaStop;
                    break;
            }

            if (stock == "")
                return null;
            
            return widget.RenderIcon(stock, IconSize.LargeToolbar, key);
        }

        public static int ShowMessageBox(Gtk.Window window, string caption, MessageType msgtype)
        {
            using (var msg = new MessageDialog(window, DialogFlags.Modal, msgtype, ButtonsType.Ok, caption))
            {
                int response = msg.Run();
                msg.Destroy();
                return response;
            }
        }
    }
}