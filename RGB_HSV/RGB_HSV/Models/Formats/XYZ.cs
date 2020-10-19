using System;
using System.Collections.Generic;
using System.Drawing;

namespace RGB_HSV.Models
{
    public class XYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        private static readonly List<double> transitionMatrix = new List<double>
            { 0.4124564, 0.3575761, 0.1804375, 0.2126729, 0.7151522, 0.0721750, 0.0193339, 0.1191920, 0.9503041 };

        public static XYZ XyzFromColor(Color color)
        {
            XYZ xyz = new XYZ();
            byte red = color.R, green = color.G, blue = color.B;

            var normalize_red = Math.Pow((red / 255.0 + 0.055) / 1.055, 2.4) * 100;
            var normalize_green = Math.Pow((green / 255.0 + 0.055) / 1.055, 2.4) * 100;
            var normalize_blue = Math.Pow((blue / 255.0 + 0.055) / 1.055, 2.4) * 100;
            xyz.X = transitionMatrix[0] * normalize_red + transitionMatrix[1] * normalize_green
                + transitionMatrix[2] * normalize_blue;
            xyz.Y = transitionMatrix[3] * normalize_red + transitionMatrix[4] * normalize_green
                + transitionMatrix[5] * normalize_blue;
            xyz.Z = transitionMatrix[6] * normalize_red + transitionMatrix[7] * normalize_green
                + transitionMatrix[8] * normalize_blue;
            return xyz;
        }
    }
}
