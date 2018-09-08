using System;
using System.Diagnostics;
using System.Text;

namespace MonoDM.Core
{
    public static class OsUtils
    {
        public static void OpenFolder(string file)
        {
            file = System.IO.Path.GetFullPath(file);
            string explorer = "explorer.exe";
            string explorerargs = "/select,{0}";
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                explorer = "xdg-open";
                explorerargs = "{1}";
            }

            Process.Start(explorer,
                String.Format(explorerargs, EncodeProcessParameterArgument(file),
                    EncodeProcessParameterArgument(
                        System.IO.Path.GetDirectoryName(file))));
        }
        public static string EncodeProcessParameterArgument(string argument, bool force = false)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));
        
            // Unless we're told otherwise, don't quote unless we actually
            // need to do so --- hopefully avoid problems if programs won't
            // parse quotes properly
            if (force == false
                && argument.Length > 0
                && argument.IndexOfAny(" \t\n\v\"".ToCharArray()) == -1)
            {
                return argument;
            }
        
            var quoted = new StringBuilder();
            quoted.Append('"');
        
            var numberBackslashes = 0;
        
            foreach (var chr in argument)
            {
                switch (chr)
                {
                    case '\\':
                        numberBackslashes++;
                        continue;
                    case '"':
                        // Escape all backslashes and the following
                        // double quotation mark.
                        quoted.Append('\\', numberBackslashes*2 + 1);
                        quoted.Append(chr);
                        break;
                    default:
                        // Backslashes aren't special here.
                        quoted.Append('\\', numberBackslashes);
                        quoted.Append(chr);
                        break;
                }
                numberBackslashes = 0;
            }
        
            // Escape all backslashes, but let the terminating
            // double quotation mark we add below be interpreted
            // as a metacharacter.
            quoted.Append('\\', numberBackslashes*2);
            quoted.Append('"');
        
            return quoted.ToString();
        }
    }
}