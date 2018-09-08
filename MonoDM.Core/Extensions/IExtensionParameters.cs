using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace MonoDM.Core.Extensions
{
    public interface IExtensionParameters
    {
        event PropertyChangedEventHandler ParameterChanged;
    }
}
