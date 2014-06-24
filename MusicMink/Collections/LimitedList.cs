﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

using MusicMinkAppLayer.Diagnostics;

using System.Text;

namespace MusicMink.Collections
{
    public class LimitedList<T> : SortedList<T>
    {
        private SortedList<T> fullList;

        private uint _limit;

        public LimitedList(Comparer<T> sortFunction, uint limit)
            : base(sortFunction)
        {
            fullList = new SortedList<T>(sortFunction);

            _limit = limit;
        }

        protected override void InsertItem(int index, T item)
        {
            fullList.Insert(index, item);

            if (this.Items.Count < _limit || _limit == 0)
            {
                base.InsertItem(index, item);
            }
            else
            {
                T lastValue = this.Items[this.Items.Count - 1];

                if (SortFunction.Compare(item, lastValue) < 0)
                {
                    base.InsertItem(index, item);
                    base.RemoveItem(base.Count - 1);
                }
            }
        }

        protected override void ClearItems()
        {
            fullList.Clear();

            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            fullList.RemoveAt(index);

            if (this.Items.Count < _limit && this.Items.Count < fullList.Count)
            {
                // DebugHelper.Assert(new CallerInfo(), this.Items.Count == _limit, "List should've been full");

                T newValue = this.fullList[this.Items.Count];

                base.InsertItem(this.Items.Count, newValue);
            }
        }

        public bool ActuallyContains(T item)
        {
            return fullList.Contains(item);
        }

        internal void UpdateLimit(uint Limit)
        {
            DebugHelper.Assert(new CallerInfo(), this.Count == 0, "Updated limit on non null limited list");

            _limit = Limit;
        }

        override internal void UpdateSortFunction(Comparer<T> newSortFunction)
        {
            DebugHelper.Assert(new CallerInfo(), this.Count == 0, "Updated limit on non null limited list");

            fullList.UpdateSortFunction(newSortFunction);

            base.UpdateSortFunction(newSortFunction);
        }
    }
}
