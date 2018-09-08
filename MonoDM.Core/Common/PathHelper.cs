using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MonoDM.Core.Common
{
    public static class PathHelper
    {
        public static string GetWithBackslash(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar.ToString();
            }

            return path;
        }
        public static string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url, UriKind.Absolute);
            return Path.GetFileName(uri.LocalPath);
        }
    }
}
