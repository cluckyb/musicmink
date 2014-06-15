using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMink.Design
{
    public class SongDesignStub
    {
        private string _name = "Song Name";
        public string Name
        {
            get
            {
                return _name;
            }
        }

        private string _durationText = "2:20";
        public string DurationText
        {
            get
            {
                return _durationText;
            }
        }

        private string _artistName = "Band Name";
        public string ArtistName
        {
            get
            {
                return _artistName;

            }
        }

        public int Rating
        {
            get
            {
                return 6;
            }
        }

        public int TrackNumber
        {
            get
            {
                return 6;
            }
        }


        public string ExtraInfoString
        {
            get
            {
                return "0 plays";
            }
        }
    }
}
