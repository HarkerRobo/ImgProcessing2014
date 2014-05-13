using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImageProcessing2014;

using java.util;
using edu.wpi.first.wpilibj.networktables;
using System.Threading;

namespace ImageProcessing2014 {
    public class Networking {
        static NetworkTable table;

        public static void Init() {
            NetworkTable.setClientMode();
            NetworkTable.setTeam(1072);
            NetworkTable.initialize();

            table = NetworkTable.getTable("imageProcessing");
        }

        public static void SendData(double distance, double angle, bool hotness) {
            table.putNumber("distance", distance);
            table.putNumber("angle", angle);
            table.putBoolean("hotness", hotness);
        }
    }
}
