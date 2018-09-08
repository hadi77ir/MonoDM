using System;
using System.Collections.Generic;
using Gtk;

namespace MonoDM.Core.UI
{
    public static class ClipboardHelper
    {
        public static string GetURLOnClipboard()
        {
            string url = string.Empty;
            var clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
            if (clipboard.WaitIsTextAvailable())
            {
                string tempUrl = clipboard.WaitForText().Split('\n')[0];
                
                if (ResourceLocation.IsURL(tempUrl))
                {
                    url = tempUrl;
                }
                else
                {
                    tempUrl = null;
                }
            }

            return url;
        }

        public static List<string> GetURLListFromClipboard()
        {
            List<string> urls = new List<string>();
            var clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
            if (clipboard.WaitIsTextAvailable())
            {
                string[] tempUrls = clipboard.WaitForText().Split('\n');
                foreach (var tempUrl in tempUrls)
                {
                    if (ResourceLocation.IsURL(tempUrl))
                    {
                        urls.Add(tempUrl);
                    }
                }
            }

            return urls;
        }
    }
}
