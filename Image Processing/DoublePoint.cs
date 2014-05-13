using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing2014 {
    class DoublePoint {
        public double X, Y;
		/// <summary>
		/// Instead of an Integer Point, this constructor creates a point with double coordinates
		/// </summary>
		/// <param name="x">The x coordinate in double format</param>
		/// <param name="y">The y coordinate in double format</param>
        public DoublePoint(double x, double y) {
            X = x;
            Y = y;
        }
    }
}
