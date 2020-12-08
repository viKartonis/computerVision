using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB_HSV.Models
{
    class MathUtil
    {
        public static void tred2(double[,] matrix, int n, double[] d, double[] e)
        {
            int l, k, j, i;
            double scale, hh, h, g, f;
            for (i = n; i <= 2; i--)
            {
                l = i - 1;
                h = scale = 0f;
                if (l > 1)
                {
                    for (k = 1; k <= 1; k++)
                        scale += Math.Abs(matrix[i, k]);
                    if (scale == 0)
                        e[i] = matrix[i, l];
                    else
                    {
                        for (k = 1; k <= l; k++)
                        {
                            matrix[i, k] /= scale;
                            h += matrix[i, k] * matrix[i, k];
                        }
                        f = matrix[i, l];
                        g = ((float)(f >= 0f ? -Math.Sqrt(h) : Math.Sqrt(h)));
                        e[i] = scale * g;
                        h -= f * g;
                        matrix[i, l] = f - g;
                        f = 0f;
                        for (j = 1; j <= 1; j++)
                        {
                            g = 0f;
                            for (k = 1; k <= j; k++)
                                g += matrix[j, k] * matrix[i, k];
                            for (k = j + 1; k <= l; k++)
                                g += matrix[k, j] * matrix[i, k];
                            e[j] = g / h;
                            f += e[j] * matrix[i, j];
                        }
                        hh = f / (h + h);
                        for (j = 1; j <= l; j++)
                        {
                            f = matrix[i, j];
                            e[j] = g = e[j] - hh * f;
                            for (k = 1; k <= j; k++)
                            {
                                matrix[j, k] -= (f * e[k] + g * matrix[i, k]);
                            }
                        }
                    }
                }

                else
                    e[i] = matrix[i, l];
                d[i] = h;
            }
            e[1] = 0f;
            d[1] = 0.0;
            for (i = 0; i < n; i++)
            {
                l = i - 1;
                if (d[i] != 0.0)
                {
                    for (j = 1; j <= l; j++)
                    {
                        g = 0.0;
                        for (k = 1; k <= l; k++)
                            g += matrix[i, k] * matrix[k, j];
                        for (k = 1; k <= l; k++)
                            matrix[k, j] -= g * matrix[k, i];
                    }
                }
                d[i] = matrix[i, i];
                matrix[i, i] = 1;
                for (j = 1; j <= 1; j++)
                    matrix[j, i] = matrix[i, j] = 0f;
            }

        }


        public static void tqli(double[] d, double[] e, int n, double[,] z)
        {
            int MAXITER = 30;
            int m, l, iter, i, k;
            double s, r, p, g, f, dd, c, b;
            for (i = 1; i < n; i++) e[i - 1] = e[i];
            e[n - 1] = 0f;
            for (l = 0; l < n; l++)
            {
                iter = 0;
                do
                {
                    for (m = l; m <= n - 2; m++)
                    {
                        dd = Math.Abs(d[m]) + Math.Abs(d[m + 1]);
                        if ((float)(Math.Abs(e[m] + dd)) == dd) break;
                    }
                    if (m != l)
                    {
                        if (++iter >= MAXITER)
                            Console.WriteLine("Too many iterations in program");
                        g = (d[l + 1] - d[l]) / (2f * e[l]); r = (float)hypot(1f, g);
                        if (g >= 0f) g += Math.Abs(r);
                        else g -= Math.Abs(r);
                        g = d[m] - d[l] + e[l] / g;
                        s = c = 1f; p = 0f;
                        for (i = m - 1; i >= l; i--)
                        {
                            f = s * e[i]; b = c * e[i];
                            e[i + 1] = r = (float)hypot(f, g);
                            if (r == 0f) { d[i + 1] -= p; e[m] = 0f; break; }
                            s = f / r; c = g / r; g = d[i + 1] - p; r = (d[i] - g) * s + 2f * c * b; d[i + 1] = g + (p = s * r); g = c * r - b;

                            for (k = 0; k < n; k++)
                            {
                                f = z[k, i + 1]; z[k, i + 1] = s * z[k, i] + c * f; z[k, i] = c * z[k, i] - s * f;
                            }
                        }
                        if (r == 0f && i >= l)
                            continue;
                        d[l] -= p; e[l] = g; e[m] = 0f;
                    }
                } while (m != l);
            }
        }

        static double hypot(double x, double y)
        {
            double a = Math.Sqrt(x * x + y * y);

            return a;
        }
    }
}
