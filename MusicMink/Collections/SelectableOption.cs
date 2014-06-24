using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMink.Collections
{
    public class SelectableOption<T>
    {
        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        T _type;
        public T Type
        {
            get
            {
                return _type;
            }
        }

        public SelectableOption(string n, T t)
        {
            _name = n;
            _type = t;
        }
    }
}
