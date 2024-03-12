using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REviewer.Modules.RE.Common
{
    public partial class RootObject : INotifyPropertyChanged
    {
        public int SegmentCount;
        public int PreviousTimerValue;
        public List<int> IGTSegments;
        private List<string> _igtsHumanFormat;
        public List<string> IGTSHumanFormat
        {
            get { return _igtsHumanFormat; }
            set
            {
                if (_igtsHumanFormat != value)
                {
                    _igtsHumanFormat = value;
                    OnPropertyChanged(nameof(IGTSHumanFormat));
                }
            }
        }
        public void InitTimers()
        {
            SegmentCount = 0;
            PreviousTimerValue = 0;
            IGTSegments = [0, 0, 0, 0];
            IGTSHumanFormat = ["00:00:00.00", "00:00:00.00", "00:00:00.00", "00:00:00.00"];

            OnPropertyChanged(nameof(IGTSHumanFormat));
        }

        private void UpdateChronometers()
        {
            SegmentCount += 1 % 4;
            IGTSegments[SegmentCount - 1] = GameTimer.Value;

            OnPropertyChanged(nameof(IGTSHumanFormat));
        }
    }
}
