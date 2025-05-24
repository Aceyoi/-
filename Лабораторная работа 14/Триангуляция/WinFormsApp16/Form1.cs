using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp16
{
    public partial class Form1 : Form
    {
        private List<PointF> points = new List<PointF>();
        private List<PointF[]> triangles = new List<PointF[]>();
        private bool isDrawing = false;

        public Form1()
        {
            InitializeComponent();

            this.Text = "Триангуляция вогнутой фигуры";
            this.DoubleBuffered = true;
            this.ClientSize = new Size(800, 600);

            var triangulateButton = new Button
            {
                Text = "Триангулировать",
                Location = new Point(10, 10),
                Size = new Size(120, 30)
            };
            triangulateButton.Click += TriangulateButton_Click;
            this.Controls.Add(triangulateButton);

            var clearButton = new Button
            {
                Text = "Очистить",
                Location = new Point(140, 10),
                Size = new Size(80, 30)
            };
            clearButton.Click += ClearButton_Click;
            this.Controls.Add(clearButton);

            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            this.Paint += Form1_Paint;
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            points.Clear();
            triangles.Clear();
            isDrawing = false;
            this.Invalidate();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!isDrawing && points.Count == 0)
                    isDrawing = true;

                points.Add(e.Location);
                this.Invalidate();
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && points.Count > 0)
            {
                this.Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = false;
                this.Invalidate();
            }
        }

        private void TriangulateButton_Click(object sender, EventArgs e)
        {
            if (points.Count < 3)
            {
                MessageBox.Show("Фигура должна содержать как минимум 3 точки");
                return;
            }

            if (points.Count > 2 && points[0] != points[points.Count - 1])
            {
                points.Add(points[0]);
            }

            triangles = Triangulate(points);
            this.Invalidate();
        }

        // Алгоритм Ear Clipping для триангуляции
        private List<PointF[]> Triangulate(List<PointF> polygon)
        {
            List<PointF[]> result = new List<PointF[]>();

            // Удаляем дубликаты точек подряд
            List<PointF> workingList = RemoveDuplicatePoints(polygon);

            if (workingList.Count < 3)
                return result;

            // Проверяем направление (по или против часовой стрелки)
            float area = CalculateSignedArea(workingList);

            if (area > 0)
            {
                workingList.Reverse();
            }

            while (workingList.Count > 3)
            {
                bool earFound = false;
                for (int i = 0; i < workingList.Count; i++)
                {
                    int prev = (i - 1 + workingList.Count) % workingList.Count;
                    int curr = i;
                    int next = (i + 1) % workingList.Count;

                    PointF a = workingList[prev];
                    PointF b = workingList[curr];
                    PointF c = workingList[next];

                    if (IsConvex(a, b, c) && IsEar(workingList, prev, curr, next))
                    {
                        result.Add(new PointF[] { a, b, c });
                        workingList.RemoveAt(curr);
                        earFound = true;
                        break;
                    }
                }

                if (!earFound)
                {
                    // Резервный метод, если не найдено "ухо"
                    return BackupTriangulation(workingList);
                }
            }

            if (workingList.Count == 3)
            {
                result.Add(workingList.ToArray());
            }

            return result;
        }

        private List<PointF> RemoveDuplicatePoints(List<PointF> points)
        {
            List<PointF> result = new List<PointF>();
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0 || points[i] != points[i - 1])
                {
                    result.Add(points[i]);
                }
            }
            return result;
        }

        private float CalculateSignedArea(List<PointF> polygon)
        {
            float area = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                int j = (i + 1) % polygon.Count;
                area += polygon[i].X * polygon[j].Y;
                area -= polygon[i].Y * polygon[j].X;
            }
            return area;
        }

        private bool IsConvex(PointF a, PointF b, PointF c)
        {
            float cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            return cross <= 0;
        }

        private bool IsEar(List<PointF> polygon, int prev, int curr, int next)
        {
            PointF a = polygon[prev];
            PointF b = polygon[curr];
            PointF c = polygon[next];

            for (int i = 0; i < polygon.Count; i++)
            {
                if (i == prev || i == curr || i == next)
                    continue;

                if (IsPointInTriangle(polygon[i], a, b, c))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsPointInTriangle(PointF p, PointF a, PointF b, PointF c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private float Sign(PointF p1, PointF p2, PointF p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private List<PointF[]> BackupTriangulation(List<PointF> polygon)
        {
            List<PointF[]> result = new List<PointF[]>();
            PointF centroid = CalculateCentroid(polygon);

            for (int i = 0; i < polygon.Count; i++)
            {
                int next = (i + 1) % polygon.Count;
                result.Add(new PointF[] { polygon[i], polygon[next], centroid });
            }

            return result;
        }

        private PointF CalculateCentroid(List<PointF> points)
        {
            float x = 0, y = 0;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
            }
            return new PointF(x / points.Count, y / points.Count);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Рисуем контур фигуры
            if (points.Count > 1)
            {
                g.DrawLines(Pens.Black, points.ToArray());

                // Замыкаем фигуру, если есть треугольники (значит фигура замкнута)
                if (triangles.Count > 0 && points.Count > 2)
                {
                    g.DrawLine(Pens.Black, points[points.Count - 1], points[0]);
                }
            }

            // Рисуем точки
            foreach (var point in points)
            {
                g.FillEllipse(Brushes.Red, point.X - 3, point.Y - 3, 6, 6);
            }

            // Рисуем треугольники
            foreach (var triangle in triangles)
            {
                g.DrawPolygon(Pens.Blue, triangle);

                using (var brush = new SolidBrush(Color.FromArgb(50, 0, 0, 255)))
                {
                    g.FillPolygon(brush, triangle);
                }
            }
        }
    }
}