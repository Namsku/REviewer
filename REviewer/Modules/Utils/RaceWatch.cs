using System.Diagnostics;
using MessagePack;

namespace REviewer.Modules.Utils
{
    [MessagePackObject]
    public class RaceWatch(nint startTime)
    {
        [Key(0)]
        private nint _offset = startTime;

        public void StartFrom(nint startTime)
        {
            _offset = startTime;
        }

        internal void Reset()
        {
            _offset = 0;
        }

        internal nint GetOffset()
        {
            return _offset;
        }
    }
}