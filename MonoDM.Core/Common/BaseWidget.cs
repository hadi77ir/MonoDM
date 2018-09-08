using System.Configuration;
using Gtk;
using MonoDM.Core.Extensions;

namespace MonoDM.Core.Common
{
	public class BaseWidget : VBox
    {
        public virtual string Text { get; }
        public virtual IExtension Extension { get; set; }
    }
}