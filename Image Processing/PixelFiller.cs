using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing2014
{
    class PixelFiller
    {
        /// <summary>
        /// marks the given pixel as filled
        /// </summary>
        /// <param name="pState">the pixel to fill</param>
		public unsafe delegate void PixelFill(byte* pState);

		public unsafe static void FillPixel(byte* pState)
        {
            *pState = (byte)PixelState.Visited;
        }

		public unsafe static void FillNullPixel(byte* pState)
        {
            *pState = (byte)PixelState.NullVisited;
        }

		public unsafe static void FillColoredPixel(byte* pState)
        {
            *pState = (byte)PixelState.Visited;
        }
    }
}
