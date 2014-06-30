using System;
using Windows.Storage.FileProperties;

namespace MusicMink.MediaSources
{
    public enum StorageProviderSource
    {
        Device,
        OneDrive
    }

    public class StorageProviderSong
    {
        public string Album { get; private set; }
        public string AlbumArtist { get; private set; }
        public string Artist { get; private set; }
        public TimeSpan Duration { get; private set; }
        public DateTime LastPlayed { get; private set; }
        public string Name { get; private set; }
        public uint PlayCount { get; private set; }
        public uint Rating { get; private set; }
        public string Source { get; private set; }
        public uint TrackNumber { get; private set; }
        public StorageProviderSource Origin { get; private set; }

        // TODO: this better
        public bool IsDifferent { get; private set; } 

        public StorageProviderSong(string path, MusicProperties properties, string artistName)
        {
            Album = properties.Album;
            AlbumArtist = (properties.AlbumArtist == string.Empty ? artistName : properties.AlbumArtist);
            Artist = artistName; // For some reason this is AlbumArtist instead of artist, so we need to get the artist on our own
            Duration = properties.Duration;
            Name = properties.Title;
            PlayCount = 0;
            Rating = properties.Rating / 10;
            Source = path;
            TrackNumber = properties.TrackNumber;
            Origin = StorageProviderSource.Device;
            IsDifferent = artistName != properties.Artist;
        }
    }
}
