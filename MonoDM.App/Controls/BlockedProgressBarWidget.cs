using System;
using System.Collections.Generic;
using Cairo;
namespace MonoDM.App.Controls
{
	public class BlockedProgressBarWidget : Gtk.DrawingArea
	{
	    public BlockedProgressBarWidget()
	    {
	        _blockList = new BlockList();
	        ForeColor = new Color(0,0,0.8);
	    }

	    BlockList _blockList;

		/// <summary>
		/// Color of segmented.
		/// </summary>
		public Color ForeColor { get; set; }


		/// <summary>
		/// DirectionMode of bar
		/// </summary>
		public enum DirectionMode : int
		{
			Horizontal = 0,
			Vertical = 1
		}
		private DirectionMode _direction = DirectionMode.Horizontal;
		/// <summary>
		/// The filling direction of progress bar, Horizontal or Vertical
		/// </summary>
		public DirectionMode Direction
		{
			get { return _direction; }
			set { _direction = value; }
		}


        /// <summary>
        /// Update mode of segments
        /// </summary>
        public BlockList.UpdateMode UpdateMode
        {
            get { return _blockList.Update; }
            set { _blockList.Update = value; }
        }

        /// <summary>
        /// The length of segments of progress bar
        /// </summary>

        public int Length
        {
            get { return _blockList.Length; }
            set { _blockList.Length = value; }
        }

        /// <summary>
        /// Get or set filled segments
        /// </summary>
        public int[] FilledSegments
        {
            get { return _blockList.FilledSegments; }
            set { _blockList.FilledSegments = value; }
        }
        /// <summary>
        /// Get or sets the full list of segments
        /// </summary>
        public bool[] FullListSegment
        {
            get { return _blockList.FullListSegment; }
            set { _blockList.FullListSegment = value; }
        }

        /// <summary>
        /// Get or set the block list of segments
        /// </summary>
        public List<Block> BlockList
        {
            get { return _blockList.List; }
            set { _blockList.List = value; }
        }

        public static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            double red = color.R;
            double green = color.G;
            double blue = color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (1 - red) * correctionFactor + red;
                green = (1 - green) * correctionFactor + green;
                blue = (1 - blue) * correctionFactor + blue;
            }

            return new Color(color.A, red, green, blue);
        }
        protected bool OnExposeEvent2(Gdk.EventExpose evnt)
        {
            double borderWidth = 1; // 1px
            var rects = new List<Rectangle>();
            foreach (var block in BlockList)
            {
                rects.Add(new Rectangle());
            }

            DoDraw(evnt.Window, rects.ToArray(), borderWidth);

            return base.OnExposeEvent(evnt);
        }
        private void DoDraw(Gdk.Window window, Rectangle[] rects, double borderWidth)
        {
            Context cr = Gdk.CairoHelper.Create(window);

            // draw progress
            Color color1 = ChangeColorBrightness(this.ForeColor, 0.25f);
            Color color2 = ChangeColorBrightness(this.ForeColor, -0.25f);
            // a linear gradient for progress bar fill
            var grd = new LinearGradient(0, 0, _direction == DirectionMode.Horizontal ? 1 : Allocation.Width, _direction == DirectionMode.Horizontal ? Allocation.Height : 1);
            grd.AddColorStopRgb(0, color1);
            grd.AddColorStopRgb(1, color2);


            if (_direction == DirectionMode.Horizontal)
            {
                DrawRectangleH(cr, 0, Allocation.Height, grd);
            }
            else
            {
                DrawRectangleV(cr, 0, Allocation.Width, grd);
            }
            
            grd.Dispose();


            // draw border around widget
            // - sets line width to 2 points
            cr.LineWidth = 2;
            // - set brush/source color
            cr.SetSourceRGBA(0, 0, 0, 1);
            // - draw a virtual rect
            cr.Rectangle(new Rectangle(cr.LineWidth, cr.LineWidth, Allocation.Width - cr.LineWidth, Allocation.Height - cr.LineWidth));
            // - stroke it.
            cr.Stroke();

            cr.GetTarget().Dispose();
            cr.Dispose();
        }


        private void DrawRectangleH(Context cr, int top, int height, Gradient grd)
        {
            if (_blockList.Length > 0)
            {
                Rectangle[] rects = GetRectanglesH(top, height);
                if (rects.Length > 0) {
                    FillRectangles(cr, rects, grd);
                }
            }
        }

        private void DrawRectangleV(Context cr, int left, int width, Gradient grd)
        {
            if (_blockList.Length > 0)
            {
                Rectangle[] rects = GetRectanglesV(left, width);
                if (rects.Length > 0)
                {
                    FillRectangles(cr, rects, grd);
                }
            }
        }


        private Rectangle[] GetRectanglesH(int top, int height)
        {
            List<Rectangle> rects = new List<Rectangle>();
            float xf = 0, wf = 0, pf = 1;
            int x = 0, // 1px padding is taken into account
                y = top,
                w = 0,
                h = height;


            pf = (float)Allocation.Width / _blockList.Length;
            //h = this.Height;

            foreach (Block block in _blockList.List)
            {
                if (block.PercentProgress > 0)
                {
                    x = Convert.ToInt32(xf);
                    wf = (pf * (block.BlockSize * block.PercentProgress / 100)) + xf - x;
                    w = Convert.ToInt32(wf);

                    rects.Add(new Rectangle(x, y, w, h));
                }

                xf += pf * block.BlockSize;
            }
            return rects.ToArray();
        }

        private Rectangle[] GetRectanglesV(int left, int width)
        {
            List<Rectangle> rects = new List<Rectangle>();
            float yf = 0, hf = 0, pf = 1;
            int x = left, y = 0, w = width, h = 0;

            pf = (float)Allocation.Height / _blockList.Length;
            //w = this.Width;

            foreach (Block block in _blockList.List)
            {
                if (block.PercentProgress > 0)
                {
                    y = Convert.ToInt32(yf);
                    hf = (pf * (block.BlockSize * block.PercentProgress / 100)) + yf - y;
                    h = Convert.ToInt32(hf);

                    rects.Add(new Rectangle(x, y, w, h));
                }

                yf += pf * block.BlockSize;
            }
            return rects.ToArray();
        }
        public static void FillRectangles(Context cr, Rectangle[] rects, Pattern pat)
        {
            foreach(var rect in rects){
                FillRectangle(cr, rect, pat);
            }
        }

        public static void FillRectangle(Context cr, Rectangle rect, Pattern pat)
        {
            cr.SetSource(pat);
            cr.Rectangle(rect);
            cr.Fill();
            cr.MoveTo(new PointD(0,0));
        }
    }

}
