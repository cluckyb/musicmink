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

        public async Task SyncStorageSolution()
        {
            await LoadFolder(KnownFolders.MusicLibrary);
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
                
                if (this.TrackScanned != null)
                {
                    this.TrackScanned(this, new TrackScannedEventArgs(new StorageProviderSong(file.Path, musicProperties)));
                }
            }
        }

        public event TrackScannedEventHandler TrackScanned;
    }
}
