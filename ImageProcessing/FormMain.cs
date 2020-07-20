using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageProcessing
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            pictureBoxOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            // диалог для выбора файла
            OpenFileDialog ofd = new OpenFileDialog();
            // фильтр форматов файлов
            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
            // если в диалоге была нажата кнопка ОК
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // загружаем изображение
                    pictureBoxOriginal.Image = new Bitmap(ofd.FileName);
                }
                catch // в случае ошибки выводим MessageBox
                {
                    MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (pictureBoxResult.Image != null) // если изображение в pictureBox2 имеется
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Сохранить картинку как...";
                sfd.OverwritePrompt = true; // показывать ли "Перезаписать файл" если пользователь указывает имя файла, который уже существует
                sfd.CheckPathExists = true; // отображает ли диалоговое окно предупреждение, если пользователь указывает путь, который не существует
                                            // фильтр форматов файлов
                sfd.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
                sfd.ShowHelp = true; // отображается ли кнопка Справка в диалоговом окне
                                     // если в диалоге была нажата кнопка ОК
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // сохраняем изображение
                        pictureBoxResult.Image.Save(sfd.FileName);
                    }
                    catch // в случае ошибки выводим MessageBox
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void buttonProcess_Click(object sender, EventArgs e)
        {
            //ToWhiteAndBlack(pictureBoxOriginal, pictureBoxResult);
            //SobelGradient(pictureBoxOriginal, pictureBoxOriginal.Width, pictureBoxOriginal.Height);
            //Gaus(pictureBoxOriginal, pictureBoxResult);
            Sobel(pictureBoxOriginal, pictureBoxResult);
        }

        private void Sobel(PictureBox original, PictureBox result)
        {
            Bitmap img = new Bitmap(original.Image);
            float[,] brightnesses = new float[img.Width, img.Height];

            //матрица яркостей
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    brightnesses[i, j] = img.GetPixel(i, j).GetBrightness();
                }
            }

            Bitmap res = new Bitmap(img);
            var gr = GetGrad(brightnesses);
            var n = Normalize(gr);
            for (int i = 0; i < res.Width; i++)
            {
                for (int j = 0; j < res.Height; j++)
                {
                    res.SetPixel(i, j, Color.FromArgb(n[i, j], n[i, j], n[i, j]));
                }
            }
            result.Image = res;
        }

        private int[,] Normalize(double[,] matrix)
        {
            int[,] result = new int[matrix.GetLength(0), matrix.GetLength(1)];
            var max = GetMax(matrix);
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    result[x, y] = Convert.ToInt32(matrix[x, y] / max * 254);
                }
            }

            return result;
        }

        /*private int[,] Normalize(float[,] matrix)
        {
            int[,] result = new int[matrix.GetLength(0), matrix.GetLength(1)];
            var max = GetMax(matrix);
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    result[x, y] = Convert.ToInt32(matrix[x, y] / max * 254);
                }
            }

            return result;
        }*/

        private static double GetMax(double[,] matrix)
        {
            double max = 0;
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    if (matrix[x, y] > max)
                        max = matrix[x, y];
                }
            }

            return max;
        }

        /*private static double GetMax(float[,] matrix)
        {
            double max = 0;
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    if (matrix[x, y] > max)
                        max = matrix[x, y];
                }
            }

            return max;
        }*/

        private double[,] GetGrad(float[,] image)
        {
            int k = 1; // 2*k+1 - рамер маски. для собеля = 1
            double[,] grad = new double[image.GetLength(0), image.GetLength(1)]; // градиент - результат
                                                                                 //бежим по каждому пикселю (за исключением крайних)
            for (int y = k; y < image.GetLength(0) - k; y++)
            {
                for (int x = k; x < image.GetLength(1) - k; x++)
                {
                    grad[y, x] = CalculateValue(image, y, x, k);
                }
            }

            return grad;
        }

        private double CalculateValue(float[,] image, int y, int x, int k)
        {
            int[,] maskDy = new[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } }; // маска
            int[,] maskDx = new[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } }; // маска 

            double dy = Convolution(image, maskDy, y, x, k); // свертка для оси y 
            double dx = Convolution(image, maskDx, y, x, k); // свертка для оси x 

            return Math.Sqrt(dy * dy + dx * dx);
        }

        //свертка
        private double Convolution(float[,] image, int[,] mask, int y0, int x0, int k)
        {
            double value = 0;
            // бежим в окрестности пикселя 
            for (int y = -k; y <= k; y++)
            {
                for (int x = -k; x <= k; x++)
                {
                    value += image[y0 + y, x0 + x] * mask[y + k, x + k];
                }
            }

            return value;
        }

        private void ToWhiteAndBlack(PictureBox original, PictureBox result)
        {
            if (original.Image != null) // если изображение в pictureBox1 имеется
            {
                // создаём Bitmap из изображения, находящегося в pictureBox1
                Bitmap input = new Bitmap(original.Image);
                // создаём Bitmap для черно-белого изображения
                Bitmap output = new Bitmap(input.Width, input.Height);
                // перебираем в циклах все пиксели исходного изображения
                for (int j = 0; j < input.Height; j++)
                    for (int i = 0; i < input.Width; i++)
                    {
                        // получаем (i, j) пиксель
                        UInt32 pixel = (UInt32)(input.GetPixel(i, j).ToArgb());
                        // получаем компоненты цветов пикселя
                        float R = (float)((pixel & 0x00FF0000) >> 16); // красный
                        float G = (float)((pixel & 0x0000FF00) >> 8); // зеленый
                        float B = (float)(pixel & 0x000000FF); // синий
                                                               // делаем цвет черно-белым (оттенки серого) - находим среднее арифметическое
                        R = G = B = (R + G + B) / 3.0f;
                        // собираем новый пиксель по частям (по каналам)
                        UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);
                        // добавляем его в Bitmap нового изображения
                        output.SetPixel(i, j, Color.FromArgb((int)newPixel));
                    }
                // выводим черно-белый Bitmap в pictureBox2
                result.Image = output;
            }
        }

        int gaussCoef(double sigma, double[] a/*, double* b0*/)
        {
            double sigma_inv_4;

            sigma_inv_4 = sigma * sigma; sigma_inv_4 = 1.0 / (sigma_inv_4 * sigma_inv_4);

            double coef_A = sigma_inv_4 * (sigma * (sigma * (sigma * 1.1442707 + 0.0130625) - 0.7500910) + 0.2546730);
            double coef_W = sigma_inv_4 * (sigma * (sigma * (sigma * 1.3642870 + 0.0088755) - 0.3255340) + 0.3016210);
            double coef_B = sigma_inv_4 * (sigma * (sigma * (sigma * 1.2397166 - 0.0001644) - 0.6363580) - 0.0536068);

            double z0_abs = Math.Exp(coef_A);

            double z0_real = z0_abs * Math.Cos(coef_W);
            double z0_im = z0_abs * Math.Sin(coef_W);
            double z2 = Math.Exp(coef_B);

            double z0_abs_2 = z0_abs * z0_abs;

            a[2] = 1.0 / (z2 * z0_abs_2);
            a[0] = (z0_abs_2 + 2 * z0_real * z2) * a[2];
            a[1] = -(2 * z0_real + z2) * a[2];

            //*b0 = 1.0 - a[0] - a[1] - a[2];

            return 0;
        }

        private void Gaus(PictureBox original, PictureBox result)
        {
            Bitmap input = new Bitmap(original.Image);

            Bitmap output = new Bitmap(input.Width, input.Height);
            int r = 1;
            var rs = Math.Ceiling(r * 2.57);     // significant radius
            for (var i = 0; i < input.Height; i++)
            {
                for (var j = 0; j < input.Width; j++)
                {
                    double val = 0;
                    double wsum = 0;
                    for (var iy = i - rs; iy < i + rs + 1; iy++)
                    {
                        for (var ix = j - rs; ix < j + rs + 1; ix++)
                        {
                            var x = Convert.ToInt32(Math.Min(input.Width - 1, Math.Max(0, ix)));
                            var y = Convert.ToInt32(Math.Min(input.Height - 1, Math.Max(0, iy)));
                            var dsq = (ix - j) * (ix - j) + (iy - i) * (iy - i);
                            double wght = Math.Exp(-dsq / (2 * r * r)) / (Math.PI * 2 * r * r);
                            try
                            {
                                val += input.GetPixel(y, x).ToArgb() * wght; //scl[y * input.Width + x] * wght;
                            }
                            catch { }
                            wsum += wght;
                        }
                    }

                    try
                    {
                        output.SetPixel(i, j, Color.FromArgb(Convert.ToInt32(Math.Round(val / wsum))));
                    }
                    catch { }
                    //tcl[i * input.Width + j] = Math.Round(val / wsum);
                }
            }

            result.Image = output;
        }
    }
}

