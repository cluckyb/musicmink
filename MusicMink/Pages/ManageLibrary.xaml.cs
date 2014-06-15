using MusicMink.MediaSources;

namespace MusicMink.Pages
{
    /// <summary>
    /// Manage Library
    /// </summary>
    public sealed partial class ManageLibrary : BasePage
    {
        public ManageLibrary()
        {
            this.InitializeComponent();

            this.DataContext = MediaImportManager.Current;
        }
    }
}
