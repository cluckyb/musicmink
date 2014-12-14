using MusicMink.Collections;
using MusicMink.Common;
using MusicMinkAppLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Titles based on count
// Show search term

namespace MusicMink.ViewModels
{
    public class SearchManagerViewModel : NotifyPropertyChangedUI
    {
        private static SearchManagerViewModel _current;
        public static SearchManagerViewModel Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new SearchManagerViewModel();
                }

                return _current;
            }
        }

        #region Collections

        private SortedList<SongViewModel> _songs = new SortedList<SongViewModel>(new SongSortGenericOrder(SongViewModel.Properties.Name, true));
        public SortedList<SongViewModel> Songs
        {
            get
            {
                return _songs;
            }
        }

        private SortedList<AlbumViewModel> _albums = new SortedList<AlbumViewModel>(new GenericComparer<AlbumViewModel>());
        public SortedList<AlbumViewModel> Albums
        {
            get
            {
                return _albums;
            }
        }

        private SortedList<ArtistViewModel> _artists = new SortedList<ArtistViewModel>(new GenericComparer<ArtistViewModel>());
        public SortedList<ArtistViewModel> Artists
        {
            get
            {
                return _artists;
            }
        }

        #endregion

        #region Properties

        public static class Properties
        {
            public const string IsValidSearch = "IsValidSearch";
            public const string LastSearchTerm = "LastSearchTerm";
            public const string SearchInProgress = "SearchInProgress";
            public const string ContentInfoArtists = "ContentInfoArtists";
            public const string ContentInfoAlbums = "ContentInfoAlbums";
            public const string ContentInfoSongs = "ContentInfoSongs";
        }

        private string _lastSearchTerm;
        public string LastSearchTerm
        {
            get
            {
                return _lastSearchTerm;
            }
            protected set
            {
                if (_lastSearchTerm != value)
                {
                    _lastSearchTerm = value;
                    NotifyPropertyChanged(Properties.LastSearchTerm);
                    NotifyPropertyChanged(Properties.IsValidSearch);
                }
            }
        }

        public bool IsValidSearch
        {
            get
            {
                return !string.IsNullOrEmpty(LastSearchTerm);
            }
        }

        public bool SearchInProgress
        {
            get
            {
                return artistSearchInProgress || albumSearchInProgress || songSearchInProgress;
            }
        }

        public string ContentInfoArtists
        {
            get
            {
                return Strings.HandlePlural(Artists.Count, Strings.GetResource("FormatArtistsPlural"), Strings.GetResource("FormatArtistsSingular"));
            }
        }

        public string ContentInfoAlbums
        {
            get
            {
                return Strings.HandlePlural(Albums.Count, Strings.GetResource("FormatAlbumsPlural"), Strings.GetResource("FormatAlbumsSingular"));
            }
        }

        public string ContentInfoSongs
        {
            get
            {
                return Strings.HandlePlural(Songs.Count, Strings.GetResource("FormatSongsPlural"), Strings.GetResource("FormatSongsSingular"));
            }
        }

        #endregion

        bool artistSearchInProgress = false;
        bool albumSearchInProgress = false;
        bool songSearchInProgress = false;

        public void Search(string query)
        {
            if (SearchInProgress) return;

            if (query == LastSearchTerm) return;

            LastSearchTerm = query;

            if (string.IsNullOrEmpty(query)) return;

            artistSearchInProgress = true;
            albumSearchInProgress = true;
            songSearchInProgress = true;
            NotifyPropertyChanged(Properties.SearchInProgress);

            SearchSongs(query);
            SearchAlbums(query);
            SearchArtists(query);
        }

        private async void SearchSongs(string query)
        {
            Songs.Clear();

            List<int> foundSongs = await SearchManagerModel.SearchSongs(query);

            foreach (int songId in foundSongs)
            {
                SongViewModel foundSong = LibraryViewModel.Current.LookupSongById(songId);
                if (foundSong != null)
                {
                    Songs.Add(foundSong);
                }
            }

            songSearchInProgress = false;
            NotifyPropertyChanged(Properties.SearchInProgress);
            NotifyPropertyChanged(Properties.ContentInfoSongs);
        }

        private async void SearchAlbums(string query)
        {
            Albums.Clear();

            List<int> foundAlbums = await SearchManagerModel.SearchAlbums(query);

            foreach (int albumId in foundAlbums)
            {
                AlbumViewModel foundAlbum = LibraryViewModel.Current.LookupAlbumById(albumId);
                if (foundAlbum != null)
                {
                    Albums.Add(foundAlbum);
                }
            }

            albumSearchInProgress = false;
            NotifyPropertyChanged(Properties.SearchInProgress);
            NotifyPropertyChanged(Properties.ContentInfoAlbums);
        }

        private async void SearchArtists(string query)
        {
            Artists.Clear();

            List<int> foundArtists = await SearchManagerModel.SearchArtists(query);

            foreach (int artistId in foundArtists)
            {
                ArtistViewModel foundArtist = LibraryViewModel.Current.LookupArtistById(artistId);
                if (foundArtist != null)
                {
                    Artists.Add(foundArtist);
                }
            }

            artistSearchInProgress = false;
            NotifyPropertyChanged(Properties.SearchInProgress);
            NotifyPropertyChanged(Properties.ContentInfoArtists);
        }
    }
}
