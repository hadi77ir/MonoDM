using System;
using System.Collections.Generic;
using System.Text;
using MonoDM.Core;

namespace MonoDM.Core.Extensions
{
    public interface IExtension
    {
        string Name { get; }

        IUIExtension UIExtension { get; }
    }
}
