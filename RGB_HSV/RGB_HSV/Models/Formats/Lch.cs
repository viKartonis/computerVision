using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB_HSV.Models.Formats
{
    class LCH
    {
        public double L { get; set; }
        public double H { get; set; }
        public double C { get; set; }

        public static Lab LCHToLab(LCH lch)
        {
            return new Lab
            {
                L = lch.L,
                A = lch.C * Math.Cos(lch.H),
                B = lch.C * Math.Sin(lch.H)
            };
        }

        public static LCH[,] RGBToLch(Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;
            var resultXYZ = new XYZ[width, height];
            for (var i = 0; i < width; ++i)
            {
                for (var j = 0; j < height; ++j)
                {
                    resultXYZ[i, j] =  XYZ.XyzFromColor(image.GetPixel(i, j));
                }
            }
            var resultLab = new Lab[width, height];
            for (var i = 0; i < width; ++i)
            {
                for (var j = 0; j < height; ++j)
                {
                    resultLab[i, j] = Lab.LabFromXYZ(resultXYZ[i, j]);
                }
            }
            var resultLhc = new LCH[width, height];
            for (var i = 0; i < width; ++i)
            {
                for (var j = 0; j < height; ++j)
                {
                    resultLhc[i, j] = new LCH
                    { 
                        L = resultLab[i, j].L, 
                        C = Math.Sqrt(Math.Pow(resultLab[i, j].A,2) + Math.Pow(resultLab[i, j].B,2)), 
                        H = resultLab[i, j].A/ resultLab[i, j].B >0 ? Math.Atan(resultLab[i, j].A / resultLab[i, j].B) :
                         Math.Atan(resultLab[i, j].A / resultLab[i, j].B) + 360
                    };
                }
            }
            return resultLhc;
        }
    }
}
