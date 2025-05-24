using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Point = System.Windows.Point;

namespace Filter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string imagePath;
        private BitmapImage bitmapImage;

        

    //double[,] kernel = new double[,]
    //    {
    //        { -1, -1,-1 },
    //        { -1,9,-1 },
    //        { -1,-1,-1 }

    //    };

    double[,] kernel = new double[,]
            {
                { -1, 0, 0,1 },
                { -2, 0, 0,2 },
                { -2, 0, 0,2 },
                {-1,0,0,1 }

            };

        bool zero = true;
        bool right = true;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == true)
            {
                imagePath = openFileDialog.FileName;
                LoadImage(sourceImage, imagePath);
                //filteredImage.Source = FilterImage(bitmapImage, kernel, 1,true,true);

            }
        }

        private void LoadImage(Image img,string path)
        {
            try
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(path);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                img.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
            }
        }

        private double[,] matrProduct(double[,] matr1, double[,] matr2)
        {
            double[,] result = new double[matr1.GetLength(0),matr2.GetLength(1)];

            for (int i = 0; i < matr1.GetLength(0); i++)
            {
                for (int j = 0; j < matr2.GetLength(0); j++)
                {
                    result[i, j] = matr1[i,j] * matr2[i,j];
                    
                }
            }
            return result;

        }

        private void matrMultNumber(ref double[,] matr1,double scalar)
        {
            for (int i = 0;i < matr1.GetLength(0); i++)
            {
                for(int j =0; j < matr1.GetLength(1); j++)
                {
                    matr1[i, j] *= scalar;
                }
            }
        }

        private double summarize(double[,] matr)
        {
            double sum = 0;
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                for (int j = 0; j < matr.GetLength(1); j++)
                {
                    sum += matr[i, j];
                }
            }

            return sum;
        }
        private WriteableBitmap FilterImage(BitmapImage img, double[,] matr, double k, bool zero, bool right)
        {
            if (img == null) return null;

            int width = img.PixelWidth;
            int height = img.PixelHeight;
            int stride = width * 4;
            byte[] originalPixels = new byte[height * stride];
            img.CopyPixels(originalPixels, stride, 0);

            int size = matr.GetLength(0);
            int baseOffset = size / 2;
            int additionalOffset = (size % 2 == 0) ? (right ? 0 : -1) : 0;
            int padding = baseOffset + additionalOffset;

            
            int extendedWidth = width + 2 * padding;
            int extendedHeight = height + 2 * padding;
            byte[] extendedPixels = new byte[extendedHeight * extendedWidth * 4];

            
            for (int y = 0; y < extendedHeight; y++)
            {
                for (int x = 0; x < extendedWidth; x++)
                {
                    int extIndex = y * (extendedWidth * 4) + x * 4;

                    int origX = Math.Clamp(x - padding, 0, width - 1);
                    int origY = Math.Clamp(y - padding, 0, height - 1);

                    // Копируем пиксель
                    int origIndex = origY * stride + origX * 4;
                    if (zero && (x < padding || y < padding ||
                                x >= extendedWidth - padding || y >= extendedHeight - padding))
                    {
                        extendedPixels[extIndex] = 0;     // B
                        extendedPixels[extIndex + 1] = 0; // G
                        extendedPixels[extIndex + 2] = 0; // R
                        extendedPixels[extIndex + 3] = 0; // A
                        continue;
                    }
                    extendedPixels[extIndex] = originalPixels[origIndex];     // B
                    extendedPixels[extIndex + 1] = originalPixels[origIndex + 1]; // G
                    extendedPixels[extIndex + 2] = originalPixels[origIndex + 2]; // R
                    extendedPixels[extIndex + 3] = originalPixels[origIndex + 3]; // A
                }
            }

            
            WriteableBitmap filtered = new WriteableBitmap(width, height, img.DpiX, img.DpiY, PixelFormats.Pbgra32, null);
            byte[] resultPixels = new byte[height * stride];

            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double r = 0, g = 0, b = 0;

                    
                    Point center = GetCenterPixel(x, y, size, right);

                   
                    for (int ky = 0; ky < size; ky++)
                    {
                        for (int kx = 0; kx < size; kx++)
                        {
                            int extX = (int)(center.X + kx - size / 2);
                            int extY = (int)(center.Y + ky - size / 2);
                            int extIndex = extY * (extendedWidth * 4) + extX * 4;

                           
                            if (extX < 0 || extY < 0 || extX >= extendedWidth || extY >= extendedHeight)
                            {
                                continue;
                            }

                            b += extendedPixels[extIndex] * matr[kx, ky];
                            g += extendedPixels[extIndex + 1] * matr[kx, ky];
                            r += extendedPixels[extIndex + 2] * matr[kx, ky];
                        }
                    }

                    int index = y * stride + x * 4;
                    resultPixels[index] = Clamp(b / k);     // B
                    resultPixels[index + 1] = Clamp(g / k); // G
                    resultPixels[index + 2] = Clamp(r / k); // R
                    resultPixels[index + 3] = originalPixels[index + 3]; // Альфа-канал без изменений
                }
            }

            
            filtered.WritePixels(new Int32Rect(0, 0, width, height), resultPixels, stride, 0);
            return filtered;
        }

        
        private Point GetCenterPixel(int x, int y, int size, bool right)
        {
            int offset = size / 2;
            if (size % 2 == 0 && !right)
            {
                offset--;
            }
            return new Point(x + offset, y + offset);
        }

        private byte Clamp(double value)
        {
            return (byte)Math.Clamp(value, 0, 255);
        }

        private void GenerateMatrix_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SizeTextBox.Text, out int size) || size <= 0)
            {
                MessageBox.Show("Введите корректный размер матрицы (целое число > 0)");
                return;
            }

            DataTable matrixTable = new DataTable();
            MatrixDataGrid.Columns.Clear();

            for (int i = 0; i < size; i++)
            {
                matrixTable.Columns.Add($"Col{i}", typeof(string)); // Тип string для сохранения точного ввода

                // Динамическое создание столбцов с форматированием
                var column = new DataGridTextColumn
                {
                    Header = i.ToString(),
                    Binding = new Binding($"Col{i}")
                    {
                        StringFormat = "0.###",
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    },
                    EditingElementStyle = new Style(typeof(TextBox))
                    {
                        Setters =
                {
                    new Setter(TextBox.TextProperty,
                        new Binding($"Col{i}")
                        {
                            StringFormat = "0.###",
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        })
                }
                    }
                };
                MatrixDataGrid.Columns.Add(column);
            }

            // Заполнение начальными нулями
            for (int i = 0; i < size; i++)
            {
                DataRow row = matrixTable.NewRow();
                for (int j = 0; j < size; j++)
                {
                    row[j] = "0,000"; // Запятая как разделитель
                }
                matrixTable.Rows.Add(row);
            }

            MatrixDataGrid.ItemsSource = matrixTable.DefaultView;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MatrixDataGrid.ItemsSource == null) return;

            DataView view = (DataView)MatrixDataGrid.ItemsSource;
            DataTable table = view.Table;

            int size = table.Columns.Count;
            kernel = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] = Convert.ToDouble(table.Rows[i][j]);
                }
            }

            // Обновляем изображение
            if (bitmapImage != null)
                filteredImage.Source = FilterImage(bitmapImage, kernel, double.Parse(koef.Text),zero,right);
        }

        private void RadioButtonZero_Checked(object sender, RoutedEventArgs e)
        {
            zero = true;
        }

        private void RadioButtonBorder_Checked(object sender, RoutedEventArgs e)
        {
            zero = false;
        }

        private void RadioButtonRight_Checked(object sender, RoutedEventArgs e)
        {
            right = true;
        }

        private void RadioButtonLeft_Checked(object sender, RoutedEventArgs e)
        {
            right = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            kernel = new double[,]
{
                { 0.000789, 0.006581, 0.013347, 0.006581, 0.000789 },
                { 0.006581, 0.054901, 0.111345, 0.054901, 0.006581 },
                { 0.013347, 0.111345, 0.225821, 0.111345, 0.013347 },
                { 0.006581, 0.054901, 0.111345, 0.054901, 0.006581 },
                { 0.000789, 0.006581, 0.013347, 0.006581, 0.000789 }
            };

            FillDataGridWithKernel();
            double k = 1;
            koef.Text = k.ToString();
            filteredImage.Source = FilterImage(bitmapImage, kernel, 1, zero, right);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            kernel = new double[,]
            {
                { -1, -1,-1 },
                { -1,9,-1 },
                { -1,-1,-1 }

            };

            FillDataGridWithKernel();

            double k = 1;
            koef.Text = k.ToString();

            filteredImage.Source = FilterImage(bitmapImage, kernel, 1, zero, right);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            kernel = new double[,]
            {
                { -1,0,1 },
                { -2,0,2 },
                { -1,0,1 }

            };

            FillDataGridWithKernel();

            double k = 1;
            koef.Text = k.ToString();

            filteredImage.Source = FilterImage(bitmapImage, kernel, 1, zero, right);
        }

        private void FillDataGridWithKernel()
        {
            // Создаем DataTable
            DataTable kernelTable = new DataTable();

            // Добавляем столбцы (по количеству столбцов в kernel)
            for (int i = 0; i < kernel.GetLength(1); i++)
            {
                kernelTable.Columns.Add($"Col{i}", typeof(double));
            }

            // Заполняем строками из kernel
            for (int i = 0; i < kernel.GetLength(0); i++)
            {
                DataRow row = kernelTable.NewRow();
                for (int j = 0; j < kernel.GetLength(1); j++)
                {
                    row[j] = kernel[i, j];
                }
                kernelTable.Rows.Add(row);
            }

            // Настраиваем DataGrid
            MatrixDataGrid.AutoGenerateColumns = false;
            MatrixDataGrid.Columns.Clear();

            // Создаем столбцы с форматированием для дробей
            for (int i = 0; i < kernel.GetLength(1); i++)
            {
                var column = new DataGridTextColumn
                {
                    Header = i.ToString(),
                    Binding = new Binding($"[{i}]")
                    {
                        StringFormat = "0.######" // Формат для 6 знаков после запятой
                    },
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                };

                MatrixDataGrid.Columns.Add(column);
            }

            // Привязываем данные
            MatrixDataGrid.ItemsSource = kernelTable.DefaultView;
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            kernel = new double[,]
            {
                { -1,-2,-1 },
                {  0, 0, 0 },
                {  1, 2, 1 }

            };

            FillDataGridWithKernel();

            double k = 1;
            koef.Text = k.ToString();

            filteredImage.Source = FilterImage(bitmapImage, kernel, 1, zero, right);
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            kernel = new double[,]
                {
                    {  0, -1,  0 },
                    { -1,  4, -1 },
                    {  0, -1,  0 }
                };

            FillDataGridWithKernel();

            double k = 1;
            koef.Text = k.ToString();

            filteredImage.Source = FilterImage(bitmapImage, kernel, 1, zero, right);
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            kernel = new double[,]
            {
                { -2, -1, 0 },
                { -1,  1, 1 },
                {  0,  1, 2 }
            };

            FillDataGridWithKernel();

            double k = 1;
            koef.Text = k.ToString();

            filteredImage.Source = FilterImage(bitmapImage, kernel, 1, zero, right);
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            kernel = new double[,]
                {
                    { 1/3.0, 0,    0 },
                    { 0,    1/3.0, 0 },
                    { 0,    0,    1/3.0 }
                };

            FillDataGridWithKernel();

            double k = 1;
            koef.Text = k.ToString();

            filteredImage.Source = FilterImage(bitmapImage, kernel, 1, zero, right);
        }
    }
}
