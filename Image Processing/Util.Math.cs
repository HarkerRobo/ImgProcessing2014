using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageProcessing2014 
{
    partial class Util 
    {
        public class Math 
        {
            /// <summary>
            /// angle of prev-now-next in radians(?)
            /// </summary>
            /// <param name="prev"></param>
            /// <param name="now"></param>
            /// <param name="next"></param>
            /// <returns>the angle prev-now-next</returns>
            public static double GetAngle(Point prev, Point now, Point next)
            {
				double sinAngle = GetSinAngle (prev, now, next);
				double cosAngle = GetCosAngle (prev, now, next);
				if (sinAngle < 0) {
					return 2 * System.Math.PI - System.Math.Acos (cosAngle);
				}
				return System.Math.Acos(cosAngle);
            }

            /// <summary>
            /// returns the cosine of the angle prev-now-next
            /// </summary>
            /// <param name="prev"></param>
            /// <param name="now"></param>
            /// <param name="next"></param>
            /// <returns>the cosine of the angle prev-now-next</returns>
            public static double GetCosAngle(Point prev, Point now, Point next)
            {
                return Dot(prev, now, next) / (GetDistance(prev, now) * GetDistance(now, next));
            }

            /// <summary>
            /// returns the cosine of the angle between vector1 and vector2
            /// </summary>
            /// <param name="vector1"></param>
            /// <param name="vector2"></param>
            /// <returns>the cosine of the angle between vector1 and vector2</returns>
            public static double GetCosAngle(Point vector1, Point vector2)
            {
                //cos theta = (a dot b) / (magnitude a * magnitude b)
                return System.Math.Acos((Dot(vector1, vector2) / (GetMagnitude(vector1) * GetMagnitude(vector2))));
            }

            /// <summary>
            /// returns the sine of the angle between vector1 and vector2
            /// </summary>
            /// <param name="vector1"></param>
            /// <param name="vector2"></param>
            /// <returns>the sine of the angle between vector1 and vector2</returns>
            public static double GetSinAngle(Point vector1, Point vector2)
            {
                return Cross(vector1, vector2) / (GetMagnitude(vector1) * GetMagnitude(vector2));
            }

			public static double GetSinAngle(Point prev, Point now, Point next)
			{
				return Cross(prev, now, next) / (GetDistance(prev, now) * GetDistance(now, next));
			}


            /// <summary>
            /// returns the tangent of the angle between vector1 and vector2
            /// </summary>
            /// <param name="vector1"></param>
            /// <param name="vector2"></param>
            /// <returns>the tangent of the angle between vector1 and vector2</returns>
            public static double GetTanAngle(Point vector1, Point vector2)
            {
                return (GetSinAngle(vector1, vector2) / GetCosAngle(vector1, vector2));
            }

            /// <summary>
            /// returns the cross of vector1 and vector2
            /// </summary>
            /// <param name="vector1"></param>
            /// <param name="vector2"></param>
            /// <returns>the cross of vector1 and vector2</returns>
            private static double Cross(Point vector1, Point vector2)
            {
                return vector1.X * vector2.Y - vector1.Y * vector2.X;
            }

			private static double Cross(Point prev, Point now, Point next)
			{
				var v1 = new Point(now.X - prev.X, now.Y - prev.Y);
				var v2 = new Point(next.X - now.X, next.Y - now.Y);
				return v1.X * v2.Y - v1.Y * v2.X;
			}


            /// <summary>
            /// returns the dot of prev-now and now-next
            /// </summary>
            /// <param name="prev"></param>
            /// <param name="now"></param>
            /// <param name="next"></param>
            /// <returns>the dot of prev-now and now-next</returns>
            private static double Dot(Point prev, Point now, Point next)
            {
				var v1 = new Point(prev.X - now.X, prev.Y - now.Y);
                var v2 = new Point(next.X - now.X, next.Y - now.Y);
                return v1.X * v2.X + v1.Y * v2.Y;
            }

            /// <summary>
            /// returns the dot two vectors, v1 and v2
            /// </summary>
            /// <param name="v1"></param>
            /// <param name="v2"></param>
            /// <returns>the dot of v1 and v2</returns>
            private static double Dot(Point v1, Point v2)
            {
                return v1.X * v2.X + v1.Y * v2.Y;
            }

            /// <summary>
            /// returns the distance between the two points
            /// </summary>
            /// <param name="first"></param>
            /// <param name="second"></param>
            /// <returns>the distance between the two points</returns>
            public static double GetDistance(Point first, Point second)
            {
                var diff = new Point(first.X - second.X, first.Y - second.Y);
				return System.Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
            }

			/// <summary>
			/// returns the distance between the two double points
			/// </summary>
			/// <param name="first"></param>
			/// <param name="second"></param>
			/// <returns>the distance between the two points</returns>
			public static double GetDistance(DoublePoint first, DoublePoint second)
			{
				var diff = new DoublePoint(first.X - second.X, first.Y - second.Y);
				return System.Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
			}

            /// <summary>
            /// returns the magnitude of the vector
            /// </summary>
            /// <param name="vector"></param>
            /// <returns>the magnitude of the vector</returns>
            public static double GetMagnitude(Point vector)
            {
				return System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            }

            /// <summary>
            /// returns the horizontal distance across the image at a given distance from the camera in inches
            /// </summary>
            /// <param name="dist">the distance from the camera</param>
            /// <param name="w">the width of the image in pixels</param>
            /// <returns>the horizontal distance across the image (inches)</returns>
            private static double GetInchesPerPixels(double dist, int w)
            {
                return VisionConstants.Camera.H_FOV_INCHES * dist / w;
            }

			/// <summary>
			/// convert rectangular point to polar point
			/// </summary>
			/// <param name="x">x coordinate</param>
			/// <param name="y">y coordinate</param>
			/// <returns>returns polar coordinates</returns>
			public static DoublePoint RectangularToPolar(Point p)
			{
				// the center of the image has coordinates 320, 240
				double x = p.X - 320;
				double y = p.Y - 240;
				double r = System.Math.Sqrt (System.Math.Pow (x, 2) + System.Math.Pow (y, 2));
				double theta = System.Math.Atan2(y,x);
				return new DoublePoint(r, theta);
			}

			/// <summary>
			/// convert polar point to rectangular point
			/// </summary>
			/// <param name="r">r</param>
			/// <param name="theta">theta</param>
			/// <returns>returns rectangular coordinates</returns>
			public static DoublePoint PolarToRectangular(double r, double theta)
			{

				double x = r * System.Math.Cos (theta);
				double y = r * System.Math.Sin (theta);
				// the center of the image has coordinates 320, 240
				x = x + 320;
				y = y + 240;
				return new DoublePoint(x, y);
			} 
        }
    }
}
