using System;
using System.Drawing;

namespace RGB_HSV.Models
{
    public class HSV
    {
        public double H { get; set; }
        public double S { get; set; }
        public double V { get; set; }

        public Bitmap PreviousBitmap { get; set; }

        public static HSV HsvFromColor(Color color)
        {
            byte red = color.R, green = color.G, blue = color.B;
            var norm_red = (red / 255.0);
            var norm_green = (green / 255.0);
            var norm_blue = (blue / 255.0);
            var max = Math.Max(Math.Max(norm_red, norm_green), norm_blue);
            var min = Math.Min(Math.Min(norm_red, norm_green), norm_blue);

            var hue = 0.0;
            if (max == norm_red && norm_green >= norm_blue)
            {
                hue = 60.0 * (double)(norm_green - norm_blue) / (max - min);
            }
            else if (max == norm_red && norm_green < norm_blue)
            {
                hue = 60.0 * (double)(norm_green - norm_blue) / (max - min) + 360;
            }
            else if (max == norm_green)
            {
                hue = 60.0 * (double)(norm_blue - norm_red) / (max - min) + 120;
            }
            else if (max == norm_blue)
            {
                hue = 60.0 * (double)(norm_red - norm_green) / (max - min) + 240;
            }
            else if (max == min)
            {
                hue = -100.0;
            }
            var saturation = (max == 0) ? 0 : (1 - (1.0 * min / max));

            return new HSV { H = hue, S = saturation * 100, V = max * 100 };
        }

        public Bitmap changeH(Bitmap bitmap, string HValue)
        {
            if (!double.TryParse(HValue, out var value))
            {
                return null;
            }
            int width = bitmap.Width;
            int height = bitmap.Height;
            PreviousBitmap = (Bitmap)bitmap.Clone();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    HSV hsv = HSV.HsvFromColor(pixel);
                    hsv.H += value;
                    if (hsv.H > 360 || hsv.H < 0)
                    {
                        hsv.H -= value;
                        continue;
                    }
                    Color color = hsv.ToColor();
                    bitmap.SetPixel(i, j, color);
                }
            }
            return bitmap;
        }

        public Bitmap changeS(Bitmap bitmap, string SValue)
        {
            if (!double.TryParse(SValue, out var value))
            {
                return null;
            }
            int width = bitmap.Width;
            int height = bitmap.Height;
            PreviousBitmap = (Bitmap)bitmap.Clone();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    HSV hsv = HSV.HsvFromColor(pixel);
                    hsv.S += value;
                    if (hsv.S > 100 || hsv.S < 0)
                    {
                        hsv.S -= value;
                        continue;
                    }
                    Color color = hsv.ToColor();
                    bitmap.SetPixel(i, j, color);
                }
            }
            return bitmap;
        }

        public Bitmap changeV(Bitmap bitmap, string VValue)
        {
            if (!double.TryParse(VValue, out var value))
            {
                return null;
            }
            int width = bitmap.Width;
            int height = bitmap.Height;
            PreviousBitmap = (Bitmap)bitmap.Clone();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    HSV hsv = HSV.HsvFromColor(pixel);
                    hsv.V += value;
                    if (hsv.V > 100 || hsv.V < 0)
                    {
                        hsv.V -= value;
                        continue;
                    }
                    Color color = hsv.ToColor();
                    bitmap.SetPixel(i, j, color);
                }
            }
            return bitmap;
        }

            public Color ToColor()
        {
            var color = new Color();
            var h_i = (int)Math.Floor(H / 60.0) % 6;
            var v_min = (100 - S) * V / 100.0;
            var a = (V - v_min) * (H % 60) / 60;
            var v_inc = (v_min + a);
            var v_dec = (V - a);
            switch (h_i)
            {
                case 0:
                    {
                        color = Color.FromArgb((int)V, (int)v_inc, (int)v_min);
                        break;
                    }
                case 1:
                    {
                        color = Color.FromArgb((int)v_dec, (int)V, (int)v_min);
                        break;
                    }
                case 2:
                    {
                        color = Color.FromArgb((int)v_min, (int)V, (int)v_inc);
                        break;
                    }
                case 3:
                    {
                        color = Color.FromArgb((int)v_min, (int)v_dec, (int)V);
                        break;
                    }
                case 4:
                    {
                        color = Color.FromArgb((int)v_inc, (int)v_min, (int)V);
                        break;
                    }
                case 5:
                    {
                        color = Color.FromArgb((int)V, (int)v_min, (int)v_dec);
                        break;
                    }
            }
            var r = color.R;
            var g = color.G;
            var b = color.B;
            if(r>255)
            {
                r = 255;
            }
            else if(r < 0)
            {
                r = 0;
            }
            if (g > 255)
            {
                g = 255;
            }
            else if (g < 0)
            {
                g = 0;
            }
            if (b > 255)
            {
                b = 255;
            }
            else if (b < 0)
            {
                b = 0;
            }
            color = Color.FromArgb((int)(r * Byte.MaxValue / 100.0), 
                (int)(g * Byte.MaxValue / 100.0), (int)(b * Byte.MaxValue / 100.0));
            return color;
        }

    }


}
