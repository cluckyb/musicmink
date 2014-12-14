using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMink.ViewModels.DesignData
{
    public class ArtistDesignData
    {
        public string Name { get; set; }

        public string Picture { get; set; }

        public ArtistDesignData()
        {
            this.Name = "Antarctigo Vespucci";

            this.Picture = "ViewModels/DesignData/Art/AV.jpg";
        }
    }
}
