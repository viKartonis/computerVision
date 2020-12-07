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
        public double c { get; set; }

        public int[][] RGBToLch(Bitmap image)
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
            var resultLhc = new Lch[width, height];
            for (var i = 0; i < width; ++i)
            {
                for (var j = 0; j < height; ++j)
                {
                    resultLhc[i, j] = new Lch 
                    { 
                        L = resultLab[i, j].L, 
                        c = resultLab[i, j].A + resultLab[i, j].B, 
                        h = 1 
                    };
                }
            }

            return null;
        }
    }
}
