using MusicMinkAppLayer.Diagnostics;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Manages the SQLite database. All updates to the database should be done through this class
    /// </summary>
    internal class DatabaseManager
    {
        private static string DB_PATH = Path.Combine(ApplicationData.Current.LocalFolder.Path, "MusicMinkDB.sqlite");
        private static int DB_VERSION = 1;
        private static string DB_VERSION_KEY = "MAIN_DATABASE_VERSION";

        public SQLiteConnection sqlConnection;

        private static DatabaseManager _current; 
        public static DatabaseManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new DatabaseManager();
                }

                return _current;
            }
        }

        public void Connect()
        {
            sqlConnection = new SQLiteConnection(DB_PATH);

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(DB_VERSION_KEY))
            {
                ApplicationData.Current.LocalSettings.Values.Add(DB_VERSION_KEY, 0);
            }

            int currentDatabaseVersion = DebugHelper.CastAndAssert<int>(ApplicationData.Current.LocalSettings.Values[DB_VERSION_KEY]);

            if (currentDatabaseVersion < 1)
            {
                sqlConnection.CreateTable<ArtistTable>();
                sqlConnection.CreateTable<AlbumTable>();
                sqlConnection.CreateTable<SongTable>();
                sqlConnection.CreateTable<PlayQueueEntryTable>();
                sqlConnection.CreateTable<PlaylistTable>();
                sqlConnection.CreateTable<PlaylistEntryTable>();
                sqlConnection.CreateTable<HistoryTable>();
            }

            if (currentDatabaseVersion < DB_VERSION)
            {
                ApplicationData.Current.LocalSettings.Values[DB_VERSION_KEY] = DB_VERSION;
            }
        }

        public void Disconnect()
        {
            if (sqlConnection != null)
            {
                sqlConnection.Close();

                sqlConnection = null;
            }
        }

        #region FETCH

        internal List<SongTable> FetchSongs()
        {
            return sqlConnection.Table<SongTable>().ToList<SongTable>();
        }

        internal List<AlbumTable> FetchAlbums()
        {
            return sqlConnection.Table<AlbumTable>().ToList<AlbumTable>();
        }

        internal List<ArtistTable> FetchArtists()
        {
            return sqlConnection.Table<ArtistTable>().ToList<ArtistTable>();
        }

        internal List<PlayQueueEntryTable> FetchPlayQueueEntries()
        {
            return sqlConnection.Table<PlayQueueEntryTable>().ToList<PlayQueueEntryTable>();
        }

        internal List<PlaylistTable> FetchPlaylists()
        {
            return sqlConnection.Table<PlaylistTable>().ToList<PlaylistTable>();
        }

        internal List<PlaylistEntryTable> FetchPlaylistEntriesForPlaylist(int PlaylistId)
        {
            return sqlConnection.Query<PlaylistEntryTable>("SELECT * FROM PlaylistEntryTable WHERE PlaylistId = ?", PlaylistId).ToList<PlaylistEntryTable>();
        }

        internal List<PlaylistEntryTable> FetchPlaylistEntriesForPlaylistAndSongId(int PlaylistId, int songId)
        {
            return sqlConnection.Query<PlaylistEntryTable>("SELECT * FROM PlaylistEntryTable WHERE PlaylistId = ? AND SongId = ?", PlaylistId, songId).ToList<PlaylistEntryTable>();
        }

        internal List<HistoryTable> FetchHistoryItems()
        {
            return sqlConnection.Table<HistoryTable>().ToList<HistoryTable>();
        }

        #endregion

        #region ADD

        internal void AddSong(SongTable songTable)
        {
            sqlConnection.Insert(songTable);
        }

        internal int AddArtist(ArtistTable artistTable)
        {
            return sqlConnection.Insert(artistTable);
        }

        internal int AddAlbum(AlbumTable albumTable)
        {
            return sqlConnection.Insert(albumTable);
        }

        internal int AddPlaylistEntry(PlaylistEntryTable newPlaylistEntry)
        {
            return sqlConnection.Insert(newPlaylistEntry);
        }

        internal int AddPlaylist(PlaylistTable newPlaylist)
        {
            return sqlConnection.Insert(newPlaylist);
        }

        internal int AddPlayQueueEntry(PlayQueueEntryTable newPlayQueueEntry)
        {
            return sqlConnection.Insert(newPlayQueueEntry);
        }

        internal int AddHistoryItem(HistoryTable newHistoryItem)
        {
            return sqlConnection.Insert(newHistoryItem);
        }

        #endregion

        #region LOOKUP

        internal SongTable LookupSong(string name, int artistId)
        {
            return sqlConnection.Query<SongTable>("SELECT * FROM SongTable WHERE Name = ? AND ArtistId = ?", name, artistId).FirstOrDefault<SongTable>();
        }

        internal SongTable LookupSongByPath(string path)
        {
            return sqlConnection.Query<SongTable>("SELECT * FROM SongTable WHERE Source = ?", path).FirstOrDefault<SongTable>();
        }

        internal SongTable LookupSongById(int songId)
        {
            return sqlConnection.Query<SongTable>("SELECT * FROM SongTable WHERE SongId = ?", songId).FirstOrDefault<SongTable>();
        }

        internal ArtistTable LookupArtist(string name)
        {
            return sqlConnection.Query<ArtistTable>("SELECT * FROM ArtistTable WHERE Name = ?", name).FirstOrDefault<ArtistTable>();
        }

        internal ArtistTable LookupArtistById(int artistId)
        {
            return sqlConnection.Query<ArtistTable>("SELECT * FROM ArtistTable WHERE ArtistId = ?", artistId).FirstOrDefault<ArtistTable>();
        }

        internal AlbumTable LookupAlbum(string albumName, int albumArtistId)
        {
            return sqlConnection.Query<AlbumTable>("SELECT * FROM AlbumTable WHERE Name = ? AND ArtistId = ?", albumName, albumArtistId).FirstOrDefault<AlbumTable>();
        }

        internal AlbumTable LookupAlbumById(int albumId)
        {
            return sqlConnection.Query<AlbumTable>("SELECT * FROM AlbumTable WHERE AlbumId = ?", albumId).FirstOrDefault<AlbumTable>();
        }

        internal PlayQueueEntryTable LookupPlayQueueEntryHead()
        {
            return sqlConnection.Query<PlayQueueEntryTable>("SELECT * FROM PlayQueueEntryTable WHERE PrevId = 0").FirstOrDefault<PlayQueueEntryTable>();
        }

        internal PlayQueueEntryTable LookupPlayQueueEntryById(int rowId)
        {
            return sqlConnection.Query<PlayQueueEntryTable>("SELECT * FROM PlayQueueEntryTable WHERE RowId = ?", rowId).FirstOrDefault<PlayQueueEntryTable>();
        }

        internal List<PlayQueueEntryTable> LookupPlayQueueEntryBySongId(int songId)
        {
            return sqlConnection.Query<PlayQueueEntryTable>("SELECT * FROM PlayQueueEntryTable WHERE SongId = ?", songId);
        }

        #endregion

        #region DELETE

        internal void DeletePlayQueueEntry(int rowId)
        {
            sqlConnection.Delete<PlayQueueEntryTable>(rowId);
        }

        internal void DeletePlaylistEntry(int rowId)
        {
            sqlConnection.Delete<PlaylistEntryTable>(rowId);
        }

        internal void DeletePlaylist(int playlistId)
        {
            sqlConnection.Query<PlaylistEntryTable>("DELETE FROM PlaylistEntryTable WHERE PlaylistId = ?", playlistId);

            sqlConnection.Delete<PlaylistTable>(playlistId);
        }

        internal void DeleteArtist(int artistId)
        {
            sqlConnection.Delete<ArtistTable>(artistId);
        }

        internal void DeleteSong(int songId)
        {
            sqlConnection.Delete<SongTable>(songId);
        }

        internal void DeleteAlbum(int albumId)
        {
            sqlConnection.Delete<AlbumTable>(albumId);
        }

        internal void DeleteHistoryItem(int rowId)
        {
            sqlConnection.Delete<HistoryTable>(rowId);
        }

        #endregion

        #region OTHER

        internal void ClearPlaybackQueue()
        {
            sqlConnection.DeleteAll<PlayQueueEntryTable>();
        }

        internal void Update(BaseTable entry)
        {
            sqlConnection.Update(entry);
        }

        #endregion
    }
}
