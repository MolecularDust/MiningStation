using System.ComponentModel;

namespace Mining_Station
{
    public abstract class NotifyObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        protected void OnPropertyChanging(string property)
        {
            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(property));
        }

        public void RaiseProperychanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        public void RaiseProperychanging(string propertyName)
        {
            OnPropertyChanging(propertyName);
        }
    }
}
