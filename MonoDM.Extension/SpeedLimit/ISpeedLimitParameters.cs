using System;
using System.Collections.Generic;
using System.Text;
using MonoDM.Core.Extensions;

namespace MonoDM.Extension.SpeedLimit
{
    public interface ISpeedLimitParameters: IExtensionParameters
    {
        bool Enabled { get; set; }

        double MaxRate { get; set; }
    }
}
