using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class ObservablePairCollection<TKey, TValue> : ObservableCollection<KeyValueEditablePair<TKey, TValue>>, IDictionary<TKey, TValue>, INotifyPropertyChanged, INotifyPropertyChanging where TValue : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";
        private const string KeysName = "Keys";
        private const string ValuesName = "Values";

        public ObservablePairCollection()
        : base()
        {
        }

        public ObservablePairCollection(IEnumerable<KeyValueEditablePair<TKey, TValue>> enumerable)
        : base(enumerable)
        {
        }

        public ObservablePairCollection(List<KeyValueEditablePair<TKey, TValue>> list)
        : base(list)
        {
        }

        public ObservablePairCollection(IDictionary<TKey, TValue> dictionary)
        {
            foreach (var kv in dictionary)
            {
                Add(new KeyValueEditablePair<TKey, TValue>(kv));
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;
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

        private void OnPropertyChanged()
        {
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnPropertyChanged(KeysName);
            OnPropertyChanged(ValuesName);
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (TryGetValue(key, out value))
                    return value;
                else
                    throw new KeyNotFoundException();

            }
            set
            {
                foreach (var item in base.Items)
                {
                    if (EqualityComparer<TKey>.Default.Equals(item.Key, key))
                    {
                        item.Value = value;
                        return;
                    }
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> keys = new List<TKey>();
                foreach (var item in base.Items)
                {
                    keys.Add(item.Key);
                }
                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>();
                foreach (var item in base.Items)
                {
                    values.Add(item.Value);
                }
                return values;
            }
        }

        public new int Count
        {
            get { return base.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(TKey key, TValue value)
        {
            Add(new KeyValueEditablePair<TKey, TValue>(key, value));
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public new void Clear()
        {
            foreach (var item in this)
                this.Remove(item);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            foreach (var innerItem in base.Items)
            {
                if (EqualityComparer<TKey>.Default.Equals(item.Key, innerItem.Key) && EqualityComparer<TValue>.Default.Equals(item.Value, innerItem.Value))
                {
                    return true;
                }
            }
            return false;
        }


        public bool ContainsKey(TKey key)
        {
            foreach (var item in base.Items)
            {
                if (EqualityComparer<TKey>.Default.Equals(item.Key, key))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }


        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>();
            foreach (var item in base.Items)
            {
                list.Add(new KeyValuePair<TKey, TValue>(item.Key, item.Value));
            }
            return list.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            if (!ContainsKey(key))
                return false;
            KeyValueEditablePair<TKey, TValue> pairToRemove = new KeyValueEditablePair<TKey, TValue>();
            foreach (var item in base.Items)
            {
                if (EqualityComparer<TKey>.Default.Equals(item.Key, key))
                {
                    pairToRemove = item;
                    break;
                }
            }
            this.Remove(pairToRemove);
            return true;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!ContainsKey(item.Key))
                return false;
            KeyValueEditablePair<TKey, TValue> pairToRemove = new KeyValueEditablePair<TKey, TValue>();
            foreach (var innerItem in base.Items)
            {
                if (EqualityComparer<TKey>.Default.Equals(item.Key, innerItem.Key))
                {
                    pairToRemove = innerItem;
                    break;
                }
            }
            this.Remove(pairToRemove);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!this.ContainsKey(key))
            {
                value = default(TValue);
                return false;
            }
            foreach (var item in base.Items)
            {
                if (EqualityComparer<TKey>.Default.Equals(item.Key, key))
                {
                    value = item.Value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        public bool ContainsValue(TValue value)
        {
            foreach (var item in base.Items)
            {
                if (EqualityComparer<TValue>.Default.Equals(value, item.Value))
                {
                    return true;
                }
            }
            return false;
        }

        public void Sort()
        {
            var ordered = base.Items.OrderBy(pair => pair.Key).ToList();
            this.Clear();
            foreach (var kv in ordered)
                this.Add(kv);
        }
    }
}
