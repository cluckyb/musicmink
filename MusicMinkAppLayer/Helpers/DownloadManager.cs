using MusicMinkAppLayer.Diagnostics;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;

namespace MusicMinkAppLayer.Helpers
{
    /// <summary>
    /// Helper for downloading files from the web
    /// </summary>
    public class DownloadManager : INotifyPropertyChanged
    {
        public static class Properties
        {
            public const string PendingDownloads = "PendingDownloads";
        }

        private DownloadManager()
        {
        }

        private static DownloadManager _current;
        public static DownloadManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new DownloadManager();
                }

                return _current;
            }
        }

        private int _pendingDownloads = 0;
        public int PendingDownloads
        {
            get
            {
                return _pendingDownloads;
            }
            set
            {
                if (_pendingDownloads != value)
                {
                    _pendingDownloads = value;
                    NotifyPropertyChanged(Properties.PendingDownloads);
                }
            }
        }


        public async Task<bool> DownloadFile(Uri source, string target)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(source);

            WebResponse response = await request.GetResponseAsync();

            try
            {
                using (Stream downloadStream = response.GetResponseStream())
                {
                    using (Stream fileStream = await folder.OpenStreamForWriteAsync(target, CreationCollisionOption.ReplaceExisting))
                    {
                        int chunkSize = 4096;
                        byte[] bytes = new byte[chunkSize];
                        int byteCount;

                        while ((byteCount = downloadStream.Read(bytes, 0, chunkSize)) > 0)
                        {
                            fileStream.Write(bytes, 0, byteCount);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Error, "Exception in File Download {0}", e.Message);

                return false;
            }

            return true;
        }

        public async Task<bool> FileExists(StorageFolder folder, string target)
        {
            var f = await folder.GetFileAsync(target);

            return f == null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
