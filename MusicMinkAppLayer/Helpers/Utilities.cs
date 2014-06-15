using System;

namespace MusicMinkAppLayer.Helpers
{
    /// <summary>
    /// Useful static utility functions
    /// </summary>
    public static class Utilities
    {
        public static bool IsSupportedFileType(string fileType)
        {
            return (fileType.ToLowerInvariant() == ".mp3" || fileType.ToLowerInvariant() == ".m4a");
        }
    }
}
