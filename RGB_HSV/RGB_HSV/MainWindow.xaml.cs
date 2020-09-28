using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Label = System.Windows.Controls.Label;

namespace RGB_HSV
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap bitmap;
        private double white_x = 95.04;
        private double white_y = 100.0;
        private double white_z = 108.88;
        private ViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel = new ViewModel();
        }

        private struct XYZ
        {
            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }
        }

        private struct HSV
        {
            public double h { get; set; }
            public double s { get; set; }
            public double v { get; set; }
        }

        private Dictionary<int, int> getDictionary(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Color pixel;
            var lValues = new Dictionary<int, int>();
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    pixel = bitmap.GetPixel(j, i);
                    byte red = pixel.R;
                    byte green = pixel.G;
                    byte blue = pixel.B;
                    var xyz = getXYZ(red, green, blue);
                    int l = (int)Math.Floor(116 * func(xyz.y / white_y) - 16);
                    lValues[l] = lValues.TryGetValue(l, out int val) ? val + 1 : 1;
                }
            }
            return lValues;
        }

        private async void showBarChart()
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            Bitmap image = bitmap.Clone() as Bitmap;
            var lValues =  getDictionary(image);
            var max = lValues.Max(kv => kv.Value);
            int lValuesCount = lValues.Count;

            Bitmap extandBarChart = new Bitmap(200, 100);
            Color color = new Color();
            color = Color.FromArgb(255, 255, 255);


            for (int i = 0; i < 200; ++i)
            {
                for (int j = 0; j < 100; ++j)
                {
                    extandBarChart.SetPixel(i, j, color);
                }
            }

            color = Color.FromArgb(0, 0, 0);

            for (int i = 0; i < 200; i += 2)
            {
                if (lValues.ContainsKey(i / 2))
                {
                    int currentHeight = (int)Math.Floor(1.0 * lValues[i / 2] / max * 100);
                    for (int j = 100 - currentHeight; j < 100; ++j)
                    {
                        extandBarChart.SetPixel(i, j, color);
                        extandBarChart.SetPixel(i + 1, j, color);
                    }
                }
            }
            using (MemoryStream memory = new MemoryStream())
            {
                extandBarChart.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(memory.ToArray());
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                barChart.Source = bitmapImage;
                barChart.Visibility = Visibility.Visible;
            }
        }

        private void Load_Image_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string fileName = openFileDialog.FileName;
                bitmap = new Bitmap(fileName);
                mainImage.Source = new BitmapImage(new Uri(fileName));
                showBarChart();
            }
        }

        private HSV getHSV(Color color)
        {
            HSV hsv = new HSV();
            byte red = color.R, green = color.G, blue = color.B;
            double norm_red = (red / 255.0);
            double norm_green = (green / 255.0);
            double norm_blue = (blue / 255.0);
            double max = Math.Max(Math.Max(norm_red, norm_green), norm_blue);
            double min = Math.Min(Math.Min(norm_red, norm_green), norm_blue);

            double hue = 0.0;
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
            double saturation = (max == 0) ? 0 : (1 - (1.0 * min / max));
            double value = max;
            hsv.h = hue;
            hsv.s = saturation * 100;
            hsv.v = value * 100;
            return hsv;
        }


        private XYZ getXYZ(byte red, byte green, byte blue)
        {
            XYZ xyz = new XYZ();
            var matrix = new List<double>
            { 0.4124564, 0.3575761, 0.1804375, 0.2126729, 0.7151522, 0.0721750, 0.0193339, 0.1191920, 0.9503041 };
            double normalize_red = Math.Pow((red / 255.0 + 0.055)/1.055, 2.4) * 100;
            double normalize_green = Math.Pow((green / 255.0 + 0.055) / 1.055, 2.4) * 100;
            double normalize_blue = Math.Pow((blue / 255.0 + 0.055) / 1.055, 2.4) * 100; 
            xyz.x = matrix[0] * normalize_red + matrix[1] * normalize_green 
                + matrix[2] * normalize_blue;
            xyz.y = matrix[3] * normalize_red + matrix[4] * normalize_green
                + matrix[5] * normalize_blue;
            xyz.z = matrix[6] * normalize_red + matrix[7] * normalize_green
                + matrix[8] * normalize_blue;
            return xyz;
        }

        private double func(double x)
        {
            double firstBranch = Math.Pow(x, 1.0 / 3);
            double secondBranch = Math.Pow(29.0 / 6, 2) * x * 1.0 / 3 + 4.0 / 29;
            return (x > Math.Pow((6.0 / 29), 3) ? firstBranch : secondBranch);
        }

        private void mainImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var point = e.GetPosition(this);
            if (point.X >= 0 && point.Y >= 0 && point.X <= bitmap.Width && point.Y <= bitmap.Height)
            {
                Color color = bitmap.GetPixel(Convert.ToInt32(point.X), Convert.ToInt32(point.Y));
                byte red = color.R, green = color.G, blue = color.B;
                HSV hsv = getHSV(color);
                XYZ xyz = getXYZ(red, green, blue);
                double L = 116 * func(xyz.y/white_y) - 16;
                double a = 500 * (func(xyz.x / white_x) - func(xyz.y / white_y));
                double b = 200 * (func(xyz.y / white_y) - func(xyz.z / white_z));
                hsvText.Content = "R: " + color.R + " G: " + color.G + " B: " + color.B
                    + "\nH: " + hsv.h + "\nS: " + hsv.s
                    + "\nV: " + hsv.v
                    + "\nL: " + L + "\na: " + a + "\nb: " + b;
            }
            hsvText.Visibility = Visibility.Visible;
        }

        private Color convertHSVToRGB(HSV hsv)
        {
            double h = hsv.h, s = hsv.s, v = hsv.v;
            Color color = new Color();
            int h_i = (int)Math.Floor(h / 60.0) % 6;
            double v_min = (100 - s) * v / 100.0;
            double a = (v - v_min) * (h % 60) / 60;
            double v_inc = (v_min + a);
            double v_dec = (v - a);
            switch (h_i)
            {
                case 0:
                    {
                        color = Color.FromArgb((int)v, (int)v_inc, (int)v_min);
                        break;
                    }
                case 1:
                    {
                        color = Color.FromArgb((int)v_dec, (int)v, (int)v_min);
                        break;
                    }
                case 2:
                    {
                        color = Color.FromArgb((int)v_min, (int)v, (int)v_inc);
                        break;
                    }
                case 3:
                    {
                        color = Color.FromArgb((int)v_min, (int)v_dec, (int)v);
                        break;
                    }
                case 4:
                    {
                        color = Color.FromArgb((int)v_inc, (int)v_min, (int)v);
                        break;
                    }
                case 5:
                    {
                        color = Color.FromArgb((int)v, (int)v_min, (int)v_dec);
                        break;
                    }
            }
            double r = color.R;
            double g = color.G;
            double b = color.B;
            color = Color.FromArgb((int)(r * 255 / 100.0), (int)(g*255 / 100.0), (int)(b * 255 / 100.0));
            return color;
        }

        private void ChangeH(object sender, TextChangedEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;
            var value = 0.0;
            Double.TryParse(textBox.Text, out value);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap trueBitmap = (Bitmap)bitmap.Clone();
            for(int i = 0; i < width; ++i)
            {
                for(int j = 0; j < height; ++j)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    HSV hsv = getHSV(pixel);
                    hsv.h = hsv.h + value;
                    if(hsv.h > 360 || hsv.h < 0)
                    {
                        hsv.h -= value;
                        continue;
                    }
                    Color color = convertHSVToRGB(hsv);
                    bitmap.SetPixel(i, j, color);
                }
            }
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Jpeg);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(memory.ToArray());
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                mainImage.Source = bitmapImage;
                mainImage.Visibility = Visibility.Visible;
            }
            showBarChart();
            bitmap = trueBitmap;
        }

        private void ChangeS(object sender, TextChangedEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;
            var value = 0.0;
            Double.TryParse(textBox.Text, out value);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap trueBitmap = (Bitmap)bitmap.Clone();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    HSV hsv = getHSV(pixel);
                    hsv.s = hsv.s + value;
                    if (hsv.s > 100 || hsv.s < 0)
                    {
                        hsv.s -= value;
                        continue;
                    }
                    Color color = convertHSVToRGB(hsv);
                    bitmap.SetPixel(i, j, color);
                }
            }
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Jpeg);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(memory.ToArray());
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                mainImage.Source = bitmapImage;
                mainImage.Visibility = Visibility.Visible;
            }
            if (bitmap.Equals(trueBitmap))
            {
                return;
            }
            bitmap = trueBitmap;
        }

        private void ChangeV(object sender, TextChangedEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;
            var value = 0.0;
            Double.TryParse(textBox.Text, out value);
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap trueBitmap = (Bitmap)bitmap.Clone();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    HSV hsv = getHSV(pixel);
                    hsv.v = hsv.v + value;
                    if (hsv.v > 100 || hsv.v < 0)
                    {
                        hsv.h -= value;
                        continue;
                    }
                    Color color = convertHSVToRGB(hsv);
                    bitmap.SetPixel(i, j, color);
                }
            }
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Jpeg);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(memory.ToArray());
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                mainImage.Source = bitmapImage;
                mainImage.Visibility = Visibility.Visible;
            }
            if (bitmap.Equals(trueBitmap))
            {
                return;
            }
            bitmap = trueBitmap;
        }
    }
}
