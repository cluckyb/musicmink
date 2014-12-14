using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMink.ViewModels.DesignData
{
    public class PlayQueueDesignData
    {
        public SongDesignData CurrentTrack { get; set; }

        public SongDesignData PrevTrack { get; set; }

        public SongDesignData NextTrack { get; set; }

        public double PercentTime { get; set; }

        public bool IsActive { get; set; }

        public string TimeLeft { get; set; }

        public bool IsPlaying { get; set; }

        public string ProgressTime { get; set; }

        public int TracksLeft { get; set; }

        public PlayQueueDesignData()
        {
            CurrentTrack = new SongDesignData();
            PrevTrack = new SongDesignData();
            NextTrack = new SongDesignData();

            PercentTime = 0.33;

            IsActive = true;

            TimeLeft = "-2:14";

            ProgressTime = "2:14/3:14";

            IsPlaying = false;

            TracksLeft = 5;
        }
    }
}
