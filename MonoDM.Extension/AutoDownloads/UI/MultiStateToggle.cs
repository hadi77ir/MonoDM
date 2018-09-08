using Gtk;
using Color = Cairo.Color;
using Cairo;
using Gdk;

namespace MonoDM.Extension.AutoDownloads.UI
{
    public class MultiStateToggle : DrawingArea
    {
        private EnableMode _enableMode = EnableMode.Disabled;
        public EnableMode EnableMode
        {
            get { return _enableMode;}
            set { _enableMode = value; this.QueueDraw(); }
        }
        Color[] Colors = {
            new Color(176 / 255.0,176/ 255.0,176/ 255.0),
            new Color(0,174/ 255.0,108/ 255.0),
            new Color(134/ 255.0,233/ 255.0,196/ 255.0)
        };

        //todo
        public MultiStateToggle()
        {
            WidthRequest = 20;
            HeightRequest = 20;
            QueueResize();
            Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | EventMask.ExposureMask | EventMask.Button1MotionMask | EventMask.Button2MotionMask;
        }

        public MultiStateToggle(EnableMode enableMode) : this()
        {
            EnableMode = enableMode;
        }

        protected override bool OnExposeEvent(Gdk.EventExpose evnt)
        {
            var res = base.OnExposeEvent(evnt);
            SetBgColor(EnableMode, evnt);
            return res;
        }

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            if(EnableMode == EnableMode.Active)
                EnableMode = EnableMode.ActiveWithLimit;
            else if(EnableMode == EnableMode.ActiveWithLimit)
                EnableMode = EnableMode.Disabled;
            else if(EnableMode == EnableMode.Disabled)
                EnableMode = EnableMode.Active;
            
            return base.OnButtonPressEvent(evnt);
        }

        /*override (Gdk.EventButton evnt)
        {
            var res = base.OnButtonPressEvent(evnt);
            if ((evnt.Button & (uint)Gdk.ModifierType.Button1Mask) != 0)
            {
                if ((evnt.Button & (uint)Gdk.ModifierType.ShiftMask) != 0)
                {
                    // if is left click + shift
                    EnableMode = EnableMode.ActiveWithLimit;
                }
                else
                {
                    // if is left click
                    EnableMode = EnableMode.Active;
                }
            }else if ((evnt.Button & (uint)Gdk.ModifierType.Button3Mask) != 0)
            {
                //right clicked
                EnableMode = EnableMode.Disabled;
            }
            
            QueueDraw();
            
            return res;
        }*/
 
        void SetBgColor(EnableMode enableMode, Gdk.EventExpose evnt)
        {
            //this.ModifyBg(StateType.Normal, Colors[(int)EnableMode]);
            Cairo.Context cr = Gdk.CairoHelper.Create(this.GdkWindow);
            cr.Rectangle (0,0, Allocation.Width, Allocation.Height);
            Color c = new Color(0,0,0);
            cr.SetSourceColor(c);
            cr.Stroke();
            
            cr.Rectangle(1,1, Allocation.Width - 1, Allocation.Height - 1);
            if (enableMode == EnableMode.Active)
            {
                c = Colors[1];
            }
            else if (enableMode == EnableMode.ActiveWithLimit)
            {
                c = Colors[2];                
            }
            else if (enableMode == EnableMode.Disabled)
            {
                c = Colors[0];
            }

            cr.SetSourceColor(c);
            cr.Fill();
            
            cr.Target.Dispose();
            cr.Dispose();
            
            // set tooltip
            HasTooltip = true;
            TooltipText = enableMode.ToString();
        }
    }
}