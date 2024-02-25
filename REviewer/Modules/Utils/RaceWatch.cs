using System.Diagnostics;
using MessagePack;

namespace REviewer.Modules.Utils
{
    [MessagePackObject]
    public class RaceWatch
    {
        [IgnoreMember]
    
        private Stopwatch _stopwatch = new();
        
        [Key(0)]
        private TimeSpan _offset = TimeSpan.Zero;

        public RaceWatch()
        {
            _offset = TimeSpan.Zero;
        }

        public RaceWatch(TimeSpan startTime)
        {
            _offset = startTime;
        }

        public void StartFrom(TimeSpan startTime)
        {
            _offset = startTime;
            _stopwatch.Restart();
        }

        internal void Reset()
        {
            _stopwatch.Reset();
            _offset = TimeSpan.Zero;
        }

        internal void Stop()
        {
            _stopwatch.Stop();
        }

        internal void Start()
        {
            _stopwatch.Start();
        }

        internal void Restart()
        {
            _stopwatch.Restart();
        }

        internal TimeSpan GetOffset()
        {
            return _offset;
        }

        [IgnoreMember]
        public TimeSpan Elapsed => _stopwatch.Elapsed + _offset;

        [IgnoreMember]
        public bool IsRunning => _stopwatch.IsRunning;
    }
}