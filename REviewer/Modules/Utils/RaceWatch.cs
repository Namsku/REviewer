
using System.ComponentModel;

namespace REviewer.Modules.Utils
{
    public class RaceWatch
    {
        private int _offset;

        public RaceWatch(int startTime)
        {
            _offset = startTime;
        }

        public void StartFrom(int startTime)
        {
            _offset = startTime;
        }

        public void Reset()
        {
            _offset = 0;
        }

        public int GetOffset()
        {
            return _offset;
        }
    }
}