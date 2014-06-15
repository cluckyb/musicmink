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

        public StorageProviderSong(string path, MusicProperties properties)
        {
            Album = properties.Album;
            AlbumArtist = (properties.AlbumArtist == string.Empty ? properties.Artist : properties.AlbumArtist);
            Artist = properties.Artist;
            Duration = properties.Duration;
            Name = properties.Title;
            PlayCount = 0;
            Rating = properties.Rating / 10;
            Source = path;
            TrackNumber = properties.TrackNumber;
            Origin = StorageProviderSource.Device;
        }
    }
}
