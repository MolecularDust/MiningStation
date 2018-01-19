using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class KeyValueEditablePair<TKey, TValue> : NotifyObject where TValue : INotifyPropertyChanging, INotifyPropertyChanged
    {
        protected TKey _key;
        protected TValue _value;

        public TKey Key
        {
            get { return _key; }
            set
            {
                if (
                    (_key == null && value != null)
                    || (_key != null && value == null)
                    || !_key.Equals(value))
                {
                    OnPropertyChanging("Key");
                    _key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        public TValue Value
        {
            get { return _value; }
            set
            {
                if (
                    (_value == null && value != null)
                    || (_value != null && value == null)
                    || (_value != null && !_value.Equals(value)))
                {
                    OnPropertyChanging("Value");
                    _value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        public KeyValueEditablePair()
        {
        }

        public KeyValueEditablePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
            Value.PropertyChanging += Value_PropertyChanging;
            Value.PropertyChanged += Value_PropertyChanged;
        }

        public KeyValueEditablePair(KeyValuePair<TKey, TValue> kv)
        {
            Key = kv.Key;
            Value = kv.Value;
            Value.PropertyChanging += Value_PropertyChanging;
            Value.PropertyChanged += Value_PropertyChanged;
        }

        private void Value_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            Debug.WriteLine("+++ Value_PropertyChanging " + e.PropertyName);
            OnPropertyChanging(e.PropertyName);
        }

        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
    }
}
