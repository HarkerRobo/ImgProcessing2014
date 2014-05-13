using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* FILE LOG:
 * 1/24/14		Added CalculatePointCount(), AddLineSegment(), GetLineSegments() and CalculateBoundingBox()
 * 
 * TODO:
 * 	Maybe change CalculateConvexHull() to Chan's algorihim?
 * 	Need comments for CalculateConvexHull()
 */

namespace ImageProcessing2014 {

    /// <summary>
    /// Blob: A collection of points.
    /// </summary>
    class Blob {
		private const double CORNER_ANGLE_THRESHOLD = 2.71; //The ideal angle between points, in radians. A magic number determined through testing.
		private const double CORNER_DISTANCE_THRESHOLD = 7; //How many pixels apart must two points be before they are unique. Again, arbitrary.


        private List<LineSegment> lineSegments = new List<LineSegment>();

        private List<Point> convexHull;
        private List<Point> corners;

        public List<Point> ConvexHull {
            get {
                if(convexHull==null) convexHull = CalculateConvexHull();
                return convexHull;
            }
        }
        public List<Point> Corners {
            get {
                if(corners==null) corners = FindCorners();
                return corners;
            }
        }

        public List<Point> CalculateConvexHull() {
            Point bottomMost = FindBottomMostPoint();

            List<Point> chull = new List<Point>();

            Point pNow = bottomMost;
            Point pPrev = new Point(pNow.X, -1);
            Point pNext = new Point();

            do {
                //Console.WriteLine(lineSegments.Count);

                chull.Add(pNow);
                double maximumAngle = 0;

                foreach(ImageProcessing2014.LineSegment lineSeg in lineSegments) {

                    //Console.WriteLine(pNow);

                    Point point1 = new Point(lineSeg.rightX, lineSeg.Y);
                    Point point2 = new Point(lineSeg.leftX, lineSeg.Y);
                    
                    if(!point1.Equals(pNow)) {
                        //if((pNext.X==341 && pNext.Y==37 && point1.X==344 && point1.Y==40) || (point1.X==341 && point1.Y==37 && pNext.X==344 && pNext.Y==40))
                        //    Console.WriteLine("asdf");

                        double tempAngle = Util.Math.GetAngle(pPrev, pNow, point1);

                        if(Math.Abs(tempAngle-maximumAngle)<.000001) {
                            double distCurr = Util.Math.GetDistance(pNow, pNext); //Distance between current candidate and origin.
                            double distNew = Util.Math.GetDistance(pNow, point1); //Distance between potential point and origin.
                            if(distNew > distCurr)
                                pNext = point1;
                        } else if(tempAngle > maximumAngle) {
                            maximumAngle = tempAngle;
                            pNext = point1;
                        }
                    }
                    if(!point2.Equals(pNow)) {
                        double tempAngle = Util.Math.GetAngle(pPrev, pNow, point2);

                        if(Math.Abs(tempAngle-maximumAngle)<.000001) {
                            double distCurr = Util.Math.GetDistance(pNow, pNext); //Distance between current candidate and origin.
                            double distNew = Util.Math.GetDistance(pNow, point2); //Distance between potential point and origin.
                            if(distNew > distCurr)
                                pNext = point2;

                        } else if(tempAngle > maximumAngle) {
                            maximumAngle = tempAngle;
                            pNext = point2;
                        }
                    }
                }

                pPrev = pNow;
                pNow = pNext;

                if(!pNow.Equals(bottomMost) && chull.Contains(pNow)) {
                    Console.WriteLine("derp!");
                }
            } while(!pNow.Equals(bottomMost));
            //} while(!chull.Contains(pNow));

            //if(!pNow.Equals(bottomMost)) {
            //    foreach(Point point in chull)
            //        Console.WriteLine("XY: " + point.X + " " + point.Y);
            //}

            return chull;
        }

        /// <summary>
        /// Finds the bottom most point in all the line segments.
        /// </summary>
        /// <returns>The bottom most point.</returns>
        private Point FindBottomMostPoint() {
            Point bottomMost = new Point(VisionConstants.Camera.IMAGE_WIDTH, VisionConstants.Camera.IMAGE_HEIGHT);
            foreach(LineSegment lineSeg in lineSegments) {
                if(bottomMost.Y > lineSeg.Y || (bottomMost.Y == lineSeg.Y && bottomMost.X > lineSeg.leftX)) {
                    bottomMost = new Point(lineSeg.leftX, lineSeg.Y);
                }
            }
            return bottomMost;
        }

        /// <summary>
        /// Calculates how many points are currently in the list of Line Segments
        /// </summary>
        /// <returns>The point count.</returns>
        public int CalculatePointCount() {
            int count = 0;
            foreach(LineSegment lineSeg in lineSegments) {
                count += lineSeg.rightX - lineSeg.leftX + 1; //The length of the segment plus the initial point.
            }
            return count;
        }

        /// <summary>
        /// Adds a line segment to the blob.
        /// </summary>
        /// <param name="ls">The line segment to add.</param>
        public void AddLineSegment(LineSegment ls) {
            lineSegments.Add(ls);
        }

        /* Bounding Boxes
         * The code initially assumes the bounding box to be the entire image. It then attempts to narrow down the 
         * box by testing it against the line segments of the blob to see if their max x-values (highX) are greater than
         * the current maximum x-value of the box, if their min x-values (lowX) are smaller than than the current minimum
         * x-value, or if the y-value is either less than the minimum y-value or greater than the maximum y-value.  The 
         * Rectangle returned reflects the closest fit box around the blob with initial position (minX, minY), 
         * length (maxX - minX) and width (maxY - minY).
         */
        public Rectangle CalculateBoundingBox() {
            int minY = VisionConstants.Camera.IMAGE_HEIGHT;
            int maxY = 0;
            int minX = VisionConstants.Camera.IMAGE_WIDTH;
            int maxX = 0;									//Box initialized as entire image
            int lowX;
            int highX;
            int y;
            foreach(LineSegment ls in lineSegments) {
                lowX = ls.leftX;
                highX = ls.rightX;
                y = ls.Y;
                if(lowX < minX)  	//If the segment's endpoint is farther to the left, move the minimum x-value.
                    minX = lowX;
                if(highX > maxX) 	//If the segment's endpoint is farther to the right, move the maximum x-value.
                    maxX = highX;
                if(y < minY)
                    minY = y;
                if(y > maxY)		//If the segment's y-position is either greater or less than the current 
                    maxY = y;		//dimensions restrict the box further.
            }
            return new Rectangle(minX, minY, maxX-minX, maxY-minY);
        }

        /// <summary>
        /// Gets the current line segment list for the blob.
        /// </summary>
        /// <returns>The line segments of the blob.</returns>
        public List<LineSegment> GetLineSegments() {
            return lineSegments;
        }

        
        /// <summary>
        /// Finds the corner points of a blob
        /// </summary>
        /// <returns>A list of the corner points.</returns>
        /// <param name="ConvexHull">Convex hull.</param>
        public List<Point> FindCorners() {
            List<Point> potentialCorners = new List<Point>();
            List<Point> actualCorners = new List<Point>();
            Point pPrev = ConvexHull[ConvexHull.Count - 1];
            Point pCurr = ConvexHull[0];
            Point pNext = ConvexHull[1];
            //Loop through the convex hull to look for potential corners.
            for(int i = 0; i < ConvexHull.Count; i++) {
                //If the angle between points is smaller than the threshold, mark it as a candidate.
                if(Util.Math.GetAngle(pPrev, pCurr, pNext) <= CORNER_ANGLE_THRESHOLD)
                    potentialCorners.Add(pCurr);
                pPrev = pCurr;
                pCurr = pNext;
                pNext = ConvexHull[(i + 2) % ConvexHull.Count]; // (% chull.Count) allows pNext to wrap around to the start of the list.
            }

			if (potentialCorners.Count <= 4) {
				return potentialCorners;
			}
            //Repeat the vetting process above with the candidates in potentialCorners.
            pPrev = potentialCorners[potentialCorners.Count - 1];
            pCurr = potentialCorners[0];
            pNext = potentialCorners[1];
            for(int i = 0; i < potentialCorners.Count; i++) {
                if(Util.Math.GetAngle(pPrev, pCurr, pNext) <= CORNER_ANGLE_THRESHOLD)
                    actualCorners.Add(pCurr);
                pPrev = pCurr;
                pCurr = pNext;
                pNext = potentialCorners[(i + 2) % potentialCorners.Count];
            }
			if (actualCorners.Count <= 4) {
				return actualCorners;
			}
            pPrev = actualCorners[actualCorners.Count - 1];
            pCurr = actualCorners[0];
            pNext = actualCorners[1];
            bool conflictsCleared = false;
            //Loop until all conflicts, i.e. several points part of the same corner, are resolved.
            while(!conflictsCleared) {
                //Console.WriteLine("findCorners");
                conflictsCleared = true;
                for(int i = 0; i < actualCorners.Count; i++) {
                    if(Util.Math.GetDistance(pCurr, pNext) < CORNER_DISTANCE_THRESHOLD) {
                        //Compare the angle between the previous point, the current point, and the point
                        //after the next point with the previous point, the next point, and the point after
                        //the next point to see which one is closer to a right angle (pi / 2 radians) and
                        //therefore more suited as the corner point. Then it removes the other point.
                        if(Util.Math.GetAngle(pPrev, pCurr, actualCorners[(i + 2) % actualCorners.Count]) - (System.Math.PI / 2) < 
								Util.Math.GetAngle(pPrev, pNext, actualCorners[(i + 2) % actualCorners.Count]) - (System.Math.PI / 2)) {
                            actualCorners.Remove(pNext);
                        } else {
                            actualCorners.Remove(pCurr);
                        }
                        conflictsCleared = false;
                    }
                    //As the code occasionally removes either pCurr or pNext, it is no longer possible
                    //to set pPrev to pCurr and pCurr to pNext as has done previously. Instead, future
                    //values of points are set relative to their intended positions in the list.
                    if(actualCorners.Count != 0) {
                        pPrev = actualCorners[i % actualCorners.Count];
                        pCurr = actualCorners[(i + 1) % actualCorners.Count];
                        pNext = actualCorners[(i + 2) % actualCorners.Count];
                    }
                }
            }
            return actualCorners;
        }

    }


}
