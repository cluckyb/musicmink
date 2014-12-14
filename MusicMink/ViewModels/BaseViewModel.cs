using MusicMink.Common;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace MusicMink.ViewModels
{
    public abstract class BaseViewModel<T> : NotifyPropertyChangedUI
        where T : RootModel
    {
        protected T rootModel;

        public BaseViewModel(T model)
        {
            rootModel = model;
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
