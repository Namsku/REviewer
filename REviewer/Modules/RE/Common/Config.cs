using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REviewer.Modules.RE.Common
{
    public class Config
    {
        public readonly int _position;
        public readonly int _size;
        public readonly int scaling; // New field for scaling

        public Config(int pos, int size, int scaling)
        {
            _position = pos;
            _size = size;
            this.scaling = scaling; // Initialize scaling
        }
    }
}
