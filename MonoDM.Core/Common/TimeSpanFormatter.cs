using System;
using System.Collections.Generic;
using System.Text;

namespace MonoDM.Core.Common
{
    public static class TimeSpanFormatter
    {
        public static string ToString(TimeSpan ts)
        {
            if (ts == TimeSpan.MaxValue)
            {
                return "?";
            }

            if(Math.Ceiling(ts.TotalDays) > 1)
                return 
                    $"{Math.Ceiling(ts.TotalDays) - 1}d {ts:h\'h \'m\'m\'}";
            
            if (ts.Hours > 0)
                return ts.ToString("h'h 'm'm'");
            
            if (ts.Minutes > 0)
                return ts.ToString("m'm 's's'");
            
            return ts.ToString("s's'");
        }
    }
}
