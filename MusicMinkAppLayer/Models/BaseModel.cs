using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using MusicMinkAppLayer.Tables;
using MusicMinkAppLayer.Diagnostics;

namespace MusicMinkAppLayer.Models
{
    /// <summary>
    /// All models need to extend RootModel so that ViewModels can connect only to Models
    /// bot not all models need a table, so RootModel provides the basics and the BaseModel
    /// is for everything that also uses a table
    /// </summary>
    public abstract class RootModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Uses reflection magic to provide quick easy set/get helpers that work
    /// with the SQLLite database
    /// </summary>
    /// <typeparam name="T">The table type associated with this model</typeparam>
    public abstract class BaseModel<T> : RootModel
        where T : BaseTable
    {
        protected T rootTable;

        public BaseModel(T table)
        {
            rootTable = table;
        }

        protected void SetTableField<U>(string tableProperty, U value, string property)
        {
            PropertyInfo propertyInfo = typeof(T).GetRuntimeProperty(tableProperty);

            DebugHelper.Assert(new CallerInfo(), propertyInfo != null, "Table {0} does not contain property {1}", typeof(T), tableProperty);

            if (propertyInfo != null)
            {
                U field = DebugHelper.CastAndAssert<U>(propertyInfo.GetValue(rootTable));

                if (!EqualityComparer<U>.Default.Equals(field, value))
                {
                    propertyInfo.SetValue(rootTable, value);
                    DatabaseManager.Current.Update(rootTable);
                    NotifyPropertyChanged(property);
                }
            }
        }

        protected U GetTableField<U>(string tableProperty)
        {
            PropertyInfo propertyInfo = typeof(T).GetRuntimeProperty(tableProperty);

            if (propertyInfo != null)
            {
                return DebugHelper.CastAndAssert<U>(propertyInfo.GetValue(rootTable));
            }
            else
            {
                return default(U);
            }
        }
    }
}
