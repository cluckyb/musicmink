using MusicMinkAppLayer.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;

namespace MusicMinkAppLayer.Models
{
    public class SearchManagerModel
    {
        public static Task<List<int>> SearchSongs(string query)
        {
            return Task.Factory.StartNew(() =>
            {
                List<int> songIds = new List<int>();
                IEnumerable<SongTable> songResults = DatabaseManager.Current.SearchSongs(query);

                foreach (SongTable songResult in songResults)
                {
                    songIds.Add(songResult.SongId);
                }

                return songIds;
            });
        }

        public static Task<List<int>> SearchAlbums(string query)
        {
            return Task.Factory.StartNew(() =>
            {
                List<int> albumIds = new List<int>();
                IEnumerable<AlbumTable> albumResults = DatabaseManager.Current.SearchAlbums(query);

                foreach (AlbumTable albumResult in albumResults)
                {
                    albumIds.Add(albumResult.AlbumId);
                }

                return albumIds;
            });
        }

        public static Task<List<int>> SearchArtists(string query)
        {
            return Task.Factory.StartNew(() =>
            {
                List<int> artistIds = new List<int>();
                IEnumerable<ArtistTable> artistResults = DatabaseManager.Current.SearchArtists(query);

                foreach (ArtistTable artistResult in artistResults)
                {
                    artistIds.Add(artistResult.ArtistId);
                }

                return artistIds;
            });
        }
    }
}
