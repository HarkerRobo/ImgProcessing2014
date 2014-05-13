using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace ImageProcessing2014 {
    class HSLColor {
        /// <summary>
        /// Our internal hue value
        /// </summary>
        private double m_hue;

        /// <summary>
        /// Our internal saturation value
        /// </summary>
        private double m_saturation;

        /// <summary>
        /// Our internal luminosity value.
        /// </summary>
        private double m_luminosity;

        /// <summary>
        /// The Hue component of the HSL color in range [0, 360.0)
        /// </summary>
        public double Hue { get { return m_hue; } set { m_hue = NormalizeHue(value); } }

        /// <summary>
        /// The Saturation component of the HSL color
        /// </summary>
        public double Saturation { get { return m_saturation; } set { m_saturation = ForceBounds(value, 0.0, 1.0); } }

        /// <summary>
        /// The Value component of the HSL color
        /// </summary>
        public double Luminosity { get { return m_luminosity; } set { m_luminosity = ForceBounds(value, 0.0, 1.0); } }

        /// <summary>
        /// Creates a new HSLColor instance with H = 0.0, S = 0.0, L = 0.0.
        /// </summary>
        public HSLColor() {
        }

        /// <summary>
        /// Creates a new HSLColor instance with the given HSL values.
        /// </summary>
        /// <param name="hue">Hue in degrees from 0.0 to 360.0</param>
        /// <param name="saturation">Saturation from 0.0 to 1.0</param>
        /// <param name="luminosity">Value from 0.0 to 1.0</param>
        public HSLColor(double hue, double saturation, double luminosity) {
            Debug.Assert(0.0 <= saturation && saturation <= 1.0);
            Debug.Assert(0.0 <= luminosity && luminosity <= 1.0);

            m_hue = NormalizeHue(hue);
            m_saturation = ForceBounds(saturation, 0.0, 1.0);
            m_luminosity = ForceBounds(luminosity, 0.0, 1.0);
        }

        /// <summary>
        /// Gets a representation of this HSL Color as a C# Color object.
        /// This is an expensive operation that involves conversion from HSL color space to RGB space.
        /// No caching is used for this operation, so consider caching it on your own if you will call
        /// this on the same method repeatedly. 
        /// 
        /// Figure 1: http://en.wikipedia.org/wiki/File:HSV-RGB-comparison.svg
        /// </summary>
        /// <returns></returns>
        public Color ToColor() {
            Debug.Assert(!Double.IsNaN(m_hue));

            double chroma = (255.0 - Math.Abs(2.0 * m_luminosity * 255.0 - 255.0)) * m_saturation;
            double hPrime = m_hue / 60.0; //Squish hue into the 6 sections of HSL-RGB conversion (see RGB-Hue graph)
            double x = chroma * (1.0 - Math.Abs((hPrime % 2.0) - 1.0));
            double[] rgbTemp;
            if(hPrime < 1.0)
                rgbTemp = new[] { chroma, x, 0 };
            else if(hPrime < 2.0)
                rgbTemp = new[] { x, chroma, 0 };
            else if(hPrime < 3.0)
                rgbTemp = new[] { 0, chroma, x };
            else if(hPrime < 4.0)
                rgbTemp = new[] { 0, x, chroma };
            else if(hPrime < 5.0)
                rgbTemp = new[] { x, 0, chroma };
            else if(hPrime < 6.0)
                rgbTemp = new[] { chroma, 0, x };
            else
                throw new Exception("hPrime wasn't within [0, 6)??? hPrime = " + hPrime);
            double m = m_luminosity * 255.0 - 0.5 * chroma;

            return Color.FromArgb(
                (int)ForceBounds(rgbTemp[0] + m, 0, 255),
                (int)ForceBounds(rgbTemp[1] + m, 0, 255),
                (int)ForceBounds(rgbTemp[2] + m, 0, 255)
            );
        }


        /// <summary>
        /// Normalizes a hue value so that it is within the [0, 360] range.
        /// </summary>
        /// <param name="hue">The input hue value which we are normalizing</param>
        /// <returns>The normalized hue value</returns>
        public static double NormalizeHue(double hue) {
            //----------------------------------------------------------------------------------------
            // Three cases exist.  Hue is in our [0, 360] range, below it, or above it. 
            //----------------------------------------------------------------------------------------
            if(hue >= 0.0 && hue <= 360.0) {
                return hue;
            } else if(hue < 0) {
                //-------------------------------------------------------------------------------------
                // If hue is less than 0, we want to normalize it to the range [0.0, 360.0]
                // Let's assume a number -350.  We want to normalize that to 10.
                //    1. We flip the number's sign to 350 and divide it by 360 yielding 'a' = 0.9722222
                //    2. We ceil the resultant value to yield the integer (double) 'b' = 1.
                //    3. We add b * 360.0 to the input value.
                // That can be summarized as:
                //    result = hue + 360.0 * Ciel(-hue/360.0).
                //-------------------------------------------------------------------------------------
                return hue + 360.0 * (double)Math.Ceiling(-hue / 360.0);
            } else //Hue is greater than 360.0. {
                //-------------------------------------------------------------------------------------
                // If hue is greater than 360, we want to normalize it to the range [0.0, 360.0]
                // Let's assume a number 370.  We want to normalize that to 10. To do this, we simply
                // have to use mod arithmatic.  
                //-------------------------------------------------------------------------------------
                return hue % 360.0;
        }

        /// <summary>
        /// Forces a number into the given bounds (low/high inclusive).  
        /// If the number is below the low value, the low value is returned.
        /// If the number is above the high value, the high value is returned.
        /// Otherwise, the original value is returned.
        /// </summary>
        /// <param name="value">The number which we are forcing into the given bounds</param>
        /// <param name="low">The lower inclusive bounds</param>
        /// <param name="high">The upper inclusive bounds</param>
        /// <returns>A value within the given range.</returns>
        public static double ForceBounds(double value, double low, double high) {
            if(value < low) return low;
            if(value > high) return high;
            return value;
        }

        /// <summary>
        /// Creates a new HSL Color from the given RGB values
        /// </summary>
        /// <param name="r">The color component in range [0, 255]</param>
        /// <param name="g">The color component in range [0, 255]</param>
        /// <param name="b">The color component in range [0, 255]</param>
        /// <returns>The color represented in HSL</returns>
        public static HSLColor FromRGB(int r, int g, int b) {
            double rPrime = (double)r / 255.0;
            double gPrime = (double)g / 255.0;
            double bPrime = (double)b / 255.0;
            double[] components = new[] { rPrime, gPrime, bPrime };
            double maxComponent = components.Max();
            double minComponent = components.Min();

            double luminosity = 0.5 * (maxComponent + minComponent);
            double chroma = maxComponent - minComponent;
            double saturation;
            if(Math.Abs(luminosity - 1.0) <= double.Epsilon || Math.Abs(luminosity - 0.0) <= double.Epsilon)
                saturation = 0.0; //In the "white" or "black" situation, saturation is undefined.
            else
                saturation = chroma / (1 - Math.Abs(2 * luminosity - 1));

            double huePrime;
            if(Math.Abs(chroma - 0.0) <= double.Epsilon)
                huePrime = 0.0; //This is the case where we have pure grayscale, so technically there is an absence of color.
            else if(Math.Abs(maxComponent - rPrime) <= double.Epsilon)
                huePrime = (((gPrime - bPrime) / chroma) + 6.0) % 6.0; //This can go negative, thusly the +6.0
            else if(Math.Abs(maxComponent - gPrime) <= double.Epsilon)
                huePrime = ((bPrime - rPrime) / chroma) + 2.0;
            else if(Math.Abs(maxComponent - bPrime) <= double.Epsilon)
                huePrime = ((rPrime - gPrime) / chroma) + 4.0;
            else
                throw new Exception("Impossible case: Max component was not r, g, or b?");
            return new HSLColor(
                ForceBounds(huePrime * 60.0, 0.0, 360.0),
                ForceBounds(saturation, 0.0, 1.0),
                ForceBounds(luminosity, 0.0, 1.0)
            );
        }

        /// <summary>
        /// Prints a friendly string representation of our hsl color
        /// </summary>
        /// <returns>
        /// Friendly string representation of our hsl color
        /// </returns>
        public override string ToString() {
            return "[Object HSLColor { Hue = " + Hue + ", Saturation = " + Saturation + ", Luminosity = " + Luminosity + " }]";
        }

    }
}