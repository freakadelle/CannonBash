using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core
{
    class Crossjoint
    {

        public int index;
        public float3 pos;

        public Crossjoint(int _index, float3 _pos)
        {
            index = _index;
            pos = _pos;
        }
    }
}
