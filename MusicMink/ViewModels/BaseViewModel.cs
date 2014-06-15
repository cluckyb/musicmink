using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace MusicMink.ViewModels
{
    public abstract class BaseViewModel<T> : INotifyPropertyChanged
        where T : RootModel
    {
        protected T rootModel;

        public BaseViewModel(T model)
        {
            rootModel = model;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void SetModelField<U>(string modelProperty, U value, string property)
        {
            PropertyInfo propertyInfo = typeof(T).GetRuntimeProperty(modelProperty);

            DebugHelper.Assert(new CallerInfo(), propertyInfo != null, "Model {0} does not contain property {1}", typeof(T), modelProperty);

            if (propertyInfo != null)
            {
                U field = DebugHelper.CastAndAssert<U>(propertyInfo.GetValue(rootModel));

                if (!EqualityComparer<U>.Default.Equals(field, value))
                {
                    propertyInfo.SetValue(rootModel, value);
                    NotifyPropertyChanged(property);
                }
            }
        }

        protected U GetModelField<U>(string modelProperty)
        {
            PropertyInfo propertyInfo = typeof(T).GetRuntimeProperty(modelProperty);

            DebugHelper.Assert(new CallerInfo(), propertyInfo != null, "Model {0} does not contain property {1}", typeof(T), modelProperty);

            if (propertyInfo != null)
            {
                return DebugHelper.CastAndAssert<U>(propertyInfo.GetValue(rootModel));
            }
            else
            {
                return default(U);
            }
        }
    }
}
