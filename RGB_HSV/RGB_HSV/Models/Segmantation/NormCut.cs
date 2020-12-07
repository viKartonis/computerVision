using RGB_HSV.Models.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB_HSV.Models.Segmantation
{
    class NormCut
    {
        private double Distance(LCH first, LCH second)
        {
            var lab1 = LCH.LCHToLab(first);
            var lab2 = LCH.LCHToLab(second);
            var deltaL = second.L - first.L;
            var lRoof = (first.L + second.L) / 2;
            var sl = 1 + 0.015 * Math.Pow(lRoof - 50, 2) / Math.Sqrt(20 + Math.Pow(lRoof - 50, 2));
            var added1 = Math.Pow(deltaL / sl, 2);            
            var deltaC = second.C - first.C;
            var cRoof = (first.C + second.C) / 2;
            var sc = 1 + 0.045 * cRoof;
            var added2 = Math.Pow(deltaC / sc, 2);
            var smallDeltaH = 0.0;
            var a1 = lab1.A + lab1.A * (1 - Math.Sqrt(Math.Pow(cRoof, 7)/ (Math.Pow(cRoof, 7) + Math.Pow(25, 7))));
            var a2 = lab2.A + lab2.A * (1 - Math.Sqrt(Math.Pow(cRoof, 7) / (Math.Pow(cRoof, 7) + Math.Pow(25, 7))));
            var c1 = Math.Sqrt(Math.Pow(a1, 2) + Math.Pow(a2, 2));
            var h1 = Math.Atan2(lab1.B, a1) % 360;
            var h2 = Math.Atan2(lab2.B, a2) % 360;
            if (Math.Abs(h1 - h2) <= 180)
            {
                smallDeltaH = h2 - h1;
            }
            else if(Math.Abs(h1 - h2) > 180 && h2 <= h1)
            {
                smallDeltaH = h2 - h1 + 360;
            }
            else if(Math.Abs(h1 - h2) > 180 && h2 > h1)
            {
                smallDeltaH = h2 - h1 - 360;
            }
            var deltaH = 2 * Math.Sqrt(first.C*second.C) *Math.Sin(smallDeltaH/2);
            var hRoof = 0.0;
            if (Math.Abs(h1 - h2) <= 180)
            {
                hRoof = (h2 + h1)/2;
            }
            else if (Math.Abs(h1 - h2) > 180 && h2 + h1 < 360)
            {
                hRoof = (h2 + h1 + 360)/2;
            }
            else if (Math.Abs(h1 - h2) > 180 && h2 > h1)
            {
                hRoof = h2 - h1 - 360;
            }
          //  var hRoof = (first.H + second.H) / 2;
            var sh = 0;
            var added3 = Math.Pow(deltaH/sh,2);
            return 0;
        }

       private double[][] MakeMatrixM(LCH[,] srcImage)
        {
            var height = srcImage.GetUpperBound(0) + 1;
            var width = srcImage.Length / height;
            var matrix = new double[width * height, width * height];
            for(var i = 0; i < height; ++i)
            {
                for(var j = 0; j < width; ++j)
                {
                    matrix[i * width + j, i * width + j] = -1;

                    if(i - 1 >= 0)
                    {
                        matrix[(i- 1) * width + j, i * width + j] = Distance(srcImage[i - 1, j], srcImage[i, j]);
                        matrix[i * width + j, (i - 1) * width + j] = matrix[(i - 1) * width + j, i * width + j];
                    }
                }
            }
            return null;
        }

        public Bitmap ApplyMethod(Bitmap srcImage)
        {
            var width = srcImage.Width;
            var height = srcImage.Height;
            var lhcImage = LCH.RGBToLch(srcImage);
            var W = MakeMatrixM(lhcImage);

            return null;
        }
    }
}
