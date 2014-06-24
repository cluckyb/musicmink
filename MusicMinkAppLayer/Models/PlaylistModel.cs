using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Tables;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MusicMinkAppLayer.Models
{
    /// <summary>
    /// Model for a Playlist
    /// </summary>
    public class PlaylistModel : BaseModel<PlaylistTable>
    {
        private Dictionary<int, PlaylistEntryModel> IndexLookupMap = new Dictionary<int, PlaylistEntryModel>();
        private List<int> SongIds = new List<int>();
        public ObservableCollection<PlaylistEntryModel> Songs = new ObservableCollection<PlaylistEntryModel>();

        public static class Properties
        {
            public const string PlaylistId = "PlaylistId";

            public const string Name = "Name";
        }

        public PlaylistModel(PlaylistTable table)
            : base(table)
        {

        }

        #region properties

        public int PlaylistId
        {
            get
            {
                return GetTableField<int>(PlaylistTable.Properties.PlaylistId);
            }
        }

        public string Name
        {
            get
            {
                return GetTableField<string>(PlaylistTable.Properties.Name);
            }
            set
            {
                SetTableField<string>(PlaylistTable.Properties.Name, value, Properties.Name);
            }
        }

        #endregion

        #region Methods

        public bool ContainsSong(int songId)
        {
            return SongIds.Contains(songId);
        }

        public void Populate()
        {
            Songs.Clear();

            List<PlaylistEntryTable> allEntries = DatabaseManager.Current.FetchPlaylistEntriesForPlaylist(PlaylistId);

            PlaylistEntryModel head = null;

            foreach (PlaylistEntryTable playlistEntry in allEntries)
            {
                PlaylistEntryModel newEntry = new PlaylistEntryModel(playlistEntry);

                IndexLookupMap.Add(newEntry.RowId, newEntry);
                SongIds.Add(playlistEntry.SongId);

                if (newEntry.PrevId == 0)
                {
                    DebugHelper.Assert(new CallerInfo(), head == null, "Second head found for playlist {0}!", PlaylistId);

                    head = newEntry;
                }
            }

            PlaylistEntryModel currentLocation = head;

            while (currentLocation != null && Songs.Count < IndexLookupMap.Count)
            {
                Songs.Add(currentLocation);

                if (IndexLookupMap.ContainsKey(currentLocation.NextId))
                {
                    currentLocation = IndexLookupMap[currentLocation.NextId];
                }
                else
                {
                    currentLocation = null;
                }
            }

            DebugHelper.Assert(new CallerInfo(), currentLocation == null, "Circular reference found in Playlist {0}", PlaylistId);
            DebugHelper.Assert(new CallerInfo(), Songs.Count == IndexLookupMap.Count, "Missing element found in Playlist {0}", PlaylistId);
        }

        public void RemoveSong(int songId)
        {
            List<PlaylistEntryTable> entriesToRemove = DatabaseManager.Current.FetchPlaylistEntriesForPlaylistAndSongId(PlaylistId, songId);

            foreach (PlaylistEntryTable entry in entriesToRemove)
            {
                RemoveEntry(entry.RowId);
            }
        }

        public void RemoveEntry(int entryId)
        {
            PlaylistEntryModel songToRemove = null;

            if (!IndexLookupMap.TryGetValue(entryId, out songToRemove))
            {
                DebugHelper.Alert(new CallerInfo(), "Tried to remove play queue entry {0} but its not in our lookup", entryId);

                return;
            }

            PlaylistEntryModel previousModel = null;

            if (IndexLookupMap.TryGetValue(songToRemove.PrevId, out previousModel))
            {
                previousModel.NextId = songToRemove.NextId;
            }

            PlaylistEntryModel nextModel = null;

            if (IndexLookupMap.TryGetValue(songToRemove.NextId, out nextModel))
            {
                nextModel.PrevId = songToRemove.PrevId;
            }

            SongIds.Remove(songToRemove.SongId);
            Songs.Remove(songToRemove);
            IndexLookupMap.Remove(songToRemove.RowId);
            DatabaseManager.Current.DeletePlaylistEntry(songToRemove.RowId);
        }

        public void MoveSong(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;

            PlaylistEntryModel songToMove = Songs[oldIndex];
            PlaylistEntryModel target = null;

            if (newIndex > 0)
            {
                if (newIndex < oldIndex)
                {
                    target = Songs[newIndex - 1];
                }
                else
                {
                    target = Songs[newIndex];
                }
            }

            // Remove from old spot
            PlaylistEntryModel previousModel = null;

            if (IndexLookupMap.TryGetValue(songToMove.PrevId, out previousModel))
            {
                previousModel.NextId = songToMove.NextId;
            }

            PlaylistEntryModel nextModel = null;

            if (IndexLookupMap.TryGetValue(songToMove.NextId, out nextModel))
            {
                nextModel.PrevId = songToMove.PrevId;
            }

            // Insert after new spot
            if (target == null)
            {
                PlaylistEntryModel head = null;

                if (Songs.Count > 0)
                {
                    head = Songs.First();
                }

                if (head != null)
                {
                    songToMove.NextId = head.RowId;
                    head.PrevId = songToMove.RowId;
                    songToMove.PrevId = 0;
                }
                else
                {
                    // Should be redundant
                    songToMove.NextId = 0;
                    songToMove.PrevId = 0;
                }
            }
            else
            {
                PlaylistEntryModel newNextModel = null;

                if (IndexLookupMap.TryGetValue(target.NextId, out newNextModel))
                {
                    newNextModel.PrevId = songToMove.RowId;
                }

                songToMove.NextId = target.NextId;
                target.NextId = songToMove.RowId;
                songToMove.PrevId = target.RowId;
            }

            Songs.Move(oldIndex, newIndex);
        }

        public int AddSong(int songId)
        {
            PlaylistEntryModel currentTail = null;

            if (Songs.Count > 0)
            {
                currentTail = Songs.Last();
            }

            PlaylistEntryTable newPlaylistEntry;

            if (currentTail == null)
            {
                newPlaylistEntry = new PlaylistEntryTable(PlaylistId, songId, 0, 0);

                DatabaseManager.Current.AddPlaylistEntry(newPlaylistEntry);
            }
            else
            {
                newPlaylistEntry = new PlaylistEntryTable(PlaylistId, songId, 0, currentTail.RowId);

                DatabaseManager.Current.AddPlaylistEntry(newPlaylistEntry);

                currentTail.NextId = newPlaylistEntry.RowId;
            }

            PlaylistEntryModel newEntry = new PlaylistEntryModel(newPlaylistEntry);

            IndexLookupMap.Add(newEntry.RowId, newEntry);
            Songs.Add(newEntry);
            SongIds.Add(newEntry.SongId);

            return newEntry.RowId;
        }

        #endregion
    }
}
