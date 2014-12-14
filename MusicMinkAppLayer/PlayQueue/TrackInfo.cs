using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Tables;
using SQLite;

namespace MusicMinkAppLayer.PlayQueue
{
    /// <summary>
    /// Stripped down Play Queue entry info for consumption by the MediaPlayer
    /// </summary>
    public class TrackInfo
    {
        public string Title { get; private set; }

        public string Album { get; private set; }

        public string Artist { get; private set; }

        public string AlbumArtist { get; private set; }

        public string Path { get; private set; }
        public string ArtPath { get; private set; }

        public int SongId { get; private set; }

        public int RowId { get; private set; }
        public int NextId { get; private set; }
        public int PrevId { get; private set; }

        public TrackInfo(string title, string artist, string album, string albumArtist, string path, string artPath, int rowId, int nextId, int prevId, int songId)
        {
            ArtPath = artPath;
            Title = title;
            Artist = artist;
            Album = album;
            AlbumArtist = albumArtist;
            Path = path;
            RowId = rowId;
            NextId = nextId;
            PrevId = prevId;
            SongId = songId;
        }

        public static TrackInfo TrackInfoFromRowId(int rowId)
        {
            try
            {
                PlayQueueEntryTable playQueueEntry = DatabaseManager.Current.LookupPlayQueueEntryById(rowId);

                if (playQueueEntry != null)
                {
                    SongTable songTable = DatabaseManager.Current.LookupSongById(playQueueEntry.SongId);

                    if (songTable != null)
                    {
                        AlbumTable albumTable = DatabaseManager.Current.LookupAlbumById(songTable.AlbumId);
                        ArtistTable artistTable = DatabaseManager.Current.LookupArtistById(songTable.ArtistId);

                        if (albumTable != null && artistTable != null)
                        {
                            ArtistTable albumArtistTable = DatabaseManager.Current.LookupArtistById(albumTable.ArtistId);

                            if (albumArtistTable != null)
                            {
                                return new TrackInfo(songTable.Name, artistTable.Name, albumTable.Name, albumArtistTable.Name, songTable.Source, albumTable.AlbumArt, rowId, playQueueEntry.NextId, playQueueEntry.PrevId, playQueueEntry.SongId);
                            }
                            else
                            {
                                Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Couldn't play row {0}, no artistEntry {1} matches!", rowId, albumTable.ArtistId);
                            }
                        }
                        else
                        {
                            Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Couldn't play row {0}, no albumEntry {1} or artistEntry {2} matches ({3} {4})!", rowId, songTable.AlbumId, songTable.ArtistId, albumTable != null, artistTable != null);
                        }
                    }
                    else
                    {
                        Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Couldn't play row {0}, no songEntry for {1} matches!", rowId, playQueueEntry.SongId);
                    }
                }
                else
                {
                    Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Couldn't play row {0}, no playQueueEntry matches!", rowId);
                }
            }
            catch (SQLiteException ex)
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Error, "Couldn't play row {0}, got an exception {1}!", rowId, ex.Message);
            }

            return null;
        }
    }
}
