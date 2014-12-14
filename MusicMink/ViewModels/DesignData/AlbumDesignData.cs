using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMink.ViewModels.DesignData
{
    public class AlbumDesignData
    {
        public string Name { get; set; }

        public string AlbumArt { get; set; }

        public AlbumDesignData()
        {
            this.Name = "Soulmate Stuff";

            this.AlbumArt = "ViewModels/DesignData/Art/SS.jpg";
        }
    }
}
