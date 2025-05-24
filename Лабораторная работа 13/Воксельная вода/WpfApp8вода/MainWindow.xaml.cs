using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace VoxelWater
{
    public partial class MainWindow : Window
    {
        PerspectiveCamera camera;
        Model3DGroup sceneGroup = new Model3DGroup();
        List<WaterCube> water = new List<WaterCube>();
        HashSet<(int x, int y, int z)> occupied = new HashSet<(int, int, int)>();

        Point lastMousePos;
        bool isRotating = false;
        AxisAngleRotation3D rotationY = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
        AxisAngleRotation3D rotationX = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 10);
        Transform3DGroup cameraTransform = new Transform3DGroup();

        public MainWindow()
        {
            InitializeComponent();
            InitScene();

            cameraTransform.Children.Add(new RotateTransform3D(rotationY));
            cameraTransform.Children.Add(new RotateTransform3D(rotationX));
            viewport.Camera.Transform = cameraTransform;

            viewport.MouseDown += Viewport_MouseDown;
            viewport.MouseUp += Viewport_MouseUp;
            viewport.MouseMove += Viewport_MouseMove;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += (s, e) => UpdateWater();
            timer.Start();
        }

        private void InitScene()
        {
            camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 10, 40),
                LookDirection = new Vector3D(0, -5, -40),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 60
            };

            viewport.Camera = camera;

            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = sceneGroup;
            viewport.Children.Add(modelVisual);

            // Свет
            sceneGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)));

            // Куб — источник воды
            GeometryModel3D cube = CreateCube(0, 5, 0, Colors.Black);
            sceneGroup.Children.Add(cube);

            // Миска (поверхность)
            for (int x = -5; x <= 5; x++)
            {
                for (int z = -5; z <= 5; z++)
                {
                    double y = -Math.Sqrt(Math.Max(0, 25 - x * x - z * z)) / 5;
                    GeometryModel3D voxel = CreateCube(x, y, z, Colors.Red);
                    sceneGroup.Children.Add(voxel);
                    occupied.Add((x, (int)Math.Round(y), z));
                }
            }
        }

        private void UpdateWater()
        {
            // Новая капля
            var drop = new WaterCube(0, 5, 0);
            water.Add(drop);
            sceneGroup.Children.Add(drop.Model);

            List<WaterCube> newDrops = new List<WaterCube>();

            foreach (var w in water)
            {
                int wx = (int)Math.Round(w.X);
                int wy = (int)Math.Round(w.Y);
                int wz = (int)Math.Round(w.Z);

                var below = (wx, wy - 1, wz);

                if (!occupied.Contains(below) && !IsSolid(below.Item1, below.Item2, below.Item3))
                {
                    w.Y -= 1;
                    w.UpdatePosition();
                }
                else
                {
                    var dirs = new (int dx, int dz)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
                    bool moved = false;

                    foreach (var (dx, dz) in dirs)
                    {
                        int nx = wx + dx;
                        int nz = wz + dz;
                        int ny = wy;

                        var pos = (nx, ny, nz);
                        var under = (nx, ny - 1, nz);

                        if (!occupied.Contains(pos) && IsSolid(under.Item1, under.Item2, under.Item3))
                        {
                            var d = new WaterCube(nx, ny, nz);
                            newDrops.Add(d);
                            occupied.Add(pos);
                            moved = true;
                            break;
                        }
                    }

                    if (!moved)
                    {
                        occupied.Add((wx, wy, wz));
                    }
                }
            }

            foreach (var d in newDrops)
            {
                water.Add(d);
                sceneGroup.Children.Add(d.Model);
            }
        }

        private bool IsSolid(int x, int y, int z)
        {
            double bowlY = -Math.Sqrt(Math.Max(0, 25 - x * x - z * z)) / 5;
            return y <= bowlY + 0.5 || occupied.Contains((x, y, z));
        }

        private GeometryModel3D CreateCube(double x, double y, double z, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            double size = 0.5;

            Point3D[] pts = new Point3D[]
            {
                new Point3D(x-size, y-size, z-size),
                new Point3D(x+size, y-size, z-size),
                new Point3D(x+size, y+size, z-size),
                new Point3D(x-size, y+size, z-size),
                new Point3D(x-size, y-size, z+size),
                new Point3D(x+size, y-size, z+size),
                new Point3D(x+size, y+size, z+size),
                new Point3D(x-size, y+size, z+size)
            };

            int[] indices = {
                0,1,2, 2,3,0, // Front
                1,5,6, 6,2,1, // Right
                5,4,7, 7,6,5, // Back
                4,0,3, 3,7,4, // Left
                3,2,6, 6,7,3, // Top
                4,5,1, 1,0,4  // Bottom
            };

            foreach (int index in indices)
                mesh.Positions.Add(pts[index]);

            for (int i = 0; i < mesh.Positions.Count; i++)
                mesh.TriangleIndices.Add(i);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));
            return new GeometryModel3D(mesh, material);
        }

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isRotating = true;
                lastMousePos = e.GetPosition(this);
                Mouse.Capture(viewport);
            }
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isRotating = false;
            Mouse.Capture(null);
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isRotating) return;

            Point current = e.GetPosition(this);
            double dx = current.X - lastMousePos.X;
            double dy = current.Y - lastMousePos.Y;

            rotationY.Angle += dx * 0.5;
            rotationX.Angle += dy * 0.5;
            rotationX.Angle = Math.Max(-90, Math.Min(90, rotationX.Angle));

            lastMousePos = current;
        }
    }

    public class WaterCube
    {
        public double X, Y, Z;
        public GeometryModel3D Model;

        public WaterCube(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
            Model = CreateModel(Colors.Blue);
            UpdatePosition();
        }

        private GeometryModel3D CreateModel(Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            double size = 0.5;

            Point3D[] pts = new Point3D[]
            {
                new Point3D(-size,-size,-size),
                new Point3D( size,-size,-size),
                new Point3D( size, size,-size),
                new Point3D(-size, size,-size),
                new Point3D(-size,-size, size),
                new Point3D( size,-size, size),
                new Point3D( size, size, size),
                new Point3D(-size, size, size)
            };

            int[] indices = {
                0,1,2, 2,3,0, // Front
                1,5,6, 6,2,1, // Right
                5,4,7, 7,6,5, // Back
                4,0,3, 3,7,4, // Left
                3,2,6, 6,7,3, // Top
                4,5,1, 1,0,4  // Bottom
            };

            foreach (int index in indices)
                mesh.Positions.Add(pts[index]);

            for (int i = 0; i < mesh.Positions.Count; i++)
                mesh.TriangleIndices.Add(i);

            Material mat = new DiffuseMaterial(new SolidColorBrush(color));
            return new GeometryModel3D(mesh, mat);
        }

        public void UpdatePosition()
        {
            Model.Transform = new TranslateTransform3D(X, Y, Z);
        }
    }
}
