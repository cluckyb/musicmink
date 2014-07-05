using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Tables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MusicMinkAppLayer.Models
{
    /// <summary>
    /// This is the main Model that helps provide access to all the other models
    /// </summary>
    public class LibraryModel
    {
        private static LibraryModel _current;
        public static LibraryModel Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new LibraryModel();
                }

                return _current;
            }
        }

        private PerfTracer libraryTracer = new PerfTracer("LibraryModel");

        private LibraryModel()
        {
            libraryTracer.Restart();

            DatabaseManager.Current.Connect();

            libraryTracer.Trace("Database Connected");

            _playQueue = new PlayQueueModel();

            libraryTracer.Trace("PlayQueueModel Created");
        }

        private PlayQueueModel _playQueue;
        public PlayQueueModel PlayQueue
        {
            get
            {
                return _playQueue;
            }
            private set
            {
                _playQueue = value;
            }
        }

        #region Events

        public event EventHandler<AlbumCreatedEventArgs> AlbumCreated;

        public void RaiseAlbumCreated(AlbumModel album)
        {
            if (AlbumCreated != null)
            {
                AlbumCreated(this, new AlbumCreatedEventArgs(album));
            }
        }

        #endregion

        #region Collections

        private Dictionary<int, SongModel> songLookupDictionary = new Dictionary<int, SongModel>();
        private Dictionary<int, AlbumModel> albumLookupDictionary = new Dictionary<int, AlbumModel>();
        private Dictionary<int, ArtistModel> artistLookupDictionary = new Dictionary<int, ArtistModel>();
        private Dictionary<int, PlaylistModel> playlistLookupDictionary = new Dictionary<int, PlaylistModel>();
        private Dictionary<int, MixModel> mixLookupDictionary = new Dictionary<int, MixModel>();


        private ObservableCollection<SongModel> _allSongs = new ObservableCollection<SongModel>();
        public ObservableCollection<SongModel> AllSongs
        {
            get
            {
                return _allSongs;
            }
        }

        private ObservableCollection<AlbumModel> _allAlbums = new ObservableCollection<AlbumModel>();
        public ObservableCollection<AlbumModel> AllAlbums
        {
            get
            {
                return _allAlbums;
            }
        }

        private ObservableCollection<ArtistModel> _allArtists = new ObservableCollection<ArtistModel>();
        public ObservableCollection<ArtistModel> AllArtists
        {
            get
            {
                return _allArtists;
            }
        }

        private ObservableCollection<PlaylistModel> _playlists = new ObservableCollection<PlaylistModel>();
        public ObservableCollection<PlaylistModel> Playlists
        {
            get
            {
                return _playlists;
            }
        }

        private ObservableCollection<MixModel> _mixes = new ObservableCollection<MixModel>();
        public ObservableCollection<MixModel> Mixes
        {
            get
            {
                return _mixes;
            }
        }

        #endregion

        #region Basic Methods

        public void Start()
        {
            libraryTracer.Trace("Starting");

            LoadCollection();

            libraryTracer.Trace("Collection loaded");

            PlayQueue.Start();

            libraryTracer.Trace("Play Queue Loaded");
        }

        private void LoadCollection()
        {
            PerfTracer perfTracer = new PerfTracer("LibraryModel Loading");

            IEnumerable<SongTable> allSongs = DatabaseManager.Current.FetchSongs();
            foreach (SongTable songEntry in allSongs)
            {
                SongModel songModel = new SongModel(songEntry);
                _allSongs.Add(songModel);
                songLookupDictionary.Add(songModel.SongId, songModel);                
            }

            perfTracer.Trace("Songs Added");

            IEnumerable<AlbumTable> allAlbums = DatabaseManager.Current.FetchAlbums();
            foreach (AlbumTable albumEntry in allAlbums)
            {
                AlbumModel albumModel = new AlbumModel(albumEntry);
                _allAlbums.Add(albumModel);
                albumLookupDictionary.Add(albumModel.AlbumId, albumModel);
            }

            perfTracer.Trace("Albums Added");

            IEnumerable<ArtistTable> allArtists = DatabaseManager.Current.FetchArtists();
            foreach (ArtistTable artistEntry in allArtists)
            {
                ArtistModel artistModel = new ArtistModel(artistEntry);
                _allArtists.Add(artistModel);
                artistLookupDictionary.Add(artistModel.ArtistId, artistModel);
            }

            perfTracer.Trace("Artists Added");

            IEnumerable<PlaylistTable> allPlaylists = DatabaseManager.Current.FetchPlaylists();
            foreach (PlaylistTable playlistEntry in allPlaylists)
            {
                PlaylistModel playlistModel = new PlaylistModel(playlistEntry);
                Playlists.Add(playlistModel);
                playlistLookupDictionary.Add(playlistModel.PlaylistId, playlistModel);

                playlistModel.Populate();
            }

            perfTracer.Trace("Playlists Added");

            IEnumerable<MixTable> allMixes = DatabaseManager.Current.FetchMixes();
            foreach (MixTable mixEntry in allMixes)
            {
                MixModel mixModel = new MixModel(mixEntry);
                Mixes.Add(mixModel);
                mixLookupDictionary.Add(mixModel.MixId, mixModel);

                mixModel.Populate();
            }

            perfTracer.Trace("Mixes Added");
        }

        #endregion

        #region Artist

        public ArtistModel SearchArtistByName(string artistName)
        {
            ArtistTable artistTable = DatabaseManager.Current.LookupArtist(artistName);

            if (artistTable == null)
            {
                return null;
            }
            else
            {
                return LookupArtistById(artistTable.ArtistId);
            }
        }

        public ArtistModel LookupArtistByName(string artistName)
        {
            ArtistTable artistTable = DatabaseManager.Current.LookupArtist(artistName);

            if (artistTable == null)
            {
                ArtistTable newArtist = new ArtistTable(artistName);
                DatabaseManager.Current.AddArtist(newArtist);

                ArtistModel artistModel = new ArtistModel(newArtist);
                artistLookupDictionary.Add(artistModel.ArtistId, artistModel);

                return artistModel;
            }
            else
            {
                return LookupArtistById(artistTable.ArtistId);
            }
        }

        public ArtistModel LookupArtistById(int artistId)
        {
            if (artistLookupDictionary.ContainsKey(artistId))
            {
                return artistLookupDictionary[artistId];
            }
            else
            {
                ArtistTable artistTable = DatabaseManager.Current.LookupArtistById(artistId);

                if (artistTable == null)
                {
                    return null;
                }
                else
                {
                    ArtistModel artistModel = new ArtistModel(artistTable);
                    artistLookupDictionary.Add(artistModel.ArtistId, artistModel);

                    return artistModel;
                }
            }
        }

        public void DeleteArtist(int artistId)
        {
            if (artistLookupDictionary.ContainsKey(artistId))
            {
                ArtistModel modelToRemove = artistLookupDictionary[artistId];

                AllArtists.Remove(modelToRemove);

                artistLookupDictionary.Remove(artistId);
            }

            DatabaseManager.Current.DeleteArtist(artistId);
        }

        #endregion

        #region Album

        // Unlike LookupAlbumByName, returns null if it cannot find such an album instead of creating one
        public AlbumModel SearchAlbumByName(string albumName, int albumArtistId)
        {
            AlbumTable albumTable = DatabaseManager.Current.LookupAlbum(albumName, albumArtistId);

            if (albumTable == null)
            {
                return null;
            }
            else
            {
                return LookupAlbumById(albumTable.AlbumId);
            }
        }

        public AlbumModel LookupAlbumByName(string albumName, int albumArtistId)
        {
            AlbumTable albumTable = DatabaseManager.Current.LookupAlbum(albumName, albumArtistId);

            if (albumTable == null)
            {
                AlbumTable newAlbum = new AlbumTable(string.Empty, albumArtistId, albumName, 0);
                DatabaseManager.Current.AddAlbum(newAlbum);

                AlbumModel albumModel = new AlbumModel(newAlbum);
                albumLookupDictionary.Add(albumModel.AlbumId, albumModel);

                RaiseAlbumCreated(albumModel);

                return albumModel;
            }
            else
            {
                return LookupAlbumById(albumTable.AlbumId);
            }
        }

        public AlbumModel LookupAlbumById(int albumId)
        {
            if (albumLookupDictionary.ContainsKey(albumId))
            {
                return albumLookupDictionary[albumId];
            }
            else
            {
                AlbumTable albumTable = DatabaseManager.Current.LookupAlbumById(albumId);

                if (albumTable == null)
                {
                    return null;
                }
                else
                {
                    AlbumModel albumModel = new AlbumModel(albumTable);
                    albumLookupDictionary.Add(albumModel.AlbumId, albumModel);

                    return albumModel;
                }
            }
        }

        public void DeleteAlbum(int albumId)
        {
            if (albumLookupDictionary.ContainsKey(albumId))
            {
                AlbumModel albumToRemove = albumLookupDictionary[albumId];

                albumToRemove.DeleteArt();

                AllAlbums.Remove(albumToRemove);
                albumLookupDictionary.Remove(albumId);
            }

            DatabaseManager.Current.DeleteAlbum(albumId);
        }


        private SongModel LookupSongByPath(string path)
        {
            SongTable songTable = DatabaseManager.Current.LookupSongByPath(path);

            if (songTable == null) return null;

            if (songLookupDictionary.ContainsKey(songTable.SongId)) return songLookupDictionary[songTable.SongId];

            SongModel songModel = new SongModel(songTable);
            _allSongs.Add(songModel);
            songLookupDictionary.Add(songModel.SongId, songModel);

            return songModel;
        }

        #endregion

        #region Song

        // TODO: #18 actually use OriginSource: 
        public SongModel GetSongFromSource(SongOriginSource origin, string path)
        {
            SongModel currentTableEntry = LookupSongByPath(path);

            return currentTableEntry;
        }

        public SongModel AddNewSong(string artist, string album, string albumArtist, string title, string path, SongOriginSource origin, long duration, uint rating, uint trackNumber)
        {
            ArtistModel artistModel = LookupArtistByName(artist);

            ArtistModel albumArtistModel = LookupArtistByName(albumArtist);

            AlbumModel albumModel = LookupAlbumByName(album, albumArtistModel.ArtistId);

            SongModel currentTableEntry = LookupSongByPath(path);

            if (currentTableEntry == null)
            {
                SongTable newSong = new SongTable(albumModel.AlbumId, artistModel.ArtistId, duration, 0, title, origin, 0, rating, path, trackNumber);
                DatabaseManager.Current.AddSong(newSong);

                SongModel songModel = new SongModel(newSong);
                _allSongs.Add(songModel);
                songLookupDictionary.Add(songModel.SongId, songModel);

                return songModel;
            }

            return null;
        }

        public SongModel LookupSongById(int songId)
        {
            if (songLookupDictionary.ContainsKey(songId))
            {
                return songLookupDictionary[songId];
            }
            else
            {
                SongTable songTable = DatabaseManager.Current.LookupSongById(songId);

                if (songTable == null)
                {
                    return null;
                }
                else
                {
                    SongModel songModel = new SongModel(songTable);
                    _allSongs.Add(songModel);
                    songLookupDictionary.Add(songModel.SongId, songModel);

                    return songModel;
                }
            }
        }

        public void DeleteSong(int songId)
        {
            if (songLookupDictionary.ContainsKey(songId))
            {
                SongModel songToRemove = songLookupDictionary[songId];
                AllSongs.Remove(songToRemove);
                songLookupDictionary.Remove(songId);
            }

            DatabaseManager.Current.DeleteSong(songId);
        }

        public SongModel SearchSongByName(string songName, int artistId)
        {
            SongTable songTable = DatabaseManager.Current.LookupSong(songName, artistId);

            if (songTable == null)
            {
                return null;
            }
            else
            {
                return LookupSongById(songTable.SongId);
            }
        }

        #endregion

        #region Playlist

        public void AddPlaylist(string name)
        {
            PlaylistTable table = new PlaylistTable(name);

            DatabaseManager.Current.AddPlaylist(table);

            PlaylistModel playlistModel = new PlaylistModel(table);
            Playlists.Add(playlistModel);
            playlistLookupDictionary.Add(playlistModel.PlaylistId, playlistModel);
        }

        public void DeletePlaylist(int playlistId)
        {
            if (playlistLookupDictionary.ContainsKey(playlistId))
            {
                PlaylistModel playlistModel = playlistLookupDictionary[playlistId];

                Playlists.Remove(playlistModel);
                playlistLookupDictionary.Remove(playlistId);
            }

            DatabaseManager.Current.DeletePlaylist(playlistId);
        }

        #endregion

        #region Mixes

        public void AddMix(string name)
        {
            MixTable table = new MixTable(name, false, 0, MixSortOrder.None, false);

            DatabaseManager.Current.AddMix(table);

            MixModel mixModel = new MixModel(table);
            Mixes.Add(mixModel);
            mixLookupDictionary.Add(mixModel.MixId, mixModel);

            MixEntryTable entryTable = new MixEntryTable(mixModel.MixId, string.Empty, true, MixType.None);
            DatabaseManager.Current.AddMixEntry(entryTable);

            mixModel.AddEntry(entryTable);
        }

        public void DeleteMix(int mixId)
        {
            if (mixLookupDictionary.ContainsKey(mixId))
            {
                MixModel mixModel = mixLookupDictionary[mixId];

                Mixes.Remove(mixModel);
                mixLookupDictionary.Remove(mixId);
            }

            DatabaseManager.Current.DeletePlaylist(mixId);
        }

        #endregion
    }

    public class AlbumCreatedEventArgs : EventArgs
    {
        public AlbumModel NewAlbum { get; private set; }

        public AlbumCreatedEventArgs(AlbumModel album)
            : base()
        {
            this.NewAlbum = album;
        }
    }
}
