using System;
using System.Collections.Generic;
using MonoDM.Core.Extensions;
using Gtk;
using MonoDM.Core.Common;

namespace MonoDM.App
{
    public interface IApp: IDisposable
    {
        BaseWindow MainWindow { get; }

        StatusIcon TrayIcon { get; }

        List<IExtension> Extensions { get; }

        IExtension GetExtensionByType(Type type);

        void Start(string[] args);
    }
}
