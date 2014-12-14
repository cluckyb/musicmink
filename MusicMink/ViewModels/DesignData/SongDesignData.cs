using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMink.ViewModels.DesignData
{
    public class SongDesignData
    {
        public string Name { get; set; }

        public ArtistDesignData Artist { get; set; }

        public AlbumDesignData Album { get; set; }

        public int Rating { get; set; }

        public SongDesignData()
        {
            this.Name = "Bang!";

            this.Artist = new ArtistDesignData();

            this.Album = new AlbumDesignData();

            this.Rating = 8;
        }
    }
}
