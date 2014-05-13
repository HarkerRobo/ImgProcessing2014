using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing2014
{
    unsafe class PixelTester
    {
        /// <summary>
        /// tests if the given state should be flood filled
        /// </summary>
        /// <param name="state">the pixel to test</param>
        /// <returns></returns>
        public delegate bool PixelTest(byte state);

        public static bool TestGoalPixel(byte state)
        {
            return state == (byte)PixelState.Seed || state == (byte)PixelState.Candidate;
        }

        public static bool TestNullPixel(byte state)
        {
            return state == (byte)PixelState.None;
        }

        public static bool TestRedPixel(byte state)
        {
            return state == (byte)PixelState.Red;
        }

        public static bool TestBluePixel(byte state)
        {
            return state == (byte)PixelState.Blue;
        }
    }
}
