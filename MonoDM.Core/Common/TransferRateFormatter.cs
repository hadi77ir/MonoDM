using System;
using System.Collections.Generic;
using System.Text;

namespace MonoDM.Core.Common
{
    public static class TransferRateFormatter
    {
        private const long KBs = 1024;
        private const long MBs = KBs * 1024;
        private const long GBs = MBs * 1024;

        private const string BsFormatPattern = "{0:0} B/s";
        private const string KBsFormatPattern = "{0:0.00} KB/s";
        private const string MBsFormatPattern = "{0:0.00} MB/s";
        private const string GBsFormatPattern = "{0:0.00} GB/s";

        public static string ToString(double size)
        {
            if (size < KBs)
            {
                return String.Format(BsFormatPattern, size);
            }
            else if (size >= KBs && size < MBs)
            {
                return String.Format(KBsFormatPattern, size / KBs);
            }
            else if (size >= MBs && size < GBs)
            {
                return String.Format(MBsFormatPattern, size / MBs);
            }
            else // size >= GB
            {
                return String.Format(GBsFormatPattern, size / GBs);
            }
        }
    }
}
