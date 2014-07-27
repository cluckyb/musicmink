using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace MusicMink.MediaSources
{
    public class LocalMusicLibraryStorageProvider : IStorageProvider
    {
        private List<StorageProviderSong> results = new List<StorageProviderSong>();

        private bool isCanceled = false;

        private List<string> RealMusicProperties = new List<string>(){"System.Music.Artist"};

        public async Task SyncStorageSolution()
        {
            await SyncStorageSolution(KnownFolders.MusicLibrary);
        }

        public async Task SyncStorageSolution(StorageFolder target)
        {
            await LoadFolder(target);
        }

        public void Cancel()
        {
            isCanceled = true;
        }

        private async Task LoadFolder(StorageFolder folder)
        {
            if (isCanceled) return;

            IReadOnlyList<IStorageItem> fileList = await folder.GetItemsAsync();

            foreach (IStorageItem storageItem in fileList)
            {
                if (storageItem.IsOfType(StorageItemTypes.File))
                {
                    StorageFile storageFile = DebugHelper.CastAndAssert<StorageFile>(storageItem);

                    await LoadFile(storageFile);
                }
                else if (storageItem.IsOfType(StorageItemTypes.Folder))
                {
                    StorageFolder storageFolder = DebugHelper.CastAndAssert<StorageFolder>(storageItem);

                    await LoadFolder(storageFolder);
                }
            }
        }

        private async Task LoadFile(StorageFile file)
        {
            if (isCanceled) return;

            if (Utilities.IsSupportedFileType(file.FileType))
            {
                StorageItemContentProperties fileProperties = file.Properties;

                MusicProperties musicProperties = await fileProperties.GetMusicPropertiesAsync();

                IDictionary<string, object> artistProperties = await fileProperties.RetrievePropertiesAsync(RealMusicProperties);

                object artists = null;
                artistProperties.TryGetValue("System.Music.Artist", out artists);

                string[] artistsAsArray = DebugHelper.CastAndAssert<string[]>(artists);

                string artistName = string.Empty;

                if (artistsAsArray != null && artistsAsArray.Length > 0)
                {
                    artistName = artistsAsArray[0];
                }

                if (this.TrackScanned != null)
                {
                    this.TrackScanned(this, new TrackScannedEventArgs(new StorageProviderSong(file.Path, musicProperties, artistName)));
                }
            }
        }

        public event TrackScannedEventHandler TrackScanned;
    }
}
