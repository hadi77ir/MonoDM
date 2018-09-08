using System;
using System.Collections.Generic;
using System.Text;
using MonoDM.Core.Extensions;

namespace MonoDM.Core.UI
{
    public class CoreExtention : IExtension
    {
        #region IExtension Members

        public string Name
        {
            get { return "Core"; }
        }

        public IUIExtension UIExtension
        {
            get { return new CoreUIExtention(); }
        }

        #endregion
    }
}
