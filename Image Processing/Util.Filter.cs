using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


/* FILE LOG:
 * 1/24/2013        Added InitializeUT (table) and FilterLuminosity (explained in the code)
 *                  Added FilterHue and went over/commented all code as well as increased readability
 * 
 * TODO's:
 *  HSLColor cannot be found
 *  PixelState and VisionConstants do not exist
 */

///NOTE: Our math class overrides their math class - use
///     System.Math.{xxx}     if you want to use a math function in C#'s libraries
///     Math.{xxx}            if you want to use a math function in our Libraries 

namespace ImageProcessing2014 {
    partial class Util {
        public const int MIN_SIZE = 20;
        public const int MAX_Y = 15;
        public const int MAX_X = 20;
        /// <summary>
        /// Filter is a class that contins utilities to filter and parse images. Filter includes the methods:
        ///     InitializeLookUpTable() - initializes the lookuptable for the next two (2) functions. ALWAYS initialize
        ///         the tables before calling the other methods - this is like calling encoder.start() before using encoders
        ///         
        ///     FilterLuminosity(byte* pScan0, byte* pStates, int imageStride, int imageWidth, int imageHeight, bool DEBUG) - 
        ///         filters the image (based on the given parameters) based on Luminosity (RGB to Light/Dark)
        ///         
        ///     FilterHue(byte* pScan0, int imageStride, byte* pStates, bool DEBUG) - filters the image (based on the 
        ///         given parameters) based on Hue/Stauration
        /// </summary>

        public unsafe class Filter {
            /*---------------------------------------INITIALIZATION---------------------------------------------------------*/

            //constants
			private const int CANDIDATE_THRESHOLD = 200;
			private const int SEED_THRESHOLD = 205;

            // an array of HSL float values for RBG ints (unsigned integers)
            private static double[] HSLTable = new double[(1 << 16) * 3];

            //Tables for Sin and Cosine
            public static double[] sinTable = new double[180];
            public static double[] cosTable = new double[180];

			public static void FilterGreenGoal(byte* pScan0, int imageStride, int imageWidth, int imageHeight, byte* pStates)
			{
				fixed(double* pHSLTable = HSLTable) {
					//-------------------------------------------------------------------------------------
					// Local Variable declarations
					// Many local variables are placed outside the loop so that no compiler attempts 
					// to zero their values between iterations (or even worse, stack push/pop)
					//-------------------------------------------------------------------------------------

					// Pointer inside of our bitmap data
					uint* pCurrentPixel = (uint*)pScan0;

					// Pointer inside of our pixel state data
					byte* pCurrentState = pStates;

					// The offset from the end of one scanline's data to the next
					var pLineEndLineStartOffset = imageStride - imageWidth * 4;

					// The hue of the pixel which we are testing
					double hue;

					// Index of our color inside of our RGB -> HSL lookup table
					uint lutIndex;

					// The value of the current pixel
					uint pixelValue;
					int xOffset;
					int yOffset;

					for(yOffset = 0; yOffset < imageHeight; yOffset++) {
						for(xOffset = 0; xOffset < imageWidth; xOffset++) {
							pixelValue = *pCurrentPixel;
							// Given:
							// A       R       G       B              
							// 76543210765432107654321076543210
							// to 
							//                 R     G   B
							//                 5432103210543210
							// 0 0000 || 4 0100 || 8 1000 || C 1100
							// 1 0001 || 5 0101 || 9 1001 || D 1101
							// 2 0010 || 6 0110 || A 1010 || E 1110
							// 3 0011 || 7 0100 || B 1011 || F 1100

							lutIndex = (((pixelValue >> 2) & 0x00003F) | // b
								((pixelValue >> 6) & 0x0003C0) | // g
								((pixelValue >> 8) & 0x00FC00)) * 3;

							// Hue is requested first, as the processor will precache the block of data 
							// following it.  Thusly, we cannot read hslTable later on, even if we definitely
							// won't use it soon. 

							//hue = pHSLTable[i + 2];
							//sat = pHSLTable[i + 1];
							//lum = pHSLTable[i + 0];

							// The lum filter is placed first, as a quick "get out" solution
							// We reference array-style to avoid writing to stack

							if(pHSLTable[lutIndex + 1] > (0.1f * Util.Drawing.MaxValueSnL) &&
								pHSLTable[lutIndex] > (0.2f * Util.Drawing.MaxValueSnL) && pHSLTable[lutIndex] < (0.9f * Util.Drawing.MaxValueSnL)) // The saturation ensures the thing feels really blue, as opposed to noisy
							{
								// The luminosity filter is not repeated in here, as high luminosity means low saturation.
								hue = pHSLTable[lutIndex + 2];
								if(hue < (Util.Drawing.MaxValueH * 170.0f / 360.0f)) {
									if(hue > (Util.Drawing.MaxValueH * 90.0f / 360.0f))
										*pCurrentState = (byte)PixelState.Seed;
									else
										*pCurrentState = (byte)PixelState.None;
								} else //Hue is > 160.0f
								{
									*pCurrentState = (byte)PixelState.None;
								}
							} else
								*pCurrentState = (byte)PixelState.None;
							#if DEBUG
							if(*pCurrentState == (byte)PixelState.Seed)
								*(uint*)pCurrentPixel = Util.Drawing.GREEN;
							else if(*pCurrentState == (byte)PixelState.Candidate)
								*(uint*)pCurrentPixel = Util.Drawing.GRAY;
							else
								*(uint*)pCurrentPixel = Util.Drawing.BLACK;
							#endif

							//increment pixel and state
							pCurrentPixel++;
							pCurrentState++;
						}

						//set the current pixel as the previous value + the offset (unsigned integer)
						pCurrentPixel = (uint*)(((byte*)pCurrentPixel) + pLineEndLineStartOffset);
					}
				}
			}
            // initializes the lookup table for RGB to HSL conversion
            public static void InitializeLookUpTable() {
				Int32 i = 0;
				for (Int32 r = 0; r < 64; r++)
					for (Int32 g = 0; g < 16; g++)
						for (Int32 b = 0; b < 64; b++) {
							var color = HSLColor.FromRGB (r << 2, g << 4, b << 2);
							;

							//initializes the table as {luminosity, saturation, and hue}
							HSLTable [i++] = color.Luminosity * Util.Drawing.MaxValueSnL;
							HSLTable [i++] = color.Saturation * Util.Drawing.MaxValueSnL;
							HSLTable [i++] = color.Hue / 360 * Util.Drawing.MaxValueH;
						}

				//create the sin and cos tables
				//                for (var s = 0; s < 180; s++)
				//                {
				//                    sinTable[s] = System.Math.Sin((s - 90) * System.Math.PI / 180);
				//                    sinTable[s + 90] = -sinTable[s];
				//					cosTable[s] = System.Math.Cos((s - 90) * System.Math.PI / 180);
				//                    cosTable[s + 90] = cosTable[s];
				//                }
			}


            /* -----------------------------------------IMAGE PROCESSING METHODS--------------------------------------------------
             * 
             * How the below code works (generically)
             * 
             * Let's assume that this is the picture to process, with the boxes being individual pixels
             * _____________________
             * |x|y|z|_|_|_|_|_|_|_|
             * |_|_|_|_|_|_|_|_|_|_|
             * |_|_|_|_|_|_|_|_|_|_|
             * |_|_|_|_|_|_|_|_|_|_|
             * |_|_|_|_|_|_|_|_|_|_|
             * 
             * Box x is pScan0 (the first pixel)
             * X may be one of three pStates (White, Grey, or Black). It's state is stored in an array of Light/Dark values
             *  for future processing. pStates is a pointer array
             *  
             * pCurrentPixel is a byte pointer that points to the current pixel 
             * In FilterLuminosity,
             *  imageStride is 10 
             *  imageHeight is 5 
             * 
             * xOffset and yOffset are the "offset" from pScan0 to the current pixel. So, the x and yOffset to pixel x
             * are both 0, and the xOffset to pixel y is 1 whereas the yOffset is 0. x and yOffset are used to get the pixel
             * to filter.
             * 
             * If DEBUG is true, this function will print the Light/Dark image. If false, nothing will be printed.
             * 
             *  - Manan
             * -------------------------------------------------------------------------------------------------------------------
             */

            ///FilterLuminosity is a method that filters an image based on luminosity and separates it into Light/Grey/Dark colors
            ///
            ///pscan0           the pointer to the first pixel in the image
            ///imageStride      the length of a row of pixels (in bytes) including padding
            ///pStates          pointer to the array of pixel states          
            ///imageWidth       the width of the desired image
            ///imageHeight      the height of the desired image
            ///
            ///Used for goals, which have retroreflective tape
            ///<returns> </returns>
            public static void FilterGoal(byte* pScan0, byte* pStates, int imageStride, int imageWidth, int imageHeight) {
                for(int yOffset = 0; yOffset < imageHeight; yOffset++) {
                    //The current pixel, stored as a pointer to a byte array 
                    byte* pCurrentPixel = pScan0 + yOffset * imageStride;

                    //pointer to a byte array, checks whether the pixel is NOT part of the goal, if it is a CANDIDATE, or if
                    //it is DEFINITELY in the goal
                    byte* pCurrentState = pStates + yOffset * imageWidth;

                    for(int xOffset = 0; xOffset < imageWidth; xOffset++) {
                        //r, g, and b values for the current pixel
                        //it's B, G, R, and A (not rgb) - ignore A value
                        int b = pCurrentPixel[0];
                        int g = pCurrentPixel[1];
                        int r = pCurrentPixel[2];

                        //Formula to calculate the luminosity for a point (take it on face)
						double lum = (System.Math.Max(System.Math.Max(b, g), r) + System.Math.Min(System.Math.Min(b, g), r))/1.6;

                        //Sets the current state array value to either White, Grey, or Black 
                        if(lum < CANDIDATE_THRESHOLD)
                            *pCurrentState = (byte)PixelState.None;
                        else if(lum < SEED_THRESHOLD)
                            *pCurrentState = (byte)PixelState.Candidate;
                        else
                            *pCurrentState = (byte)PixelState.Seed;

                        //For debugging, print the resultant image
#if DEBUG
                        if(lum < CANDIDATE_THRESHOLD)
                            *(uint*)pCurrentPixel = Util.Drawing.BLACK;
                        else if(lum < SEED_THRESHOLD)
                            *(uint*)pCurrentPixel = Util.Drawing.GRAY;
                        else
                            *(uint*)pCurrentPixel = Util.Drawing.WHITE;
#endif

                        //increments the pixel to the next 4 values (r, g, b, and a)
                        pCurrentPixel += 4;

                        //increments the current state
                        pCurrentState++;
                    }
                }
            }

            /// FilterBall filters the image by hue for object detection
            /// pScan0              pointer to the first pixel of the image
            /// imageStride         the length of a row of pixels (in bytes) including this padding
            /// imageWidth          image width
            /// imageHeight         image height
            /// pStates             pointer to the array of pixel states
            /// <returns> </returns>

            public static void FilterBall(byte* pImageScan0, int imageStride, int imageWidth, int imageHeight, byte* pStates) {
                fixed(double* pHSLTable = HSLTable) {
                    //-------------------------------------------------------------------------------------
                    // Local Variable declarations
                    // Many local variables are placed outside the loop so that no compiler attempts 
                    // to zero their values between iterations (or even worse, stack push/pop)
                    //-------------------------------------------------------------------------------------

                    // Pointer inside of our bitmap data
                    uint* pCurrentPixel = (uint*)pImageScan0;

                    // Pointer inside of our pixel state data
                    byte* pCurrentState = pStates;

                    // The offset from the end of one scanline's data to the next
                    var pLineEndLineStartOffset = imageStride - imageWidth * 4;

                    // The hue of the pixel which we are testing
                    double hue;

                    // Index of our color inside of our RGB -> HSL lookup table
                    uint lutIndex;

                    // The value of the current pixel
                    uint pixelValue;
                    int xOffset;
                    int yOffset;

                    for(yOffset = 0; yOffset < imageHeight; yOffset++) {
                        for(xOffset = 0; xOffset < imageWidth; xOffset++) {
                            pixelValue = *pCurrentPixel;
                            // Given:
                            // A       R       G       B              
                            // 76543210765432107654321076543210
                            // to 
                            //                 R     G   B
                            //                 5432103210543210
                            // 0 0000 || 4 0100 || 8 1000 || C 1100
                            // 1 0001 || 5 0101 || 9 1001 || D 1101
                            // 2 0010 || 6 0110 || A 1010 || E 1110
                            // 3 0011 || 7 0100 || B 1011 || F 1100

                            lutIndex = (((pixelValue >> 2) & 0x00003F) | // b
                                        ((pixelValue >> 6) & 0x0003C0) | // g
                                        ((pixelValue >> 8) & 0x00FC00)) * 3;

                            // Hue is requested first, as the processor will precache the block of data 
                            // following it.  Thusly, we cannot read hslTable later on, even if we definitely
                            // won't use it soon. 

                            //hue = pHSLTable[i + 2];
                            //sat = pHSLTable[i + 1];
                            //lum = pHSLTable[i + 0];

                            // The lum filter is placed first, as a quick "get out" solution
                            // We reference array-style to avoid writing to stack

                            if(pHSLTable[lutIndex + 1] > (0.2f * Util.Drawing.MaxValueSnL) &&
								pHSLTable[lutIndex] > (0.2f * Util.Drawing.MaxValueSnL) && pHSLTable[lutIndex] < (0.9f * Util.Drawing.MaxValueSnL)) // The saturation ensures the thing feels really blue, as opposed to noisy
                            {
                                // The luminosity filter is not repeated in here, as high luminosity means low saturation.
                                hue = pHSLTable[lutIndex + 2];
                                if(hue < (Util.Drawing.MaxValueH * 200.0f / 360.0f)) {
                                    if(hue < (Util.Drawing.MaxValueH * 15.0f / 360.0f))
                                        *pCurrentState = (byte)PixelState.Red;
                                    else
                                        *pCurrentState = (byte)PixelState.None;
                                } else //Hue is > 200.0f
                                {
                                    if(hue < (Util.Drawing.MaxValueH * 270.0f / 360.0f)) { //This has a larger interval, and thusly comes first
                                        *pCurrentState = (byte)PixelState.Blue;
                                    } else if(hue > (uint)(Util.Drawing.MaxValueH * 330.0f / 360.0f))
                                        *pCurrentState = (byte)PixelState.Red;
                                    else
                                        *pCurrentState = (byte)PixelState.None;
                                }
                            } else
                                *pCurrentState = (byte)PixelState.None;

#if DEBUG
                            if(*pCurrentState == (byte)PixelState.Red)
                                *(uint*)pCurrentPixel = Util.Drawing.RED;
                            else if(*pCurrentState == (byte)PixelState.Blue)
                                *(uint*)pCurrentPixel = Util.Drawing.BLUE;
                            else
                                *(uint*)pCurrentPixel = Util.Drawing.BLACK;
#endif

                            //increment pixel and state
                            pCurrentPixel++;
                            pCurrentState++;
                        }

                        //set the current pixel as the previous value + the offset (unsigned integer)
                        pCurrentPixel = (uint*)(((byte*)pCurrentPixel) + pLineEndLineStartOffset);
                    }
                }
            }

        }
    }
}