using System;
namespace ImageProcessing2014 {
    partial class Util {
        public unsafe class Drawing {
            public const double MinValue = 0;
            public const double MaxValueSnL = 1;
            public const double MaxValueH = 360;

            //color constants
			public const uint WHITE  = 0xFFFFFFFF;
			public const uint GRAY   = 0xFF7F7F7F;
			public const uint BLACK  = 0xFF000000;
			public const uint RED    = 0xFFFF0000;
			public const uint GREEN  = 0xFF00FF00;
			public const uint BLUE   = 0xFF0000FF;
			public const uint YELLOW = 0xFFFFFF00;
			public const uint CYAN   = 0xFF00FFFF;


            /// <summary>
            /// draws a solid (filled) rectangle
            /// </summary>
            /// <param name="pScan0">pointer to the first pixel of the image</param>
            /// <param name="stride">image stride</param>
            /// <param name="imageWidth">image width</param>
            /// <param name="imageHeight">image height</param>
            /// <param name="left">top left x-coordinate of the rectangle</param>
            /// <param name="top">top left Y-coordinate of the rectangle</param>
            /// <param name="width">width of rectangle to draw</param>
            /// <param name="height">height of rectangle to draw</param>
            /// <param name="color">color to draw rectangle</param>
            public static void FillRectangle(
                byte* pScan0, int stride, int imageWidth, int imageHeight,
                int left, int top, int width, int height, uint color) {
                var startX = System.Math.Max(0, left);
                var startY = System.Math.Max(0, top);
                var endX = System.Math.Min(left + width, imageWidth - 1);
                var endY = System.Math.Min(top + height, imageHeight - 1);

                for(var y = startY; y <= endY; y++) {
                    var pScan = (uint*)(pScan0 + stride * y + startX * 4);
                    for(var x = startX; x <= endX; x++, pScan++)
                        *(pScan) = color;
                }
            }

            /// <summary>
            /// draws the outline of a rectangle
            /// </summary>
            /// <param name="pScan0">pointer to the first pixel of the image</param>
            /// <param name="stride">image stride</param>
            /// <param name="imageWidth">image width</param>
            /// <param name="imageHeight">image height</param>
            /// <param name="rectangle">rectangle to draw</param>
            /// <param name="color">color to draw outline</param>
            public static void DrawRectangle(
                byte* pScan0, int stride, int imageWidth, int imageHeight,
                System.Drawing.Rectangle rectangle, uint color) {
                var startX = System.Math.Max(0, rectangle.X);
                var startY = System.Math.Max(0, rectangle.Y);
                var width = rectangle.Width;
                //            var height = rectangle.Height;
                //            var endX = Math.Min(startX + rectangle.Width, imageWidth - 1);
                var endY = System.Math.Min(startY + rectangle.Height, imageHeight - 1);

                //top side
                var pScan = (uint*)(pScan0 + stride * startY + startX * 4);
                for(var x = 0; x <= width; x++, pScan++)
                    *pScan = color;

                //left/right sides
                for(var y = startY + 1; y < endY; y++) {
                    pScan = (uint*)(pScan0 + stride*y + startX*4);
                    *pScan = color;
                    pScan += width;
                    *pScan = color;
                }

                //bottom side
                pScan = (uint*)(pScan0 + stride*endY + startX*4);
                for(var x = 0; x <= width; x++, pScan++)
                    *pScan = color;
            }

            public static void FillRectangle(
                byte* pScan0, int stride, int imageWidth, int imageHeight,
                System.Drawing.Rectangle rectangle, uint color) {
                var startX = System.Math.Max(0, rectangle.X);
                var startY = System.Math.Max(0, rectangle.Y);
                var endX = System.Math.Min(startX + rectangle.Width, imageWidth - 1);
                var endY = System.Math.Min(startY + rectangle.Height, imageHeight - 1);

                for(var y = startY; y <= endY; y++) {
                    var pScan = (uint*)(pScan0 + stride * y + startX * 4);
                    for(var x = startX; x <= endX; x++, pScan++)
                        *(pScan) = color;
                }
            }
        }
    }
}

