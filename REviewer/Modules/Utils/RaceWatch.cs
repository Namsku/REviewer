
namespace REviewer.Modules.Utils
{
    public class RaceWatch(nint startTime)
    {
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