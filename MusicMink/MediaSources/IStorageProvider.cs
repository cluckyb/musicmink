using System;
using System.Threading.Tasks;

namespace MusicMink.MediaSources
{
    interface IStorageProvider
    {
        Task SyncStorageSolution();

        void Cancel();

        event TrackScannedEventHandler TrackScanned;
    }

    public delegate void TrackScannedEventHandler(object sender, TrackScannedEventArgs e);

    public class TrackScannedEventArgs : EventArgs
    {
        public StorageProviderSong ScannedTrack { get; private set; }

        public TrackScannedEventArgs(StorageProviderSong track)
            : base()
        {
            this.ScannedTrack = track;
        }
    }
}
