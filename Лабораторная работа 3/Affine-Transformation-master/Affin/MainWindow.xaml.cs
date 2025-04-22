using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Affin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool drawing = true;
        private bool clicked = false;
        private int rotationAngle = 0;
        private DispatcherTimer timer;
        private Point firstPoint;
        private List<Line> lines = new List<Line>();
        public MainWindow()

        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCanvas(drawCanvas);
        }

        private void InitializeCanvas(Canvas _Canvas)
        {
            Line axisY = new Line
            {
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                X1 = _Canvas.ActualWidth / 2,
                Y1 = 0,
                X2 = _Canvas.ActualWidth / 2,
                Y2 = _Canvas.ActualHeight
            };
            _Canvas.Children.Add(axisY);

            Line axisX = new Line
            {
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                X1 = 0,
                Y1 = _Canvas.ActualHeight / 2,
                X2 = _Canvas.ActualWidth,
                Y2 = _Canvas.ActualHeight / 2
            };
            _Canvas.Children.Add(axisX);
        }

        private void CanvasClick(object sender, MouseEventArgs e)
        {
            
            if (drawing)
            {
                if(!clicked)
                {
                    firstPoint = e.GetPosition(drawCanvas);
                    clicked = true;
                }
                else
                {
                    Point endPoint = e.GetPosition(drawCanvas);
                    Line line = new Line
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        X1 = firstPoint.X,
                        Y1 = firstPoint.Y,
                        X2 = endPoint.X,
                        Y2 = endPoint.Y
                    };
                    drawCanvas.Children.Add(line);
                    lines.Add(line);
                    firstPoint = endPoint;
                    if(!(bool)sequance.IsChecked) { clicked = false; }
                    
                }
            }
        }

        private void clear(object sender, RoutedEventArgs e)
        {
            foreach(Line line in lines)
            {
                drawCanvas.Children.Remove(line);
            }
            lines.Clear();
            clicked = false;
            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            rotate(lines, rotationAngle);
        }

        private void rotate(List<Line> lines, int angle)
        {
            foreach (Line line in lines)
            {
                line.Y1 -= drawCanvas.ActualHeight / 2;
                line.Y2 -= drawCanvas.ActualHeight / 2;

                line.X1 -= drawCanvas.ActualWidth / 2;
                line.X2 -= drawCanvas.ActualWidth / 2;

                double new_x1 = line.X1 * Math.Cos(angle * Math.PI / 180) - line.Y1 * Math.Sin(angle * Math.PI / 180);
                double new_y1 = line.X1 * Math.Sin(angle * Math.PI / 180) + line.Y1 * Math.Cos(angle * Math.PI / 180);

                double new_x2 = line.X2 * Math.Cos(angle * Math.PI / 180) - line.Y2 * Math.Sin(angle * Math.PI / 180);
                double new_y2 = line.X2 * Math.Sin(angle * Math.PI / 180) + line.Y2 * Math.Cos(angle * Math.PI / 180);

                line.Y1 = new_y1 + drawCanvas.ActualHeight / 2;
                line.Y2 = new_y2 + drawCanvas.ActualHeight / 2;

                line.X1 = new_x1 + drawCanvas.ActualWidth / 2;
                line.X2 = new_x2 + drawCanvas.ActualWidth / 2;
            }
        }
        private void start(object sender, RoutedEventArgs e)
        {

            if (lines.Count > 0)
            {
                if (rotationAngle != 0 || !angleInput.Text.Equals(""))
                {
                    if (!angleInput.Text.Equals(""))
                    {
                        rotationAngle = int.Parse(angleInput.Text);
                    }
                }
                
                if (drawing)
                {
                    timer.Start();
                    drawing = false;
                    clearButton.IsEnabled = false;

                    angleInput.IsEnabled = false;
                    clicked = false;
                }
                else
                {
                    timer.Stop();
                    drawing = true;
                    clearButton.IsEnabled = true;
                    angleInput.IsEnabled = true;
                }
            }
        }

        private void rotateButton_Click(object sender, RoutedEventArgs e)
        {
            if (rotationAngle != 0 || !angleInput.Text.Equals("")){
                if (!angleInput.Text.Equals(""))
                {
                    rotationAngle = int.Parse(angleInput.Text);
                }
                rotate(lines, rotationAngle);
            }
        }

        private void sequance_Checked(object sender, RoutedEventArgs e)
        {
            clicked = false;
        }
    }
}
