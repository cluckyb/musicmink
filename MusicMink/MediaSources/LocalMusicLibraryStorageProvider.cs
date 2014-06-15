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
            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Loading Folder {0}", folder.Name);

            if (isCanceled) return;

            IReadOnlyList<IStorageItem> fileList = await folder.GetItemsAsync();

            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "{0} Items Found", fileList.Count);

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
            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Loading File {0} Type {1}", file.Name, file.FileType);

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
