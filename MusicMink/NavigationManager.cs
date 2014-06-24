using MusicMink.MediaSources;
using MusicMink.Pages;
using MusicMinkAppLayer.Diagnostics;
using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;

namespace MusicMink
{
    enum NavigationLocation
    {
        Home,

        AlbumList,
        ArtistList,
        PlaylistList,
        SongList,
        MixList,

        AlbumPage,
        ArtistPage,
        PlaylistPage,
        MixPage,

        SettingsPage,

        ManageLibrary,
    }

    abstract class ContinuationInfo { }

    /// <summary>
    /// Used to help cleanly handle navigation across the root app frame.
    /// Also helps manage ContinuationInfo (information passed through app sessions
    /// like with the AlbumArt File Picker)
    /// </summary>
    class NavigationManager
    {
        private static NavigationManager _current;
        public static NavigationManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new NavigationManager();
                }

                return _current;
            }
        }

        private Frame mainNavigationFrame;

        public ContinuationInfo ContinuationInfo = null;

        public void GoHome()
        {
            while (mainNavigationFrame.CanGoBack)
            {
                mainNavigationFrame.GoBack();
            }
        }
        
        public void Navigate(NavigationLocation type, object parameter = null)
        {
            mainNavigationFrame.Navigate(NavigationLocationToPageType(type), parameter);
        }

        private Type NavigationLocationToPageType(NavigationLocation location)
        {
            switch (location)
            {
                case NavigationLocation.AlbumList:
                    return typeof(AlbumList);
                case NavigationLocation.AlbumPage:
                    return typeof(AlbumPage);
                case NavigationLocation.ArtistList:
                    return typeof(ArtistList);
                case NavigationLocation.ArtistPage:
                    return typeof(ArtistPage);
                case NavigationLocation.Home:
                    return typeof(HomePage);
                case NavigationLocation.ManageLibrary:
                    return typeof(ManageLibrary);
                case NavigationLocation.MixList:
                    return typeof(MixList);
                case NavigationLocation.MixPage:
                    return typeof(MixPage);
                case NavigationLocation.PlaylistList:
                    return typeof(PlaylistList);
                case NavigationLocation.PlaylistPage:
                    return typeof(PlaylistPage);
                case NavigationLocation.SettingsPage:
                    return typeof(Settings);
                case NavigationLocation.SongList:
                    return typeof(SongList);
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected NavigationLocation {0}", location);
                    return typeof(HomePage);
            }
        }


        internal void SetRootFrame(Frame MainContentFrame)
        {
            mainNavigationFrame = MainContentFrame;
        }

        internal void HandleContinuation(IContinuationActivatedEventArgs continuationEventArgs)
        {
            if (mainNavigationFrame != null && mainNavigationFrame.Content is AlbumPage && continuationEventArgs is FileOpenPickerContinuationEventArgs)
            {
                AlbumPage currentAlbumPage = DebugHelper.CastAndAssert<AlbumPage>(mainNavigationFrame.Content);
                FileOpenPickerContinuationEventArgs filePickerOpenArgs = DebugHelper.CastAndAssert<FileOpenPickerContinuationEventArgs>(continuationEventArgs);

                currentAlbumPage.HandleFilePickerLaunch(filePickerOpenArgs);
            }
            else if (mainNavigationFrame != null && mainNavigationFrame.Content is ManageLibrary && continuationEventArgs is FileOpenPickerContinuationEventArgs)
            {
                ManageLibrary scanPhonePage = DebugHelper.CastAndAssert<ManageLibrary>(mainNavigationFrame.Content);
                FileOpenPickerContinuationEventArgs filePickerOpenArgs = DebugHelper.CastAndAssert<FileOpenPickerContinuationEventArgs>(continuationEventArgs);

                // TODO: #12 Figure out why this hangs on "resuming" 
                MediaImportManager.Current.HandleFilePickerLaunch(filePickerOpenArgs);
            }
        }
    }
}
