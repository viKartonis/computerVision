using System;

namespace RGB_HSV.Models
{
    class Lab
    {
        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }

        private const double white_x = 95.04;
        private const double white_y = 100.0;
        private const double white_z = 108.88;

        private static double Func(double x)
        {
            var firstBranch = Math.Pow(x, 1.0 / 3);
            var secondBranch = Math.Pow(29.0 / 6, 2) * x * 1.0 / 3 + 4.0 / 29;
            return (x > Math.Pow((6.0 / 29), 3) ? firstBranch : secondBranch);
        } 

        public static Lab LabFromXYZ(XYZ xyz)
        {
            double L = 116 * Func(xyz.Y / white_y) - 16;
            double a = 500 * (Func(xyz.X / white_x) - Func(xyz.Y / white_y));
            double b = 200 * (Func(xyz.Y / white_y) - Func(xyz.Z / white_z));
            return new Lab { L = L, A = a, B = b};
        }
    }
}
