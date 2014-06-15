using MusicMinkAppLayer.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MusicMink.Collections
{
    /// <summary>
    /// A sorted observable collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortedList<T> : ObservableCollection<T>
    {
        private Comparer<T> _sortFunction;

        public SortedList(Comparer<T> sortFunction)
        {
            SortFunction = sortFunction;
        }

        public Comparer<T> SortFunction
        {
            get
            {
                return _sortFunction;
            }
            set
            {
                if (_sortFunction != value)
                {
                    _sortFunction = value;
                }
            }
        }

        protected override void InsertItem(int index, T item)
        {
            if (SortFunction == null)
            {
                base.InsertItem(index, item);
            }
            else
            {
                int lowerBound = 0;
                int upperBound = this.Count;

                while (lowerBound < upperBound)
                {
                    int midpoint = (lowerBound + upperBound) / 2;
                    if (SortFunction.Compare(item, this[midpoint]) < 0)
                    {
                        upperBound = midpoint;
                    }
                    else if (SortFunction.Compare(item, this[midpoint]) > 0)
                    {
                        if (lowerBound == midpoint)
                        {
                            lowerBound++;
                        }
                        else
                        {
                            lowerBound = midpoint;
                        }
                    }
                    else
                    {
                        lowerBound = midpoint;
                        upperBound = midpoint;
                    }
                }

                base.InsertItem(lowerBound, item);
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new System.NotSupportedException();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
        }

        protected override void SetItem(int index, T item)
        {
            this.RemoveItem(index);
            this.Insert(index, item);
        }

        virtual internal void UpdateSortFunction(Comparer<T> sortFunction)
        {
            DebugHelper.Assert(new CallerInfo(), this.Count == 0, "Updated limit on non null limited list");

            _sortFunction = sortFunction;
        }
    }
}
