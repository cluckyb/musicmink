using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;


namespace MusicMink.Common
{
    public abstract class NotifyPropertyChangedUI : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        delegate()
                        {
                            handler(this, new PropertyChangedEventArgs(propertyName));
                        }
                    );
                }
            }
        }
    }
}
