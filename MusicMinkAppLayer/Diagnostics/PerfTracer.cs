using System;

namespace MusicMinkAppLayer.Diagnostics
{
    /// <summary>
    /// Simple helper to track how long various calls take. Each call to Trace() prints how long since previous call
    /// </summary>
    public class PerfTracer
    {
        private DateTime LastTick = DateTime.MinValue;
        private string Id;

        public PerfTracer(string id)
        {
            Id = id;
            LastTick = DateTime.Now;
        }

        public void Trace(string message)
        {
            DateTime now = DateTime.Now;

            Logger.Current.Log(new CallerInfo(), LogLevel.Perf, "Trace: {0} Lap Time: {1} Info: {2}", Id, (now - LastTick).TotalSeconds.ToString("F3"), message);

            LastTick = now;
        }

        public void Restart()
        {
            LastTick = DateTime.Now;
        }
    }
}
