using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMink.Common
{
    public class GenericComparer<T> : Comparer<T>
        where T : IComparable
    {
        public override int Compare(T x, T y)
        {
            return x.CompareTo(y);
        }
    }
}
