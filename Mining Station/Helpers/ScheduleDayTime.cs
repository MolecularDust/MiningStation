using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class ScheduleDayTime : NotifyObject, IEquatable<ScheduleDayTime>
    {
        private int _day;
        public int Day
        {
            get { return _day; }
            set
            {
                OnPropertyChanging("Day");
                if (value < 0)
                    _day = 0;
                if (value > 365)
                    _day = 365;
                if (value >= 1 && value <= 365)
                    _day = value;
                OnPropertyChanged("Day");
            }
        }
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

        public ScheduleDayTime() { }

        public ScheduleDayTime(int day, int hour, int minute)
        {
            this.Day = day;
            this.Hour = hour;
            this.Minute = minute;
        }

        public ScheduleDayTime(int day, ScheduleTime scheduleTime)
        {
            this.Day = day;
            this.Hour = scheduleTime.Hour;
            this.Minute = scheduleTime.Minute;
        }

        public ScheduleDayTime Clone()
        {
            var _newTime = new ScheduleDayTime();
            _newTime.Day = this.Day;
            _newTime.Hour = this.Hour;
            _newTime.Minute = this.Minute;
            return _newTime;
        }

        public bool Equals(ScheduleDayTime other)
        {
            if (other == null) return false;
            if ((this.Day == other.Day) && (this.Hour == other.Hour) && (this.Minute == other.Minute))
                return true;
            else return false;
        }

        public static int Difference(ScheduleDayTime time1, ScheduleDayTime time2)
        {
            var span1 = (time1.Day * 1440) + (time1.Hour * 60) + time1.Minute;
            var span2 = (time2.Day * 1440) + (time2.Hour * 60) + time2.Minute;
            return Math.Abs(span1 - span2);
        }
    }
}
