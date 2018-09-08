using System;
using Gdk;
using Gtk;
namespace MonoDM.Extension.AutoDownloads.UI
{
    public class TimeGridWidget : VBox
    {
        public TimeGridWidget()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var tbl = new Table(DayHourMatrix.HOURS + 1, DayHourMatrix.DAYS + 1, true);
            // hint labels
            for (uint i = 0; i < DayHourMatrix.DAYS; i++)
            {
                tbl.Attach(new Label((i+1).ToString()), 0, 1, i+1, i+2);
            }
            for (uint i = 0; i < DayHourMatrix.HOURS; i++)
            {
                tbl.Attach(new Label((i+1).ToString()), i+1, i+2, 0, 1);
            }
            // insert toggles
            for (uint i = 0; i < DayHourMatrix.DAYS; i++)
            {
                for (uint j = 0; j < DayHourMatrix.HOURS; j++)
                {
                    _timeToggles[i,j] = new MultiStateToggle();
                    tbl.Attach(_timeToggles[i, j], j + 1, j + 2, i + 1, i + 2);
                }
            }

            PackStart(tbl);
            ShowAll();
        }

        // days x hours
        MultiStateToggle[,] _timeToggles = new MultiStateToggle[DayHourMatrix.DAYS, DayHourMatrix.HOURS];
        DayHourMatrix _matrix = new DayHourMatrix();
        
        // never null.
        public DayHourMatrix SelectedTimes
        {
            get
            {
                if (_matrix == null)
                    return _matrix;
                
                for (int i = 0; i < DayHourMatrix.DAYS; i++)
                {
                    for (int j = 0; j < DayHourMatrix.HOURS; j++)
                    {
                        _matrix[(DayOfWeek) i, j] = _timeToggles[i, j].EnableMode;
                    }
                }
                
                return _matrix;
            }
            set
            {
                _matrix = value;
                
                if (_matrix == null)
                    return;
                
                for (int i = 0; i < DayHourMatrix.DAYS; i++)
                {
                    for (int j = 0; j < DayHourMatrix.HOURS; j++)
                    {
                        _timeToggles[i, j].EnableMode = _matrix[(DayOfWeek) i, j];
                    }
                }
            }
        }
    }
}