using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Pyramid3D
{
    public partial class MainWindow : Window
    {
        private Model3DGroup modelGroup = new Model3DGroup();
        private AxisAngleRotation3D rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
        private Point lastMousePos;
        private Random rand = new Random();

        public MainWindow()
        {
            InitializeComponent();
            MouseMove += Window_MouseMove;
            MouseDown += (s, e) => lastMousePos = e.GetPosition(this);
            BuildScene(5);
        }

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(SidesBox.Text, out int n) && n >= 3)
                BuildScene(n);
            else
                MessageBox.Show("Введите число ≥ 3");
        }

        private void BuildScene(int sides)
        {
            viewport.Children.Clear();
            modelGroup = new Model3DGroup();
            modelGroup.Transform = new RotateTransform3D(rotation);

            AddLight();
            AddPyramid(sides);

            var modelVisual = new ModelVisual3D { Content = modelGroup };
            viewport.Children.Add(modelVisual);
        }

        private void AddLight()
        {
            modelGroup.Children.Add(new AmbientLight(Colors.DarkGray));
            modelGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -2)));
        }

        private void AddPyramid(int n)
        {
            double radius = 1;
            double height = 1.5;
            Point3D top = new Point3D(0, height, 0);
            List<Point3D> basePoints = new List<Point3D>();

            for (int i = 0; i < n; i++)
            {
                double angle = 2 * Math.PI * i / n;
                basePoints.Add(new Point3D(
                    radius * Math.Cos(angle),
                    0,
                    radius * Math.Sin(angle)));
            }

            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                modelGroup.Children.Add(CreateFace(new[]
                {
                    basePoints[i], basePoints[next], top
                }));
            }

            for (int i = 1; i < n - 1; i++)
            {
                modelGroup.Children.Add(CreateFace(new[]
                {
                    basePoints[0], basePoints[i], basePoints[i + 1]
                }));
            }
        }

        private GeometryModel3D CreateFace(Point3D[] pts)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = new Point3DCollection(pts);
            mesh.TriangleIndices = new Int32Collection { 0, 1, 2 };

            var color = Color.FromRgb((byte)rand.Next(64, 256), (byte)rand.Next(64, 256), (byte)rand.Next(64, 256));

            var brush = new SolidColorBrush(color);
            brush.Freeze();

            var backBrush = new SolidColorBrush(Colors.Gray);
            backBrush.Freeze();

            return new GeometryModel3D
            {
                Geometry = mesh,
                Material = new DiffuseMaterial(brush),
                BackMaterial = new DiffuseMaterial(backBrush)
            };
        }


        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point current = e.GetPosition(this);
                double dx = current.X - lastMousePos.X;
                rotation.Angle += dx * 0.5;
                lastMousePos = current;
            }
        }
    }
}
