using System;
using System.Drawing;
using System.Windows;

namespace ImageProcessing2014 {
    public unsafe class VisionConstants {
        public static class Camera {

            /// <summary>
            ///     the height of the green (goals) camera in inches
            /// </summary>
            public const double CAMERA_HEIGHT = 40; //TODO: Get actual goal camera height

            /// <summary>
            /// the x-offset of the goal camera from the center of the robot
            /// </summary>
            public const double CAMERA_X = 0; //TODO: get correct goal cam x


            public static int IMAGE_WIDTH, IMAGE_HEIGHT, IMAGE_STRIDE;

            //private static int image_width, image_height, image_stride;

            //public static int IMAGE_WIDTH {
            //    get { return image_width; }
            //    set { if(image_width==null) image_width = value; }
            //}

            //public static int IMAGE_HEIGHT {
            //    get { return image_height; }
            //    set { if(image_height==null) image_height = value; }
            //}

            //public static int IMAGE_STRIDE {
            //    get { return image_stride; }
            //    set { if(image_stride==null) image_stride = value; }
            //}

            /// <summary>
            ///     this is delta X between retro tape and the center of the goal in meters
            /// </summary>
			public const double VT_GOAL_DELTA_X = 1.06045;


            /// <summary>
            ///     the ratio of width to height of our image
            /// </summary>
            //public const float ASPECT_RATIO = IMAGE_WIDTH/(float)IMAGE_HEIGHT;

            /// <summary>
            ///     horizontal fov of the cameras in radians
            /// </summary>
            public const double H_FOV = (67.0*Math.PI/180.0);

            /// <summary>
            ///     vertical fov of the cameras in radians
            /// </summary>
            //public const double V_FOV = H_FOV/ASPECT_RATIO;

            /// <summary>
            ///     the horizontal fov in terms of inches at 1in. away
            /// </summary>
            public const double H_FOV_INCHES = 76.0f/71.0f; // width(in.) / dist(in.)

            /// <summary>
            ///     the angle between horizontal and center ray of camera
            /// </summary>
            public const double CAMERA_ROLL = 30*Math.PI/180;

            /// <summary>
            /// the yaw of the goal camera from robot z-axis (straight forward/backwards)
            /// </summary>
            public const double CAMERA_YAW = 0;

            public const double CAMERA_PITCH = Math.PI/6;
            
			//in pixels
			public const double F = 531.6;

            /// <summary>
            /// returns the angle in radians corresponding to a given pixel distance
            /// </summary>
            /// <param name="pixels">distance in pixels on the image</param>
            /// <returns>angle in radians corresponding to a given pixel distance</returns>
            public static double GetAngleFromPixels(int pixels) {
                return (double)(pixels)/IMAGE_WIDTH*H_FOV;
            }

            public static int GetPixelsFromAngle(double angle) {
                return (int)(angle*IMAGE_WIDTH/H_FOV);
            }

            //public static Point GetPointFromVector(System.Windows.Vector v)
            //{
            //	System.Windows.Vector vector = v.GetData();
            //	var p = new Point(GetPixelsFromAngle(Math.Atan2(vector[0],vector[2])),GetPixelsFromAngle((Math.Atan2(vector[1],vector[2]))));
            //	p.Offset(new Point(IMAGE_WIDTH / 2, -p.Y * 2 + IMAGE_HEIGHT / 2));
            //	return p;
            //}

        }

        public static class Field {
            public enum VisionTargetOrientation { Vertical, Horizontal };
			public const double GoalWidthM = 62*0.0254;
			public const double GoalHeightM = 29*0.0254;
			public const double GoalWidthH = 62*0.0254;
			public const double GoalHeightH = 20*0.0254;
			public const double GoalWidthL = 37*0.0254;
			public const double GoalHeightL = 32*0.0254;

			public const double GoalElevationH = 104.124*0.0254;
			public const double GoalElevationM = 88.625*0.0254;

			public const double VISION_TARGET_V_HEIGHT = 32*0.0254;
			public const double VISION_TARGET_V_WIDTH = 4*0.0254;
			public const double VISION_TARGET_H_HEIGHT = 4*0.0254;
			public const double VISION_TARGET_H_WIDTH = 23.5*0.0254;

        }
    }
}

