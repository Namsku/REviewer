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
        public readonly bool _health;
        public readonly bool _currentWeapon;

        public Config(int pos, int size, bool health, bool weapon)
        {
            _position = pos;
            _size = size;
            _health = health;
            _currentWeapon = weapon;
        }
    }
}
