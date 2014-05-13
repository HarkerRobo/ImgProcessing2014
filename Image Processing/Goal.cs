using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing2014 {
    public class Goal {
        public enum Sides : byte { Left, Right };

        public double Distance { get; private set; }
        public double Yaw { get; private set; }
        public Sides Side { get; private set; }
		public bool isHot { get; private set; }

		public Goal():this(-1,-1,Sides.Left,false) {
        }

		public Goal(double distance, double yaw, Sides side, bool ishot) {
            Distance = distance;
            Yaw = yaw;
            Side = side;
			isHot = ishot;
        }
    }
}
