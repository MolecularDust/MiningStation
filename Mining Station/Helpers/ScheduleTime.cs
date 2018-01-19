using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class ScheduleTime : NotifyObject, IEquatable<ScheduleTime>
    {
        private int _hour;
        public int Hour
        {
            get { return _hour; }
            set
            {
                OnPropertyChanging("Hour");
                if (value > 23)
                    _hour = 23;
                else _hour = value;
                OnPropertyChanged("Hour");
            }
        }
        private int _minute;
        public int Minute
        {
            get { return _minute; }
            set
            {
                OnPropertyChanging("Minute");
                if (value > 59)
                    _minute = 59;
                else _minute = value;
                OnPropertyChanged("Minute");
            }
        }

        public ScheduleTime() { }

        public ScheduleTime(int hour, int minute)
        {
            this.Hour = hour;
            this.Minute = minute;
        }

        public ScheduleTime Clone()
        {
            var _newTime = new ScheduleTime();
            _newTime.Hour = this.Hour;
            _newTime.Minute = this.Minute;
            return _newTime;
        }

        public bool Equals(ScheduleTime other)
        {
            if (other == null) return false;
            if ((this.Hour == other.Hour) && (this.Minute == other.Minute))
                return true;
            else return false;
        }

        public static int Difference(ScheduleTime time1, ScheduleTime time2)
        {
            var span1 = (time1.Hour * 60) + time1.Minute;
            var span2 = (time2.Hour * 60) + time2.Minute;
            return Math.Abs(span1 - span2);
        }
    }
}
