using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MusicMink.Collections
{
    /// <summary>
    /// A sorted ObserveableCollection of entries all grouped under a single character Title
    /// Used by AlphaGroupedObservableCollection, doesn't do much on its own
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AlphaGroupedObservableCollectionEntry<T> : ObservableCollection<T>
    {
        public List<T> sortedSync;

        public char Title
        {
            get;
            set;
        }

        public AlphaGroupedObservableCollectionEntry(char title)
        {
            this.Title = title;

            sortedSync = new List<T>();
        }

        public bool HasItems
        {
            get { return (Count > 0); }
        }

        public void Reset()
        {
            if (Count > 0)
            {
                sortedSync.Clear();
                this.Clear();
            }
        }
    }

    /// <summary>
    /// Collection of 28 AlphaGroupedObservableCollectionEntries (one for each english character,
    /// one for digits, one for everything else). Each entry stays sorted, and when items are inserted
    /// they are put into the proper group in the proper spot.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AlphaGroupedObservableCollection<T> : INotifyPropertyChanged
    {
        public class Properties
        {
            public const string Root = "Root";
        }

        private ObservableCollection<AlphaGroupedObservableCollectionEntry<T>> _root;
        public ObservableCollection<AlphaGroupedObservableCollectionEntry<T>> Root
        {
            get { return _root; }
            protected set
            {
                if (_root != value)
                {
                    _root = value;
                    NotifyPropertyChanged(Properties.Root);
                }
            }
        }

        public AlphaGroupedObservableCollection()
        {
            Root = new ObservableCollection<AlphaGroupedObservableCollectionEntry<T>>();

            string Alpha = "#ABCDEFGHIJKLMNOPQRSTUVWXYZ?";

            foreach (char c in Alpha)
            {
                Root.Add(new AlphaGroupedObservableCollectionEntry<T>(c));
            }
        }

        public void Add(T t, string key)
        {
            int j = 27;

            if (key.Length > 0)
            {
                j = charToIndex(key[0]); ;
            }

            if (j >= 0)
            {
                int index = Root[j].sortedSync.BinarySearch(t);

                if (index < 0) index = ~index;

                Root[j].Insert(index, t);
                Root[j].sortedSync.Insert(index, t);
            }
        }

        public void Remove(T t, string key)
        {
            int j = 27;

            if (key.Length > 0)
            {
                j = charToIndex(key[0]); ;
            }

            if (j >= 0)
            {
                int index = Root[j].sortedSync.BinarySearch(t);

                if (index < 0) index = ~index;

                Root[j].Remove(t);
                Root[j].sortedSync.Remove(t);
            }
        }

        private int charToIndex(char c)
        {
            if (c >= '0' && c <= '9') return 0;
            if (c >= 'A' && c <= 'Z') return (1 + c - 'A');
            if (c >= 'a' && c <= 'z') return (1 + c - 'a');
            return 27;
        }

        internal void Clear()
        {
            if (Root.Count == 0) return;

            foreach (AlphaGroupedObservableCollectionEntry<T> entry in Root)
            {
                entry.Reset();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
