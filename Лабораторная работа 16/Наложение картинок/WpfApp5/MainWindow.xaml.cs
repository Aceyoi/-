using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageOverlayApp
{
    public partial class MainWindow : Window
    {
        private BitmapImage firstImage;
        private Queue<BitmapImage> secondImagesQueue = new();
        private List<(BitmapImage image, Point position)> overlays = new();
        private RenderTargetBitmap mergedResult;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadFirstImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dlg.ShowDialog() == true)
            {
                firstImage = new BitmapImage(new Uri(dlg.FileName));
                overlays.Clear();
                RedrawMergedImage();
            }
        }

        private void LoadSecondImages_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dlg.ShowDialog() == true)
            {
                secondImagesQueue.Clear();

                foreach (string file in dlg.FileNames)
                {
                    BitmapImage img = new BitmapImage(new Uri(file));
                    secondImagesQueue.Enqueue(img);
                }
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (firstImage == null || secondImagesQueue.Count == 0)
                return;

            Point clickPosition = e.GetPosition(ImageCanvas);
            BitmapImage nextImage = secondImagesQueue.Dequeue();
            overlays.Add((nextImage, clickPosition));
            RedrawMergedImage();
        }

        private void RedrawMergedImage()
        {
            if (firstImage == null) return;

            int width = firstImage.PixelWidth;
            int height = firstImage.PixelHeight;

            mergedResult = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawImage(firstImage, new Rect(0, 0, width, height));

                foreach ((BitmapImage image, Point position) in overlays)
                {
                    dc.PushOpacity(0.5);
                    dc.DrawImage(image, new Rect(position.X, position.Y, image.PixelWidth, image.PixelHeight));
                    dc.Pop();
                }
            }

            mergedResult.Render(dv);

            ImageCanvas.Children.Clear();
            Image resultImage = new Image { Source = mergedResult };
            ImageCanvas.Children.Add(resultImage);
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (mergedResult == null)
            {
                MessageBox.Show("Нет изображения для сохранения.");
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "PNG файл|*.png",
                FileName = "merged.png"
            };

            if (dlg.ShowDialog() == true)
            {
                using FileStream stream = new FileStream(dlg.FileName, FileMode.Create);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(mergedResult));
                encoder.Save(stream);
                MessageBox.Show("Изображение сохранено.");
            }
        }
    }
}
