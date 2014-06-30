using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MusicMink.Collections
{
    /// <summary>
    /// Observable Collection of type T that is constructed by taking a another
    /// ObservableCollection of type and a U => T function. Each entry in the
    /// other collection gets mapped to its own entry here.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class ObservableCopyCollection<T, U> : ObservableCollection<T>
    {
        public bool IsRootChanging = false;

        public bool IsEndChanging = false;

        private Func<U, T> Create;

        protected override void ClearItems()
        {
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
        }

        public ObservableCopyCollection(ObservableCollection<U> baseCollection, Func<U, T> create)
        {
            foreach (U item in baseCollection)
            {
                T newItem = create(item);
                this.Add(newItem);
            }

            baseCollection.CollectionChanged += RootCollectionChanged;

            Create = create;
        }

        void RootCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // If the resulting list changes and we need to propogate the result down to the base collection
            // we don't want the results bubbling back up
            if (IsEndChanging) return;

            IsRootChanging = true;

            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                int index = e.OldStartingIndex;

                if (index != -1)
                {
                    foreach (U item in e.OldItems)
                    {
                        this.RemoveAt(index);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                int index = e.NewStartingIndex;

                foreach (U item in e.NewItems)
                {
                    this.Insert(index, Create(item));
                    index++;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                int newIndex = e.NewStartingIndex;
                int oldIndex = e.OldStartingIndex;

                foreach (U item in e.NewItems)
                {
                    this.Move(oldIndex, newIndex);
                    newIndex++;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Clear();
            }

            IsRootChanging = false;
        }
    }
}
