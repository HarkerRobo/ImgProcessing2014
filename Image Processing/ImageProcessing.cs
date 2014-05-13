using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using System.Web;
using System.IO;
using System.Timers;
using System.Net;

using MjpegProcessor;

namespace ImageProcessing2014 {
    public unsafe class ImageProcessing {

        //private static Bitmap image;
        //private static BitmapData imageData;
        //private static int imageWidth;
        //private static int imageHeight;
        //private static int stride;

        //private static Stopwatch stopwatch;

        public static void Init() {
            Util.Filter.InitializeLookUpTable();
            //stopwatch = new Stopwatch();
        }

        public static Goal ProcessImage(Bitmap image) {
            //stopwatch.Restart();

            //long t1 = stopwatch.ElapsedMilliseconds;

            Console.WriteLine("unlocking bits");

            BitmapData imageData = image.LockBits(
                new Rectangle(0, 0, VisionConstants.Camera.IMAGE_WIDTH, VisionConstants.Camera.IMAGE_HEIGHT),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppRgb
            );

            //long t2 = stopwatch.ElapsedMilliseconds;

            byte* pScan0 = (byte*)imageData.Scan0.ToPointer();

            Console.WriteLine("finding goal");
            Goal goal = FindGoal(pScan0, VisionConstants.Camera.IMAGE_STRIDE, VisionConstants.Camera.IMAGE_WIDTH, VisionConstants.Camera.IMAGE_HEIGHT);
            //Console.Write ("Goal is ");
            //Console.WriteLine((goal.isHot) ? "not " : ("") + "hot");
            Console.WriteLine("Side: " + goal.Side + " | " + "Is Hot: " + goal.isHot + " | " + "Distance: " + goal.Distance + " | " + "Yaw: " + goal.Yaw);

            //long t3 = stopwatch.ElapsedMilliseconds;

            // FindBalls(pScan0, VisionConstants.Camera.IMAGE_STRIDE, VisionConstants.Camera.IMAGE_WIDTH, VisionConstants.Camera.IMAGE_HEIGHT);

            //long t4 = stopwatch.ElapsedMilliseconds;


            //Pen penViolet = new Pen(Brushes.DarkViolet);
            //Pen penFuchsia = new Pen(Brushes.Fuchsia);'

            Console.WriteLine("unlocking bits");
            image.UnlockBits(imageData);

            //long t5 = stopwatch.ElapsedMilliseconds;


            //Console.WriteLine(t1 + "get " + (t2-t1) + "lock " + (t3-t2) + "goals " + (t4-t3) + "balls " + (t5-t4) "unlock");

            //return goal;
            return null;
        }

        public static Goal FindGoal(byte* pScan0, int stride, int imageWidth, int imageHeight) {
            List<Blob> blobs;

            PixelTester.PixelTest test = PixelTester.TestGoalPixel;
            PixelFiller.PixelFill fill = PixelFiller.FillPixel;

            Byte[] states = new byte[imageWidth * imageHeight];
            fixed(byte* pStates = states) {
                //fill states
                Util.Filter.FilterGoal(pScan0, pStates, stride, imageWidth, imageHeight);

                //find blobs
                blobs = Util.Processing.FindBlobs(pStates, fill, test);

                Util.Processing.FilterVTBlobs(blobs); // order corners happens here...
            }

            //List<List<DoublePoint>> verticalVTs   = new List<List<DoublePoint>>();
            List<DoublePoint> largestVerticalVT = new List<DoublePoint>();
            int pointCount = -1;
            // VT means Vision Targets
            List<List<DoublePoint>> horizontalVTs = new List<List<DoublePoint>>();


            foreach(Blob blob in blobs) {
                foreach(Point p in blob.ConvexHull) {
                    //fills the corners of the convex hull (from pX - 1 to pX + 1, pY - 1 to pY + 1)
                    Util.Drawing.FillRectangle(pScan0, stride, imageWidth, imageHeight, p.X - 1, p.Y - 1, 3, 3, Util.Drawing.RED);
                }

                List<DoublePoint> unfisheyedCorners = new List<DoublePoint>();

                List<Point> corners = blob.Corners;
                Util.Drawing.FillRectangle(pScan0, stride, imageWidth, imageHeight, corners[0].X - 1, corners[0].Y - 1, 3, 3, Util.Drawing.GREEN);
                unfisheyedCorners.Add(Util.Processing.CorrectFishEye(corners[0]));

                Util.Drawing.FillRectangle(pScan0, stride, imageWidth, imageHeight, corners[1].X - 1, corners[1].Y - 1, 3, 3, Util.Drawing.BLUE);
                unfisheyedCorners.Add(Util.Processing.CorrectFishEye(corners[1]));

                Util.Drawing.FillRectangle(pScan0, stride, imageWidth, imageHeight, corners[2].X - 1, corners[2].Y - 1, 3, 3, Util.Drawing.YELLOW);
                unfisheyedCorners.Add(Util.Processing.CorrectFishEye(corners[2]));

                Util.Drawing.FillRectangle(pScan0, stride, imageWidth, imageHeight, corners[3].X - 1, corners[3].Y - 1, 3, 3, Util.Drawing.CYAN);
                unfisheyedCorners.Add(Util.Processing.CorrectFishEye(corners[3]));



                /*foreach(Point corner in blob.Corners) {
                    Util.Drawing.FillRectangle(pScan0, stride, imageWidth, imageHeight, corner.X - 1, corner.Y - 1, 3, 3, Util.Drawing.GREEN);
                    unfisheyedCorners.Add(Util.Processing.CorrectFishEye(corner));
                }*/
                //double dist = Util.Processing.GetDistance(unfisheyedCorners[0], unfisheyedCorners[1], unfisheyedCorners[2], unfisheyedCorners[3], VisionConstants.Field.VisionTargetOrientation.Horizontal);
                //double angle = Util.Processing.GetAngle(unfisheyedCorners[0], unfisheyedCorners[1], unfisheyedCorners[2], unfisheyedCorners[3]);

                double x = (unfisheyedCorners[1].X + unfisheyedCorners[3].X - unfisheyedCorners[0].X - unfisheyedCorners[2].X) / 2;
                double y = (unfisheyedCorners[2].Y + unfisheyedCorners[3].Y - unfisheyedCorners[0].Y - unfisheyedCorners[1].Y) / 2;

                if(x > y)
                    horizontalVTs.Add(unfisheyedCorners);
                else if(blob.CalculatePointCount() > pointCount) { //TODO: Make calculatepointcount a property
                    pointCount = blob.CalculatePointCount();
                    largestVerticalVT = unfisheyedCorners;
                }


            }

            if(pointCount == -1) //haven't found any vertical goals
                return new Goal();

            bool isHot = false;
			double hotnessThreshold = 1.5*Util.Math.GetDistance(largestVerticalVT[0], largestVerticalVT[1]);
            Goal.Sides herro = Goal.Sides.Left;

            foreach(List<DoublePoint> corners in horizontalVTs) {
				Console.WriteLine("VT DIST: " + Util.Math.GetDistance(corners[2], largestVerticalVT[1]));
				Console.WriteLine("hotThresh" + hotnessThreshold);
                if(Util.Math.GetDistance(corners[2], largestVerticalVT[1]) < hotnessThreshold) {
                    isHot = true;
                    herro = Goal.Sides.Right;
                    break;
                } else if(Util.Math.GetDistance(corners[3], largestVerticalVT[0]) < hotnessThreshold) {
                    isHot = true;
                    herro = Goal.Sides.Left;
                    break;
                }
            }

            double dist = Util.Processing.GetDistance(largestVerticalVT[0], largestVerticalVT[1], largestVerticalVT[2], largestVerticalVT[3], VisionConstants.Field.VisionTargetOrientation.Vertical);
            double angle = Util.Processing.GetAngle(largestVerticalVT[0], largestVerticalVT[1], largestVerticalVT[2], largestVerticalVT[3]);
            //something to do with cosines. maybe sines
            double xGoal = herro == Goal.Sides.Left ? dist*Math.Sin(angle) + VisionConstants.Camera.VT_GOAL_DELTA_X : dist*Math.Sin(angle) - VisionConstants.Camera.VT_GOAL_DELTA_X;
            double yGoal = dist*Math.Cos(angle);
            return new Goal(Math.Sqrt(xGoal * xGoal + yGoal * yGoal), Math.Atan2(xGoal, yGoal), herro, isHot);
        }

        public static void FindBalls(byte* pScan0, int stride, int imageWidth, int imageHeight) {

            List<Blob> rBlobs, bBlobs;
            var states = new byte[imageWidth * imageHeight];
            fixed(byte* pStates = states) {
                //fill states
                Util.Filter.FilterBall(pScan0, stride, imageWidth, imageHeight, pStates);

                //find blobs of red and blue
                PixelFiller.PixelFill fill = PixelFiller.FillColoredPixel;

                PixelTester.PixelTest test = PixelTester.TestRedPixel;
                rBlobs = Util.Processing.FindBlobs(pStates, fill, test);

                test = PixelTester.TestBluePixel;
                bBlobs = Util.Processing.FindBlobs(pStates, fill, test);
#if DEBUG
                Console.WriteLine("============DEBUG============");
                Console.WriteLine("rBlobs contains " + rBlobs.Count);
                Console.WriteLine("bBlobs contains " + bBlobs.Count);
                Console.WriteLine("============================");
#endif
            }

            //draw bounding boxes
            //            var a = false;
            //            foreach (var blob in wBlobs) {
            //                a = !a;
            //                foreach (Point point in blob)
            //                    *(uint*)(pScan0 + point.Y*stride + point.X*4) = a ? 0xFFFF7777 : 0xFF77FF77;
            //            }

            // foreach(var blob in rBlobs) {
            //   var minX = imageWidth;
            //                var maxX = 0;
            //                int minY = imageHeight;
            //                int maxY = 0;
            //				foreach (LineSegment x in blob.GetLineSegments()) 
            //				{
            //					//                    int y = point.Y;
            //					if (x.leftX < minX)
            //						minX = x.leftX;
            //					if (x.rightX > maxX)
            //						maxX = x.rightX;
            //				}
            //				Console.WriteLine ("minX " + minX + " maxX " + maxX);
            //				var width = maxX - minX;
            //				var theta = ((double)width / imageWidth) * Util.Drawing.H_FOV;
            //				var dist = BALL_WIDTH / (2 * Math.Tan(theta / 2));
            //				var yposp = (maxX + minX) / 2 - imageWidth / 2;
            //				var ypos = BALL_WIDTH * width / yposp;
            //				var xpos = Math.Sqrt(dist * dist - (ypos * ypos + Util.Drawing.GREEN_CAMERA_HEIGHT * Util.Drawing.GREEN_CAMERA_HEIGHT));
            //				var zpos = 0.0;
            //				//Console.WriteLine(dist);
            //				//Console.WriteLine(ypos);
            //				Console.WriteLine(xpos + " " + ypos + " " + zpos);
            //			}
            //			foreach (var blob in bBlobs)
            //			{
            //				var minX = imageWidth;
            //				var maxX = 0;
            //				//                int minY = imageHeight;
            //				//                int maxY = 0;
            //				foreach (LineSegment x in blob.GetLineSegments()) {
            //					//                    int y = point.Y;
            //					if (x.leftX < minX)
            //						minX = x.leftX;
            //					if (x.leftX > maxX)
            //						maxX = x.leftX;
            //				}
            //				var width = maxX - minX;
            //				var theta = ((double)width / imageWidth) * Util.Drawing.H_FOV;
            //				var dist = BALL_WIDTH / (2 * Math.Tan(theta / 2));
            //				var yposp = (maxX + minX) / 2 - imageWidth / 2;
            //				var ypos = BALL_WIDTH * width / yposp;
            //				var xpos = Math.Sqrt(dist * dist - (ypos * ypos + Util.Drawing.GREEN_CAMERA_HEIGHT * Util.Drawing.GREEN_CAMERA_HEIGHT));
            //				var zpos = 0.0;
            //				//Console.WriteLine(dist);
            //				//Console.WriteLine(ypos);
            //				Console.WriteLine(xpos + " " + ypos + " " + zpos);
            //}
        }

        private static Bitmap getImage(string path) {
            //pictureBox.Load(path);
            //return (Bitmap)pictureBox.Image;

            //try {
            //    using(WebClient Client = new WebClient()) {
            //        Client.DownloadFile(path, "image.jpg");
            //    }
            //} catch(WebException e) {
            //    Console.WriteLine(e.Status + " | " + e.Message + " | " + e.InnerException);
            //}

            //var bytes = File.ReadAllBytes("image.jpg");
            //var ms = new MemoryStream(bytes);
            //var img = (Bitmap)Image.FromStream(ms);

            ////var img = (Bitmap)Image.FromFile("image.jpg");
            //return img;



            WebRequest requestPic = WebRequest.Create(path);
            WebResponse responsePic = requestPic.GetResponse();

            return (Bitmap)Image.FromStream(responsePic.GetResponseStream());
        }

    }
}