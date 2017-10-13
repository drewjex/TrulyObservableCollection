using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;

namespace Common.WPF
{
    public class TrulyObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public Action<object, PropertyChangedEventArgs> Handler;

        public TrulyObservableCollection() : base()
        {
            CollectionChanged += new NotifyCollectionChangedEventHandler(TrulyObservableCollection_CollectionChanged);
        }

        public TrulyObservableCollection(IEnumerable<T> collection)
        {
            CopyFrom(collection);

            CollectionChanged += new NotifyCollectionChangedEventHandler(TrulyObservableCollection_CollectionChanged);
        }

        public TrulyObservableCollection(IEnumerable<T> collection, Action<object, PropertyChangedEventArgs> handler)
        {
            CopyFrom(collection);

            Handler = handler;
            foreach (T item in Items)
            {
                ((INotifyPropertyChanged)item).PropertyChanged += new PropertyChangedEventHandler(Handler);
                RecursivelyApplyEventHandler(item);
            }

            CollectionChanged += new NotifyCollectionChangedEventHandler(TrulyObservableCollection_CollectionChanged);
        }

        public void RecursivelyApplyEventHandler(INotifyPropertyChanged item) 
        {
            foreach (PropertyInfo prop in item.GetType().GetProperties())
            {
                if (!prop.PropertyType.IsPrimitive && Convert.GetTypeCode(prop.GetValue(item)) == TypeCode.Object)
                {
                    (prop.GetValue(item) as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler(Handler);
                    RecursivelyApplyEventHandler((INotifyPropertyChanged)prop.GetValue(item));
                }
            }
        }

        private void CopyFrom(IEnumerable<T> collection)
        {
            IList<T> items = Items;
            if (collection != null && items != null)
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        items.Add(enumerator.Current);
                    }
                }
            }
        }

        void TrulyObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= new PropertyChangedEventHandler(Handler);
                    RecursivelyApplyEventHandler((INotifyPropertyChanged)item);
                }
            }
            if (e.NewItems != null)
            {
                foreach (Object item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += new PropertyChangedEventHandler(Handler);
                    RecursivelyApplyEventHandler((INotifyPropertyChanged)item);
                }
            }

        }
    }
}
