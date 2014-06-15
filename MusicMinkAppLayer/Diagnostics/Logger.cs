using MusicMinkAppLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Threading;

namespace MusicMinkAppLayer.Diagnostics
{
    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Perf
    }

    public enum LogType
    {
        FG,
        BG,
        Unknown
    }

    /// <summary>
    /// Writes log messages to a text file. A different Logger is used for the Foreground and Background processes
    /// to avoid race conditions. Must call Init() before use.
    /// When log file is full it is copped to a spool file and reset (old spool file is thus removed)
    /// </summary>
    public class Logger
    {
        public static string LOG_FOLDER = "Logs";

        private static string BASE_FILENAME = "Logs.txt";
        private static string BASE_SPOOLED_FILENAME = "Logs2.txt";
        private static long MAX_FILE_LENGTH = 524288;

        private static long MAX_LOG_BUFFER = 50;

        private string FILENAME = string.Empty;
        private string SPOOLED_FILENAME = string.Empty;

        private List<string> buffer = new List<string>();

        private LogType MyType = LogType.Unknown;

        private object listLock = new Object();
        private object editLock = new Object();

        private bool isFlushInProgress = false;
        private bool reFlush = false;

        private bool isEnabled;

        private bool isInit = false;

        private static Logger _current;
        public static Logger Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new Logger();
                }

                return _current;
            }
        }

        public void Init(LogType type)
        {
            MyType = type;
            isInit = true;

            FILENAME = LOG_FOLDER + "\\" + MyType.ToString() + "_" + BASE_FILENAME;
            SPOOLED_FILENAME = LOG_FOLDER + "\\" + MyType.ToString() + "_" + BASE_SPOOLED_FILENAME;
        }

        private Logger()
        {
            UpdateEnabled();
        }

        private async void FlushLogs(ThreadPoolTimer timer)
        {
            await Flush();
        }

        public async Task Flush()
        {
            if (!isEnabled) return;

            if (!isInit) return;

            bool earlyExit = false;

            lock (editLock)
            {
                earlyExit = isFlushInProgress;

                reFlush = isFlushInProgress;

                isFlushInProgress = true;
            }

            if (earlyExit)
            {
                return;
            }

            List<string> copy = new List<string>();
            lock (listLock)
            {
                copy = new List<string>(buffer);

                buffer.Clear();
            }

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            bool spoolFile = false;

            if (copy.Count > 0)
            {
                try
                {
                    using (var fileStream = await localFolder.OpenStreamForWriteAsync(FILENAME, CreationCollisionOption.OpenIfExists))
                    {
                        foreach (string logMessage in copy)
                        {
                            byte[] bytes = Encoding.UTF8.GetBytes(logMessage.ToString());

                            fileStream.Write(bytes, 0, bytes.Length);
                            if (fileStream.Length > MAX_FILE_LENGTH)
                            {
                                spoolFile = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debugger.Break();
                }
            }

            if (spoolFile)
            {
                IStorageFile currentLogFile = await localFolder.GetFileAsync(FILENAME);
                if (currentLogFile != null)
                {
                    await currentLogFile.RenameAsync(SPOOLED_FILENAME, NameCollisionOption.ReplaceExisting);
                }
            }

            bool runAgain = false;
            lock (editLock)
            {
                isFlushInProgress = false;

                runAgain = reFlush;
                reFlush = false;
            }

            if (runAgain)
            {
                await Flush();
            }
        }

        public void Log(CallerInfo info, LogLevel level, string message, params object[] args)
        {
            if (!isEnabled) return;

            StringBuilder builder = new StringBuilder();

            builder.Append(DateTime.Now.ToString()).Append(" - ").Append(info.FileName).Append(":").Append(info.FunctionName);
            builder.Append("(line ").Append(info.LineNumber).Append(") - ").AppendLine(string.Format(message, args));

            var async = ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) =>
            {
                WriteLog(builder.ToString());   
            }));
        }

        private async void WriteLog(string logMessage)
        {
            Debug.WriteLine(logMessage);

            bool needFlush = false;

            lock (listLock)
            {
                buffer.Add(logMessage);

                needFlush = buffer.Count > MAX_LOG_BUFFER;
            }

            if (needFlush)
            {
                await Flush();
            }
        }

        public void UpdateEnabled()
        {
            isEnabled = ApplicationSettings.GetSettingsValue<bool>(ApplicationSettings.SETTING_IS_LOGGING_ENABLED, true);
        }
    }
}
