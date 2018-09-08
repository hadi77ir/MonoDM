using System;
using System.Collections.Generic;
using System.Text;

namespace MonoDM.Core
{
    public enum SegmentState
    {
        Idle,
        Connecting,
        Downloading,
        Paused,
        Finished,
        Error,
    }
}
