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

        public Bitmap ApplyMethod(Bitmap srcImage)
        {
            var width = srcImage.Width;
            var height = srcImage.Height;
            var kMeans = new KMeans();
            var slic = kMeans.ApplyMethod(srcImage);
            return null;
        }
    }
}
