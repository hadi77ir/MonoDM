using System;
using System.Collections.Generic;
using System.Text;
using Gtk;
using MonoDM.Core.Common;

namespace MonoDM.Core.Extensions
{
    public interface IUIExtension
    {
        BaseWidget[] CreateSettingsView();

        void PersistSettings(BaseWidget[] settingsView);
    }
}
