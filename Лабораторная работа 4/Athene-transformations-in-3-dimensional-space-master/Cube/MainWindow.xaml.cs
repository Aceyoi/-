using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

namespace Cube
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double angleX = Math.Cos(Math.PI/10);
        double angleY = Math.Sin(Math.PI/12);


        double rotateAngle = 10*Math.PI/180;

        private DispatcherTimer timer;

        private List<Line> lines = new List<Line>();

        private List<(Point3D, Point3D)> edges = new List<(Point3D, Point3D)>();

        bool rotating = false;
        double centerX;
        double centerY;
        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetCenter();
        }

        private void SetCenter()
        {
            centerX = cubeCanvas.ActualWidth / 2;
            centerY = cubeCanvas.ActualHeight / 2;
        }
        void drawCube(int length)
        {
            addLine(centerX, centerY, length, length, 0, length, 0, 0, Brushes.Black);
            addLine(centerX, centerY, length, length, 0, 0, length, 0, Brushes.Black);
            addLine(centerX, centerY, length, length, 0, length, length, length, Brushes.Black);

            addLine(centerX, centerY, length, 0, length, length, 0, 0, Brushes.Black);
            addLine(centerX, centerY, length, 0, length, length, length, length, Brushes.Black);
            addLine(centerX, centerY, length, 0, length, 0, 0, length, Brushes.Black);

            addLine(centerX,centerY, 0, length, length, 0, 0, length, Brushes.Black);
            addLine(centerX, centerY, 0, length, length, 0, length, 0, Brushes.Black);
            addLine(centerX, centerY, 0, length, length, length, length, length, Brushes.Black);

            addLine(centerX, centerY, 0, 0, 0, length, 0, 0, Brushes.Green);
            addLine(centerX, centerY, 0, 0, 0, 0, length, 0, Brushes.Red);
            addLine(centerX, centerY, 0, 0, 0, 0, 0, length, Brushes.Blue);
        }

        private (double,double) projectTo2d(double x, double y, double z)
        {
            double X1 = (x - y) * angleX;
            double Y1 = z + (x + y) * angleY;

            //double X1 = x;
            //double Y1 = y;

            return (X1, Y1);
        } 
        public void addLine(double centerX,double centerY,double x,double y, double z, double x1,double y1, double z1, Brush c)
        {
            edges.Add((new Point3D(x, y, z), new Point3D(x1, y1, z1)));

            (double, double) start = projectTo2d(x, y, z);

            (double, double) end = projectTo2d(x1, y1, z1);

            
            Line line = new Line
            {
                Stroke = c,
                StrokeThickness = 2,
                X1 = centerX + start.Item1,
                Y1 = centerY + start.Item2,
                X2 = centerX + end.Item1,
                Y2 = centerY + end.Item2
            };
            cubeCanvas.Children.Add(line);
            lines.Add(line);
        }

        private void rotateX()
        {
            var newEdges = new List<(Point3D, Point3D)>();

            int i = 0;
            foreach (var line in lines)
            {
                Point3D start = edges[i].Item1;
                Point3D end = edges[i].Item2;

               
                double startY = start.Y * Math.Cos(rotateAngle) + start.Z * Math.Sin(rotateAngle);
                double startZ = -start.Y * Math.Sin(rotateAngle) + start.Z * Math.Cos(rotateAngle);

                double endY = end.Y * Math.Cos(rotateAngle) + end.Z * Math.Sin(rotateAngle);
                double endZ = -end.Y * Math.Sin(rotateAngle) + end.Z * Math.Cos(rotateAngle);

                start.Y = startY;
                start.Z = startZ;
                end.Y = endY;
                end.Z = endZ;

                
                (double, double) sp = projectTo2d(start.X, start.Y, start.Z);
                (double, double) ep = projectTo2d(end.X, end.Y, end.Z);

                
                line.X1 = centerX + sp.Item1;
                line.Y1 = centerY + sp.Item2;
                line.X2 = centerX + ep.Item1;
                line.Y2 = centerY + ep.Item2;

                
                newEdges.Add((new Point3D(start.X, start.Y, start.Z), new Point3D(end.X, end.Y, end.Z)));
                i++;
            }

            
            edges = newEdges;
        }

        private void rotateY()
        {
            var newEdges = new List<(Point3D, Point3D)>();

            int i = 0;
            foreach (var line in lines)
            {
                Point3D start = edges[i].Item1;
                Point3D end = edges[i].Item2;

                
                double startX = start.X * Math.Cos(rotateAngle) + start.Z * Math.Sin(rotateAngle);
                double startZ = -start.X * Math.Sin(rotateAngle) + start.Z * Math.Cos(rotateAngle);

                double endX = end.X * Math.Cos(rotateAngle) + end.Z * Math.Sin(rotateAngle);
                double endZ = -end.X * Math.Sin(rotateAngle) + end.Z * Math.Cos(rotateAngle);

                
                start.X = startX;
                start.Z = startZ;
                end.X = endX;
                end.Z = endZ;

                
                (double, double) sp = projectTo2d(start.X, start.Y, start.Z);
                (double, double) ep = projectTo2d(end.X, end.Y, end.Z);

                
                line.X1 = centerX + sp.Item1;
                line.Y1 = centerY + sp.Item2;
                line.X2 = centerX + ep.Item1;
                line.Y2 = centerY + ep.Item2;

                
                newEdges.Add((new Point3D(start.X, start.Y, start.Z), new Point3D(end.X, end.Y, end.Z)));
                i++;
            }

            
            edges = newEdges;
        }

        private void rotateZ()
        {
            var newEdges = new List<(Point3D, Point3D)>();

            int i = 0;
            foreach (var line in lines)
            {
                Point3D start = edges[i].Item1;
                Point3D end = edges[i].Item2;

               
                double startX = start.X * Math.Cos(rotateAngle) + start.Y * Math.Sin(rotateAngle);
                double startY = -start.X * Math.Sin(rotateAngle) + start.Y * Math.Cos(rotateAngle);

                double endX = end.X * Math.Cos(rotateAngle) + end.Y * Math.Sin(rotateAngle);
                double endY = -end.X * Math.Sin(rotateAngle) + end.Y * Math.Cos(rotateAngle);

                
                start.X = startX;
                start.Y = startY;
                end.X = endX;
                end.Y = endY;

                
                (double, double) sp = projectTo2d(start.X, start.Y, start.Z);
                (double, double) ep = projectTo2d(end.X, end.Y, end.Z);

                
                line.X1 = centerX + sp.Item1;
                line.Y1 = centerY + sp.Item2;
                line.X2 = centerX + ep.Item1;
                line.Y2 = centerY + ep.Item2;

                
                newEdges.Add((new Point3D(start.X, start.Y, start.Z), new Point3D(end.X, end.Y, end.Z)));
                i++;
            }

            
            edges = newEdges;
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            clear();
            drawCube(int.Parse(lengthInput.Text));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (check_X.IsChecked == true)
            {
                rotateX();
            }
            if (check_Y.IsChecked == true)
            {
                rotateY();
            }
            if(check_Z.IsChecked == true)
            {
                rotateZ();
            }
        }

        private void clear()
        {
            foreach (Line line in lines)
            {
                cubeCanvas.Children.Remove(line);
            }
            lines.Clear();
            edges.Clear();
            
        }

        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            if (!rotating)
            {
                timer.Start();
                rotating = true;
            }
            else
            {
                timer.Stop();
                rotating = false;
            }

        }
    }
}

// Класс для представления 3D точки
public class Point3D
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}