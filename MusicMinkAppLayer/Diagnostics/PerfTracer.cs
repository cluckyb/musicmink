using System;

namespace MusicMinkAppLayer.Diagnostics
{
    /// <summary>
    /// Simple helper to track how long various calls take. Each call to Trace() prints how long since previous call
    /// </summary>
    public class PerfTracer
    {
        private DateTime FirstTick = DateTime.MinValue;
        private DateTime LastTick = DateTime.MinValue;
        private string Id;

        public PerfTracer(string id)
        {
            Id = id;
            LastTick = DateTime.Now;
            FirstTick = LastTick;
        }

        public void Trace(string message)
        {
            DateTime now = DateTime.Now;

            Logger.Current.Log(new CallerInfo(), LogLevel.Perf, "Trace: {0} Lap Time: {1} Total Time: {2} Info: {3}", Id, (now - LastTick).TotalSeconds.ToString("F3"), (now - FirstTick).TotalSeconds.ToString("F3"), message);

            LastTick = now;
        }

        public void Restart()
        {
            LastTick = DateTime.Now;
            FirstTick = LastTick;
        }
    }
}
