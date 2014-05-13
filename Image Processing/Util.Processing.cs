using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageProcessing2014 {
	partial class Util {
		public unsafe class Processing {
			const double PROPORTION_THRESHOLD = .60;
			//Percentage error forgiveness
			const double PROPORTION_THRESHOLD_LO = 1 - PROPORTION_THRESHOLD;
			const double PROPORTION_THRESHOLD_HI = 1 + PROPORTION_THRESHOLD;
			//Width to height ratios for horizontal target.
			const double ACTUAL_TARGET_H_RATIO = VisionConstants.Field.VISION_TARGET_H_WIDTH / VisionConstants.Field.VISION_TARGET_H_HEIGHT;
			//Height to width ratio for vertical target (so proportion code will work.)
			const double ACTUAL_TARGET_V_RATIO = VisionConstants.Field.VISION_TARGET_V_HEIGHT / VisionConstants.Field.VISION_TARGET_V_WIDTH;
			private const int MIN_BLOB_COUNT = 500;
			private const int BALL_WIDTH = 20;

			/// <summary>
			/// return all blobs for the given test and mark them as filled
			/// </summary>
			/// <param name="pStates">pointer to the beginning of the array of pixel states</param>
			/// <param name="imageWidth">image width</param>
			/// <param name="imageHeight">image height</param>
			/// <param name="fill">the fill to use to fill pixels that past test</param>
			/// <param name="test">the test to use to test whether to fill pixel</param>
			/// <returns></returns>
			public static List<Blob> FindBlobs(byte* pStates, PixelFiller.PixelFill fill, PixelTester.PixelTest test) {
				List<Blob> blobs = new List<Blob>();

				for(Int32 yOffset = 0; yOffset < VisionConstants.Camera.IMAGE_HEIGHT; yOffset++) {
					Byte* pCurrentState = pStates + yOffset * VisionConstants.Camera.IMAGE_WIDTH;
					for(Int32 xOffset = 0; xOffset < VisionConstants.Camera.IMAGE_WIDTH; xOffset++) {
						if(test(*pCurrentState)) {
							Blob blob = new Blob();
							Util.Processing.FloodFill(xOffset, yOffset, pStates, fill, test, blob);
							blobs.Add(blob);
						}
						pCurrentState++;
					}
				}
				//DrawBlobs(blobs, pscan0, stride, VisionConstants.Camera.IMAGE_WIDTH, VisionConstants.Camera.IMAGE_HEIGHT, VisionConstants.Colors.RED);
				return blobs;
			}

			public static List<Blob> FilterVTBlobs(List<Blob> blobs) {
				for(int i = blobs.Count() - 1; i >= 0; i--)
					if(blobs[i].CalculatePointCount() < MIN_BLOB_COUNT) //remove blobs that are too small
                        blobs.Remove(blobs[i]);
				for(int i = blobs.Count() - 1; i >= 0; i--) {
					if(blobs[i].Corners.Count() != 4) { //remove blobs that are too small
		
						blobs.Remove(blobs[i]);
					}
					else {
						OrderCorners(blobs[i].Corners);
					}
				}
				double centerY;
				for(int i = blobs.Count() - 1; i >= 0; i--) { //remove blobs that are in the top 2/5 of the image
					centerY = (((blobs[i].Corners[2].Y + blobs[i].Corners[0].Y) / 2) + ((blobs[i].Corners[3].Y + blobs[i].Corners[1].Y) / 2)) / 2;
					if(centerY <= 2 * VisionConstants.Camera.IMAGE_HEIGHT / 5)
						blobs.Remove(blobs[i]);
				}
				double width, height;
				double ratioWH, ratioHW;

				for(int i = blobs.Count - 1; i >= 0; i--) {
//					Console.WriteLine("Blob #" + i + " Corner 1 x: " + blobs[i].Corners[0].X + " y: " + blobs[i].Corners[0].Y);
//					Console.WriteLine("Blob #" + i + " Corner 2 x: " + blobs[i].Corners[1].X + " y: " + blobs[i].Corners[1].Y);
//					Console.WriteLine("Blob #" + i + " Corner 3 x: " + blobs[i].Corners[2].X + " y: " + blobs[i].Corners[2].Y);
//					Console.WriteLine("Blob #" + i + " Corner 4 x: " + blobs[i].Corners[3].X + " y: " + blobs[i].Corners[3].Y);
					//width and height initialization
//					double distw1 = Math.GetDistance(blobs[i].Corners[0], blobs[i].Corners[1]);
//					double distw2 = Math.GetDistance(blobs[i].Corners[2], blobs[i].Corners[3]);
//					double disth1 = Math.GetDistance(blobs[i].Corners[1], blobs[i].Corners[3]);
//					double disth2 = Math.GetDistance(blobs[i].Corners[0], blobs[i].Corners[2]);
					Rectangle boundingBox = blobs [i].CalculateBoundingBox ();
					width = boundingBox.Width; //(distw1+distw2) / 2
					height = boundingBox.Height; // + ((disth2+ disth1)/2)/ System.Math.Cos(VisionConstants.Camera.CAMERA_PITCH); //Accounts for pitch of camera which throws off image.

					Console.WriteLine(height + " " + width);

					//ratio initialization
					ratioWH = width / height;
					ratioHW = height / width;

//					Console.WriteLine("Blob #" + i + " ratioWH: " + ratioWH);
//					Console.WriteLine("Blob #" + i + " ratioHW: " + ratioHW);
					/* If the ratio of the width to the height of the target does not fall somewhere between ± proportion_threshold percent
                     * of the actual horizontal target dimensions and the ratio of the height to the width of the target does not fall somewhere 
                     * between ± proportion_threshold percent of the actual vertical target dimensions then remove it from the list of blobs as it 
                     * probably does not represent one of the targets. As the orientation of the potential target is unknown, it must be compared 
                     * against the ratios of the vertical and horizontal target. Since the ratio of width to height for the vertical target would be
                     * too small, instead the height to width ratio should be compared.
                     */
					if(!((PROPORTION_THRESHOLD_LO * ACTUAL_TARGET_H_RATIO <= ratioWH) && (ratioWH <= PROPORTION_THRESHOLD_HI * ACTUAL_TARGET_H_RATIO)) &&
					   !((PROPORTION_THRESHOLD_LO * ACTUAL_TARGET_V_RATIO <= ratioHW) && (ratioHW <= PROPORTION_THRESHOLD_HI * ACTUAL_TARGET_V_RATIO))) {
//						Console.WriteLine("Removed P Blob #" + i + " Corner 1 x: " + blobs[i].Corners[0].X + " y: " + blobs[i].Corners[0].Y);
//						Console.WriteLine("Removed P Blob #" + i + " Corner 2 x: " + blobs[i].Corners[1].X + " y: " + blobs[i].Corners[1].Y);
//						Console.WriteLine("Removed P Blob #" + i + " Corner 3 x: " + blobs[i].Corners[2].X + " y: " + blobs[i].Corners[2].Y);
//						Console.WriteLine("Removed P Blob #" + i + " Corner 4 x: " + blobs[i].Corners[3].X + " y: " + blobs[i].Corners[3].Y);
						blobs.Remove(blobs[i]);
					}
				}
				Console.WriteLine("Num of blobs" + blobs.Count);
				return blobs;
			}

			/// <summary>
			/// This code attempts to order the corners passed to it. Note that this code works on the basic
			/// assumption that the corners passed define a rectangle-like object with four corners.
			/// </summary>
			/// <returns>The index of the top-left point.</returns>
			/// <param name="cornerList">A list containing the corners of the blob.</param>
			public static void OrderCorners(List<Point> cornerList) {
                int xsum = 0;
                int ysum = 0;
                List<Tuple<double, Point> > atp = new List<Tuple<double, Point> >();
                foreach (Point p in cornerList)
                {
                    xsum += p.X;
                    ysum += p.Y;
                }
                Point center = new Point(xsum / 4, ysum / 4);
                int dx = System.Math.Abs(cornerList[0].X - center.X);
                int dy = System.Math.Abs(cornerList[0].Y - center.Y);
                if (dx > dy) //horizontally oriented
                {
                    Point refp = new Point(center.X, center.Y + 1);
                    foreach(Point p in cornerList) {
                        double angle = Math.GetAngle(refp,center,p);
                        atp.Add(new Tuple<double, Point>(angle,p));
                    }
                    atp.Sort();
                    List<Point> orderedCorners = new List<Point>();
                    orderedCorners.Add(atp[2].Item2);
                    orderedCorners.Add(atp[1].Item2);
                    orderedCorners.Add(atp[3].Item2);
                    orderedCorners.Add(atp[0].Item2);
                    for (int i = 0; i < 4; i++) {
                        cornerList[i] = orderedCorners[i];
                    }
                }
                else //vertically oriented
                {
                    Point refp = new Point(center.X + 1, center.Y);
                    foreach(Point p in cornerList) {
                        double angle = Math.GetAngle(refp,center,p);
                        atp.Add(new Tuple<double, Point>(angle,p));
                    }
                    atp.Sort();
                    List<Point> orderedCorners = new List<Point>();
                    orderedCorners.Add(atp[1].Item2);
                    orderedCorners.Add(atp[0].Item2);
                    orderedCorners.Add(atp[2].Item2);
                    orderedCorners.Add(atp[3].Item2);
                    for (int i = 0; i < 4; i++) {
                        cornerList[i] = orderedCorners[i];
                    }
                }

			}

			/// <summary>
			/// fill the blob containing the given point
			/// </summary>
			/// <param name="x">x-coordinate of the point to fill</param>
			/// <param name="y">Y-coordinate of the point to fill</param>
			/// <param name="blob">blob to add to</param>
			/// <param name="pStates">pointer to the start of the array of pixel states</param>
			/// <param name="imageWidth">image width</param>
			/// <param name="imageHeight">image height</param>
			/// <param name="fill">the fill to use to fill pixels that past test</param>
			/// <param name="test">the test to use to test whether to fill pixel</param>
			/// <returns>nothing</returns>

			// refer to the wiki page http://robo.harker.org/wiki/Blob_Finder for a conceptual explanation

			public unsafe static void FloodFill(
				int x, int y,
				byte* pStates,
				PixelFiller.PixelFill fill,
				PixelTester.PixelTest test,
				Blob blob
			) {
				/* pStates is a pointer that points to the first filtered pixel, location (0, 0)
                 * During each iteration, pCurrentState points is incremented to the next pixel in
                 * the array
                 */
				byte* pCurrentState = pStates + y * VisionConstants.Camera.IMAGE_WIDTH + x;
				// Makes variables containing the x-coordinates of the filtered pixels to the left and right of pCurrentState
				int leftX = x - 1;
				int rightX = x + 1;

				// Makes pointers to the pixels to the sides of pCurrentState
				byte* pLeftState = pCurrentState - 1;
				byte* pRightState = pCurrentState + 1;

				// Marks the current pixel as visited so we only check it once
				fill(pCurrentState);

				// Runs checking pixels to the left of pCurrentState until it hits the end of the image or a pixel that is not a seed
				while(leftX > 0 && test(*pLeftState)) {

					// marks the pixel as visited
					fill(pLeftState);
					// increments pLeftState and leftX, effectively moving to the left by one pixel
					pLeftState--;
					leftX--;
				}
				// same thing as the previous while loop but to the right
				while(rightX < VisionConstants.Camera.IMAGE_WIDTH && test(*pRightState)) {
					fill(pRightState);
					pRightState++;
					rightX++;
				}

				// we define a blob to be a list of line segments
				// here we add the line segment to our blob determined by the above while loop
				LineSegment ls = new LineSegment(leftX + 1, rightX, y);
				blob.AddLineSegment(ls);

				// for loop runs through all the pixels in the line segment
				for(int i = leftX + 1; i < rightX; i++) {
					// y > 0 just a precaution
					if(y > 0) {
						// creates a pointer to the pixel directly above the i-th pixel in the line segment
						byte* pUpState = pStates + (y - 1) * VisionConstants.Camera.IMAGE_WIDTH + i;

						// tests if pUpState is a seed
						if(test(*pUpState)) {
							// recursively calls Floodfill on pUpState
							FloodFill(
								i, y - 1,
								pStates,
								fill, test,
								blob
							);
						}
					}
					// does the same thing as the previous if statement, except down instead of up
					if(y < VisionConstants.Camera.IMAGE_HEIGHT) {
						byte* pDownState = pStates + (y + 1) * VisionConstants.Camera.IMAGE_WIDTH + i;
						if(test(*pDownState)) {
							FloodFill(
								i, y + 1,
								pStates,
								fill, test,
								blob);
						}
					}
				}
			}

			/// <summary>
			/// Calculates distance from an image
			/// </summary>
			/// <returns>Distance From Image.</returns>
			/// <param name="topLeft">Top left point.</param>
			/// <param name="topRight">Top right point.</param>
			/// <param name="bottomLeft">Bottom left point.</param>
			/// <param name="bottomRight">Bottom right point.</param>
			/// <param name="orientation">Orientation.</param>
			public static double GetDistance(DoublePoint topLeft, DoublePoint topRight, DoublePoint bottomLeft, DoublePoint bottomRight, VisionConstants.Field.VisionTargetOrientation orientation) {
				var x = (bottomRight.X - bottomLeft.X + topRight.X - topLeft.X) / 2.0;

				var angle = x / VisionConstants.Camera.IMAGE_WIDTH * VisionConstants.Camera.H_FOV;

				double distance;
				if(orientation == VisionConstants.Field.VisionTargetOrientation.Horizontal)
					distance = (VisionConstants.Field.VISION_TARGET_H_WIDTH / 2) / System.Math.Tan(angle / 2);
				else
					distance = (VisionConstants.Field.VISION_TARGET_V_WIDTH / 2) / System.Math.Tan(angle / 2);

				return distance;
			}

			/// <summary>
			/// Gets angle from center in radians. Assumes unfished points
			/// </summary>
			/// <param name="topLeft"></param>
			/// <param name="topRight"></param>
			/// <param name="bottomRight"></param>
			/// <param name="bottomLeft"></param>
			/// <returns></returns>
			public static double GetAngle(DoublePoint topLeft, DoublePoint topRight, DoublePoint bottomLeft, DoublePoint bottomRight) {
				double avX = (topLeft.X + topRight.X + bottomLeft.X + bottomRight.X) / 4.0;
				//Console.WriteLine(avX);
				int centerX = VisionConstants.Camera.IMAGE_WIDTH / 2;
				//Console.WriteLine ((avX - centerX) * VisionConstants.Camera.H_FOV / VisionConstants.Camera.IMAGE_WIDTH);
				return (avX - centerX) * VisionConstants.Camera.H_FOV / VisionConstants.Camera.IMAGE_WIDTH;

			}

			/// <summary>
			/// Takes in an array of candidacies. 0 is none, 1 is candidate, 2 is seed. x and y are the point to start at.
			/// The array has to be in the form of a byte* with width and height values given.
			/// If candidacies[x][y] is not a seed will do nothing. (Should make an error in this case?)
			/// Flood fills all the candidates adjacent to seeds and replaces them with seeds, within the original array.
			/// Author: Ben H.
			/// </summary>
			/// <returns> Somewhat unclear but it returns something, can Brian or Rahul handle this? </returns>
            //public unsafe static LineSegment[] lqFloodFill(byte* candidacy, int width, int height, int x, int y) {
            //    byte* index = ByteArrayPtrIndex(candidacy, width, height, x, y);
            //    if(*index != 2) {
            //        // Something is wrong
            //        // Act accordingly
            //    }
            //    List<LineSegment> returns = lqFloodFillHelper(candidacy, width, height, index);
            //    // Internally uses a value of 3 to mean explored, have to remove this
            //    for(int i = 0; i < width * height; i++) {
            //        if(*(candidacy + i) == 3) {
            //            // can I do this?
            //            *(candidacy + i) = 2;
            //        }
            //    }
            //    return returns.ToArray();
            //}

            ///// <summary>
            ///// Have to call on a point with a value of 1 or 2 in it already, otherwise won't work properly
            ///// Does same thing as lqFloodFill
            ///// </summary>
            ///// <param name="candidacy"></param>
            ///// <param name="x"></param>
            ///// <param name="y"></param>
            ///// <returns> Same as lqFLoodFill </returns>
            //private unsafe static List<LineSegment> lqFloodFillHelper(byte* candidacy, int width, int height, byte* current) {

            //    List<LineSegment> toReturn = new List<LineSegment>();
            //    byte* testPtr = current;
            //    // Are these correct? I might be off by 1 somewhere
            //    byte* lowRowBound = current - ((current - candidacy) % width);
            //    byte* highRowBound = lowRowBound + width;
            //    List<int> newPoints = new List<int>();
            //    // Fill all the points to the left of the starting point and put them on the list of points to try
            //    while(testPtr >= lowRowBound && (*testPtr == 1 || *testPtr == 2)) {
            //        *testPtr = 3;
            //        newPoints.Add((int)(testPtr - lowRowBound));
            //        testPtr--;
            //    }
            //    //toReturn.Add((int)testPtr);
            //    byte* lowX = testPtr;
            //    // Same for the right
            //    testPtr = current + 1;
            //    // Should this be candidacy[0] or candidacy for length of a row?
            //    while(testPtr < highRowBound - 1 && (*testPtr == 1 || *testPtr == 2)) {
            //        *testPtr = 3;
            //        newPoints.Add((int)(testPtr - lowRowBound));
            //        testPtr++;
            //    }
            //    //toReturn.Add((int)testPtr);
            //    byte* highX = testPtr;
            //    toReturn.Add(new LineSegment((int)(lowX - lowRowBound), (int)(highX - lowRowBound), (int)(current - (int)candidacy / width)));

            //    foreach(int newX in newPoints) {
            //        byte* newPointPtr = lowRowBound + newX;
            //        if((int)newPointPtr > width) {
            //            if(*(newPointPtr - width) == 1 || *(newPointPtr - width) == 2) {
            //                // This is apparently wrong
            //                toReturn.AddRange(lqFloodFillHelper(candidacy, width, height, newPointPtr - width));
            //            }

            //        }
            //        // again should I use candidacy[0]?
            //        if((int)newPointPtr < (height - 1) * width) {
            //            if(*(newPointPtr + width) == 1 || *(newPointPtr + width) == 2) {
            //                // This is apparently wrong
            //                toReturn.AddRange(lqFloodFillHelper(candidacy, width, height, newPointPtr + width));
            //            }
            //        }
            //    }
            //    return toReturn;
            //}

			/// <summary>
			/// Assumes no padding
			/// </summary>
			/// <param name="width"></param>
			/// <param name="height"></param>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <returns></returns>
			public unsafe static byte* ByteArrayPtrIndex(byte* start, int width, int height, int x, int y) {
				if(y < 0 || y >= height || x < 0 || x >= width) {
					// Problem
					return (byte*)0;
				}
				return start + (x + ((y - 1 * width)));
			}

			public static void DrawBlobs(List<Blob> bloblist, byte* pscan0, int stride, int imageWidth, int imageHeight, uint color) {
				foreach(Blob blob in bloblist) {
					List<LineSegment> linesegments = blob.GetLineSegments();
					foreach(LineSegment ls in linesegments) {
						Rectangle rectangle = new Rectangle(ls.leftX, ls.Y, ls.rightX - ls.leftX, 1);
						Util.Drawing.DrawRectangle(pscan0, stride, imageWidth, imageHeight, rectangle, color);
					}
				}
			}

            //private static unsafe List<Point> FindCHullBen(byte* pscan0, int width, int height, int existingPointValue) {
            //    // This is set up to run on the filtered image directly not on a Blob, which is stored as a list of line segments.
            //    // To make it run on a blob have to do this. (Note: convertToArray is not implemented, I was going to but ran out of time.)
            //    // Have to use 3 method calls here to get 3 values back.
            //    //int width = GetMaxX();
            //    //int height = GetMaxY();
            //    // Note: this can only bother with the ends of the line segments. The points in the middle will never be a part of the convex hull.
            //    // Points that are part of the blob are existingPointValue, rest are something else, what it is doesn't matter.
            //    //byte* pscan = convertToArray(blob, width, height);

            //    // If using the image start here.

            //    List<Point> vertices = new List<Point>();

            //    int xrange = width - 1;
            //    int yrange = height - 1;

            //    // would be faster with a linked list but this is just a proof of concept
            //    List<Point> currentBounds = new List<Point>();
            //    Point origin = new Point(0, 0);
            //    Point xaxis = new Point(xrange, 0);
            //    Point yaxis = new Point(0, yrange);
            //    Point xy = new Point(xrange, yrange);
            //    currentBounds.Add(origin);
            //    currentBounds.Add(xaxis);
            //    currentBounds.Add(xy);
            //    currentBounds.Add(yaxis);

            //    bool hasOrigin = true;
            //    bool hasXaxis = true;
            //    bool hasYaxis = true;
            //    bool hasXY = true;

            //    int rectPointsLeft = 4;



            //    // These will be equal except during checking of the vertices.
            //    // During checking I need to keep them separated since moving a point
            //    // diagonally can jump over lines, so I need to increment them independently.
            //    int distFromEdgesX = 0;
            //    int distFromEdgesY = 0;

            //    while(rectPointsLeft > 0) {
            //        // add any points on the current boundary to the current boundary
            //        // do all of them but the last -> the first
            //        for(int i = 0; i < currentBounds.Count - 1; i++) {
            //            List<Point> line = GetPointsOnLine(currentBounds.ElementAt(i), currentBounds.ElementAt(i + 1));
            //            //System.out.println(currentBounds.get(i) + " to " + currentBounds.get(i+1) + " is:");
            //            //System.out.println(line);
            //            for(int j = 0; j < line.Count; j++) {
            //                if(*(pscan0 + width * (line.ElementAt(j).Y) + line.ElementAt(j).X) == existingPointValue) {
            //                    vertices.Add(line.ElementAt(j));
            //                    currentBounds.Insert(i + 1, line.ElementAt(j));
            //                    break;
            //                }
            //            }
            //        }
            //        // close the boundary by doing the last -> first line
            //        Point lastPoint = currentBounds.ElementAt(currentBounds.Count - 1);
            //        List<Point> lastLine = GetPointsOnLine(lastPoint, currentBounds.ElementAt(0));
            //        for(int j = 0; j < lastLine.Count; j++) {
            //            if(*(pscan0 + width * (lastLine.ElementAt(j).Y) + lastLine.ElementAt(j).X) == 1) {
            //                vertices.Add(lastLine.ElementAt(j));
            //                currentBounds.Add(lastLine.ElementAt(j));
            //                break;
            //            }
            //        }

            //        distFromEdgesX++;
            //        // Move all corners inward, checking them to see if they cross any
            //        // lines. If they cross any lines remove them from the list.

            //        // Origin is guaranteed to be 1st in the list if it exists so it
            //        // has a special case for the checking to see if it's on a line.
            //        if(hasOrigin) {
            //            origin.X = distFromEdgesX;
            //            Point last = currentBounds.ElementAt(currentBounds.Count - 1);
            //            List<Point> checkline = GetPointsOnLine(last, currentBounds.ElementAt(0));
            //            checkline.Add(last);
            //            checkline.Add(currentBounds.ElementAt(0));
            //            for(int i = 0; i < checkline.Count; i++) {
            //                if(checkline.ElementAt(i).X == origin.X && checkline.ElementAt(i).Y == origin.Y) {
            //                    // remove the origin from the list
            //                    hasOrigin = false;
            //                    currentBounds.Remove(origin);
            //                    rectPointsLeft--;
            //                }
            //            }
            //        }
            //        if(hasXaxis) {
            //            xaxis.X = xrange - distFromEdgesX;
            //            Point before;
            //            Point after;
            //            if(currentBounds.IndexOf(xaxis) == 0) {
            //                before = currentBounds.ElementAt(currentBounds.Count - 1);
            //            } else {
            //                before = currentBounds.ElementAt(currentBounds.IndexOf(xaxis) - 1);
            //            }
            //            if(currentBounds.IndexOf(xaxis) == currentBounds.Count - 1) {
            //                after = currentBounds.ElementAt(0);
            //            } else {
            //                after = currentBounds.ElementAt(currentBounds.IndexOf(xaxis) + 1);
            //            }
            //            List<Point> checkline = GetPointsOnLine(before, after);
            //            checkline.Add(before);
            //            checkline.Add(after);
            //            for(int i = 0; i < checkline.Count; i++) {
            //                if(checkline.ElementAt(i).X == xaxis.X && checkline.ElementAt(i).Y == xaxis.Y) {
            //                    // remove the xaxis from the list
            //                    hasXaxis = false;
            //                    currentBounds.Remove(xaxis);
            //                    rectPointsLeft--;
            //                }
            //            }
            //        }
            //        if(hasYaxis) {
            //            yaxis.X = distFromEdgesX;
            //            Point before;
            //            Point after;
            //            if(currentBounds.IndexOf(yaxis) == 0) {
            //                before = currentBounds.ElementAt(currentBounds.Count - 1);
            //            } else {
            //                before = currentBounds.ElementAt(currentBounds.IndexOf(yaxis) - 1);
            //            }
            //            if(currentBounds.IndexOf(yaxis) == currentBounds.Count - 1) {
            //                after = currentBounds.ElementAt(0);
            //            } else {
            //                after = currentBounds.ElementAt(currentBounds.IndexOf(yaxis) + 1);
            //            }
            //            List<Point> checkline = GetPointsOnLine(before, after);
            //            checkline.Add(before);
            //            checkline.Add(after);
            //            for(int i = 0; i < checkline.Count; i++) {
            //                if(checkline.ElementAt(i).X == yaxis.X && checkline.ElementAt(i).Y == yaxis.Y) {
            //                    // remove the yaxis from the list
            //                    hasYaxis = false;
            //                    currentBounds.Remove(yaxis);
            //                    rectPointsLeft--;
            //                }
            //            }
            //        }
            //        if(hasXY) {
            //            xy.X = xrange - distFromEdgesX;
            //            Point before;
            //            Point after;
            //            if(currentBounds.IndexOf(xy) == 0) {
            //                before = currentBounds.ElementAt(currentBounds.Count - 1);
            //            } else {
            //                before = currentBounds.ElementAt(currentBounds.IndexOf(xy) - 1);
            //            }
            //            if(currentBounds.IndexOf(xy) == currentBounds.Count - 1) {
            //                after = currentBounds.ElementAt(0);
            //            } else {
            //                after = currentBounds.ElementAt(currentBounds.IndexOf(xy) + 1);
            //            }
            //            List<Point> checkline = GetPointsOnLine(before, after);
            //            checkline.Add(before);
            //            checkline.Add(after);
            //            for(int i = 0; i < checkline.Count; i++) {
            //                if(checkline.ElementAt(i).X == xy.X && checkline.ElementAt(i).Y == xy.Y) {
            //                    // remove the XY from the list
            //                    hasXY = false;
            //                    currentBounds.Remove(xy);
            //                    rectPointsLeft--;
            //                }
            //            }
            //        }

            //        // Now change the Y, and do all the checks again.
            //        // This involves great copy/paste but I can't find a better way to do it.
            //        distFromEdgesY++;

            //        if(hasOrigin) {
            //            origin.X = distFromEdgesX;
            //            Point last = currentBounds.ElementAt(currentBounds.Count - 1);
            //            List<Point> checkline = GetPointsOnLine(last, currentBounds.ElementAt(0));
            //            checkline.Add(last);
            //            checkline.Add(currentBounds.ElementAt(0));
            //            for(int i = 0; i < checkline.Count; i++) {
            //                if(checkline.ElementAt(i).X == origin.X && checkline.ElementAt(i).Y == origin.Y) {
            //                    // remove the origin from the list
            //                    hasOrigin = false;
            //                    currentBounds.Remove(origin);
            //                    rectPointsLeft--;
            //                }
            //            }
            //        }
            //        if(hasXaxis) {
            //            xaxis.X = xrange - distFromEdgesX;
            //            Point before;
            //            Point after;
            //            if(currentBounds.IndexOf(xaxis) == 0) {
            //                before = currentBounds.ElementAt(currentBounds.Count - 1);
            //            } else {
            //                before = currentBounds.ElementAt(currentBounds.IndexOf(xaxis) - 1);
            //            }
            //            if(currentBounds.IndexOf(xaxis) == currentBounds.Count - 1) {
            //                after = currentBounds.ElementAt(0);
            //            } else {
            //                after = currentBounds.ElementAt(currentBounds.IndexOf(xaxis) + 1);
            //            }
            //            List<Point> checkline = GetPointsOnLine(before, after);
            //            checkline.Add(before);
            //            checkline.Add(after);
            //            for(int i = 0; i < checkline.Count; i++) {
            //                if(checkline.ElementAt(i).X == xaxis.X && checkline.ElementAt(i).Y == xaxis.Y) {
            //                    // remove the xaxis from the list
            //                    hasXaxis = false;
            //                    currentBounds.Remove(xaxis);
            //                    rectPointsLeft--;
            //                }
            //            }
            //        }
            //        if(hasYaxis) {
            //            yaxis.X = distFromEdgesX;
            //            Point before;
            //            Point after;
            //            if(currentBounds.IndexOf(yaxis) == 0) {
            //                before = currentBounds.ElementAt(currentBounds.Count - 1);
            //            } else {
            //                before = currentBounds.ElementAt(currentBounds.IndexOf(yaxis) - 1);
            //            }
            //            if(currentBounds.IndexOf(yaxis) == currentBounds.Count - 1) {
            //                after = currentBounds.ElementAt(0);
            //            } else {
            //                after = currentBounds.ElementAt(currentBounds.IndexOf(yaxis) + 1);
            //            }
            //            List<Point> checkline = GetPointsOnLine(before, after);
            //            checkline.Add(before);
            //            checkline.Add(after);
            //            for(int i = 0; i < checkline.Count; i++) {
            //                if(checkline.ElementAt(i).X == yaxis.X && checkline.ElementAt(i).Y == yaxis.Y) {
            //                    // remove the yaxis from the list
            //                    hasYaxis = false;
            //                    currentBounds.Remove(yaxis);
            //                    rectPointsLeft--;
            //                }
            //            }
            //        }
            //        if(hasXY) {
            //            xy.X = xrange - distFromEdgesX;
            //            Point before;
            //            Point after;
            //            if(currentBounds.IndexOf(xy) == 0) {
            //                before = currentBounds.ElementAt(currentBounds.Count - 1);
            //            } else {
            //                before = currentBounds.ElementAt(currentBounds.IndexOf(xy) - 1);
            //            }
            //            if(currentBounds.IndexOf(xy) == currentBounds.Count - 1) {
            //                after = currentBounds.ElementAt(0);
            //            } else {
            //                after = currentBounds.ElementAt(currentBounds.IndexOf(xy) + 1);
            //            }
            //            List<Point> checkline = GetPointsOnLine(before, after);
            //            checkline.Add(before);
            //            checkline.Add(after);
            //            for(int i = 0; i < checkline.Count; i++) {
            //                if(checkline.ElementAt(i).X == xy.X && checkline.ElementAt(i).Y == xy.Y) {
            //                    // remove the XY from the list
            //                    hasXY = false;
            //                    currentBounds.Remove(xy);
            //                    rectPointsLeft--;
            //                }
            //            }
            //        }
            //    }

            //    return vertices;
            //}

			private static List<Point> GetPointsOnLine(Point point1, Point point2) {
				List<Point> pointsOnLine = new List<Point>();
				int rise = point2.Y - point1.Y;
				int run = point2.X - point1.X;

				if(System.Math.Abs(run) > System.Math.Abs(rise)) {
					double slope = (double)rise / (double)run;
					if(run > 0) {
						for(int i = 1; i < run; i++) {
							double currentRiseDbl = i * slope;
							int currentRise = (int)System.Math.Round(currentRiseDbl, 0);
							pointsOnLine.Add(new Point(point1.X + i, point1.Y + currentRise));
						}
					} else {
						for(int i = -1; i > run; i--) {
							double currentRiseDbl = i * slope;
							int currentRise = (int)System.Math.Round(currentRiseDbl, 0);
							pointsOnLine.Add(new Point(point1.X + i, point1.Y + currentRise));
						}
					}
				} else {
					double slope = (double)run / (double)rise;
					if(rise > 0) {
						for(int i = 1; i < rise; i++) {
							double currentRunDbl = i * slope;
							int currentRun = (int)System.Math.Round(currentRunDbl, 0);
							pointsOnLine.Add(new Point(point1.X + currentRun, point1.Y + i));
						}
					} else {
						for(int i = -1; i > rise; i--) {
							double currentRunDbl = i * slope;
							int currentRun = (int)System.Math.Round(currentRunDbl, 0);
							pointsOnLine.Add(new Point(point1.X + currentRun, point1.Y + i));
						}
					}
				}
				return pointsOnLine;
			}

			/// <summary>
			/// Corrects fish eye for a point.
			/// </summary>
			/// <param name="p">the point to </param>
			/// <returns>double[0] is X; double[1] is Y</returns>
			public static DoublePoint CorrectFishEye(Point p) {
				DoublePoint polar = ImageProcessing2014.Util.Math.RectangularToPolar(p);
				double f = VisionConstants.Camera.F;
				double newR = polar.X / (1 - (polar.X * polar.X / (4 * f * f)));
				return ImageProcessing2014.Util.Math.PolarToRectangular(newR, polar.Y);
			}
		}
	}
}