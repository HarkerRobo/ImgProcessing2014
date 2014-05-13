using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing2014
{
        [Flags]
        internal enum PixelState : byte
        {
            None = 0,
            Red = 2,
            Blue = 4,
            Seed = 8,
            Candidate = 16,
            Visited = 32,
            NullVisited = 64
        }
}
