using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Helpers;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MusicMink.Dialogs
{
    class EditAlbumContinuationInfo : ContinuationInfo
    {
        public string AlbumName { get; private set; }
        public string ArtistName { get; private set; }

        public EditAlbumContinuationInfo(string album, string artist)
        {
            AlbumName = album;
            ArtistName = artist;
        }
    }

    public sealed partial class EditAlbum : ContentDialog
    {
        #region Private Vars
        private enum ImagePreviewSource
        {
            None,
            FilePicker,
            LastFM
        };
        private ImagePreviewSource previewSource = ImagePreviewSource.None;

        private AlbumViewModel Album
        {
            get
            {
                return DebugHelper.CastAndAssert<AlbumViewModel>(this.DataContext);
            }
        }

        private StorageFile albumArtStorageFile;
        private string albumArtLastFMPath;
        #endregion

        public EditAlbum(AlbumViewModel albumToEdit)
        {
            this.InitializeComponent();

            this.DataContext = albumToEdit;
        }

        /// <summary>
        /// Used when going back to the dialog after selecting a file for album art
        /// </summary>
        /// <param name="albumToEdit"></param>
        /// <param name="storageFile"></param>
        public EditAlbum(AlbumViewModel albumToEdit, StorageFile storageFile) : this(albumToEdit)
        {
            albumArtStorageFile = storageFile;

            this.Loaded += HandleEditAlbumLoaded;

            DebugHelper.Assert(new CallerInfo(), NavigationManager.Current.ContinuationInfo != null);

            updateArt.IsChecked = true;
            editAlbumArtistName.Text = (NavigationManager.Current.ContinuationInfo as EditAlbumContinuationInfo).ArtistName;
            editAlbumName.Text = (NavigationManager.Current.ContinuationInfo as EditAlbumContinuationInfo).AlbumName;
        }

        #region EventHandlers

        private async void HandleEditAlbumLoaded(object sender, RoutedEventArgs e)
        {
            if (albumArtStorageFile != null)
            {
                using (IRandomAccessStream stream = await albumArtStorageFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);

                    ImagePreview.Source = bitmapImage;

                    previewSource = ImagePreviewSource.FilePicker;
                }
            }
        }

        /// <summary>
        /// Will open the file picker to select local album art. Dialog is then re-opened with the storageFile passed through
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleLaunchFilePickerButtonClick(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            // Save current album name / artist name for when we resume
            NavigationManager.Current.ContinuationInfo = new EditAlbumContinuationInfo(editAlbumName.Text, editAlbumArtistName.Text);

            openPicker.PickSingleFileAndContinue();
        }

        private async void HandleGetLastFMArtButtonClick(object sender, RoutedEventArgs e)
        {
            noArtMessage.Visibility = Visibility.Collapsed;

            lastFMArtButton.IsEnabled = false;

            string LastFMArt = await LastFMManager.Current.GetAlbumArt(editAlbumArtistName.Text, editAlbumName.Text);

            if (!string.IsNullOrEmpty(LastFMArt))
            {
                updateArt.IsChecked = true;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(LastFMArt);

                ImagePreview.Source = bitmapImage;

                albumArtLastFMPath = LastFMArt;

                previewSource = ImagePreviewSource.LastFM;
            }
            else
            {
                noArtMessage.Visibility = Visibility.Visible;
            }

            lastFMArtButton.IsEnabled = true;
        }

        private void HandleContentDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string newAlbumName = editAlbumName.Text;
            string newArtistName = editAlbumArtistName.Text;

            // Didn't actually change
            if (!(newAlbumName == Album.Name && newArtistName == Album.ArtistName))
            {

                AlbumViewModel newAlbum = LibraryViewModel.Current.AlbumSearch(newAlbumName, newArtistName);

                if (newAlbum != null)
                {
                    List<SongViewModel> oldSongs = new List<SongViewModel>(newAlbum.Songs);

                    foreach (SongViewModel song in oldSongs)
                    {
                        song.UpdateAlbum(Album);
                    }

                    LibraryViewModel.Current.RemoveAlbumIfNeeded(newAlbum);
                }

                Album.Name = newAlbumName;
                Album.ArtistName = newArtistName;
            }

            // Art might've though
            if (updateArt.IsChecked.HasValue && updateArt.IsChecked.Value)
            {
                if (previewSource == ImagePreviewSource.LastFM)
                {
                    Album.UpdateArt(albumArtLastFMPath);
                }
                else if (previewSource == ImagePreviewSource.FilePicker)
                {
                    Album.UpdateArt(albumArtStorageFile);
                }
            }
        }

        private void HandleContentDialogSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        #endregion
    }
}
