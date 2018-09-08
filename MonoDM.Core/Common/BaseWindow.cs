using System;
using GLib;
using Gtk;

namespace MonoDM.Core.Common
{
    public class BaseWindow : Window
    {
        protected BaseWindow(GType gtype) : base(gtype)
        {
        }

        public BaseWindow(IntPtr raw) : base(raw)
        {
        }

        public BaseWindow(WindowType type) : base(type)
        {
        }

        public BaseWindow(string title) : base(title)
        {
        }

        public virtual void OnArgsReceived(string[] args)
        {
        }
    }
}