using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB_HSV.Models
{
    class KMeans
    {
        public static void Segmentation(Dictionary<Point, HSV> hsvImage)
        {
            var redH = 360;
            var whiteH = 280;
            var blackH = 50;
            var blueH = 220;

            foreach (Point p1 in hsvImage.Keys)
            {
                var distRed = Math.Abs(hsvImage[p1].H * hsvImage[p1].H - redH * redH);
                var distWhite = Math.Abs(hsvImage[p1].H * hsvImage[p1].H - whiteH * whiteH);
                var distBlack = Math.Abs(hsvImage[p1].H * hsvImage[p1].H - blackH * blackH);
                var distBlue = Math.Abs(hsvImage[p1].H * hsvImage[p1].H - blueH * blueH);

                var min1 = Math.Min(distRed, distWhite);
                var min2 = Math.Min(distBlack, distBlue);
                var min3 = Math.Min(min1, min2);

                if(min3 == distRed)
                {
                    hsvImage[p1].H = redH;
                }
                else if(min3 == distWhite)
                {
                    hsvImage[p1].H = whiteH;
                }
                else if(min3 == distBlack)
                {
                    hsvImage[p1].H = blackH;
                }
                else if(min3 == blueH)
                {
                    hsvImage[p1].H = blueH;
                }
            }
        }
    }
}
