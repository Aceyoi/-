using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp8
{
    public partial class Form1 : Form
    {
        private Bitmap bmp;
        private Graphics g;
        private PointF[] starPoints;
        private bool isDrawing = false;
        private Point lastPoint;
        private Color currentColor = Color.Black;
        private int penWidth = 2;
        private bool fillMode = false;
        private bool useModifiedAlgorithm = false; // Флаг для выбора алгоритма

        // Цвета для разных алгоритмов
        private readonly Color fillColor4 = Color.Yellow;    // Желтый для 4-связного
        private readonly Color fillColor8 = Color.Green;     // Зеленый для 8-связного

        public Form1()
        {
            InitializeComponent();
            InitializeDrawing();
            SetupEventHandlers();
        }

        private void InitializeDrawing()
        {
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Рисуем звезду (добавлено)
            starPoints = CalculateStarPoints(5, new PointF(pictureBox1.Width / 2, pictureBox1.Height / 2), 180, 200);
            g.DrawPolygon(new Pen(currentColor, penWidth), starPoints);

            pictureBox1.Image = bmp;
        }

        private void SetupEventHandlers()
        {
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseLeave += PictureBox1_MouseLeave;
            btnFill.Click += BtnFill_Click;
            btnDraw.Click += BtnDraw_Click;
            btnAlgorithm.Click += BtnAlgorithm_Click; // Кнопка переключения алгоритма
        }

        private void BtnAlgorithm_Click(object sender, EventArgs e)
        {
            useModifiedAlgorithm = !useModifiedAlgorithm;
            btnAlgorithm.Text = useModifiedAlgorithm ? "8-связный" : "4-связный";
            btnAlgorithm.BackColor = useModifiedAlgorithm ? Color.LightBlue : SystemColors.Control;
        }
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (fillMode)
                {
                    if (IsPointInPolygon(starPoints, e.Location))
                    {
                        if (useModifiedAlgorithm)
                            Fill8ConnectedOptimized(e.Location, fillColor8, Color.White);
                        else
                            Fill4ConnectedOptimized(e.Location, fillColor4, Color.White);
                        pictureBox1.Refresh();
                    }
                }
                else
                {
                    isDrawing = true;
                    lastPoint = e.Location;
                }
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && !fillMode)
            {
                using (Pen pen = new Pen(currentColor, penWidth))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                    g.DrawLine(pen, lastPoint, e.Location);
                    lastPoint = e.Location;
                }
                pictureBox1.Refresh();
            }
        }

        // 8-связный модифицированный алгоритм заливки
        private void Fill8ConnectedOptimized(Point startPoint, Color fillColor, Color targetColor)
        {
            if (startPoint.X < 0 || startPoint.X >= bmp.Width ||
                startPoint.Y < 0 || startPoint.Y >= bmp.Height)
                return;

            Color startColor = bmp.GetPixel(startPoint.X, startPoint.Y);
            if (startColor.ToArgb() != targetColor.ToArgb())
                return;

            var queue = new Queue<Point>();
            queue.Enqueue(startPoint);
            bool[,] visited = new bool[bmp.Width, bmp.Height];

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();

                if (p.X < 0 || p.X >= bmp.Width || p.Y < 0 || p.Y >= bmp.Height)
                    continue;

                if (visited[p.X, p.Y])
                    continue;

                Color currentColor = bmp.GetPixel(p.X, p.Y);
                if (currentColor.ToArgb() == targetColor.ToArgb())
                {
                    bmp.SetPixel(p.X, p.Y, fillColor);
                    visited[p.X, p.Y] = true;

                    // Основные направления (4-связные)
                    queue.Enqueue(new Point(p.X - 1, p.Y)); // Влево
                    queue.Enqueue(new Point(p.X + 1, p.Y)); // Вправо
                    queue.Enqueue(new Point(p.X, p.Y - 1)); // Вверх
                    queue.Enqueue(new Point(p.X, p.Y + 1)); // Вниз

                    // Диагональные направления (дополнительно для 8-связности)
                    queue.Enqueue(new Point(p.X - 1, p.Y - 1)); // Влево-вверх
                    queue.Enqueue(new Point(p.X + 1, p.Y - 1)); // Вправо-вверх
                    queue.Enqueue(new Point(p.X - 1, p.Y + 1)); // Влево-вниз
                    queue.Enqueue(new Point(p.X + 1, p.Y + 1)); // Вправо-вниз
                }
            }
            pictureBox1.Refresh();
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e) => isDrawing = false;
        private void PictureBox1_MouseLeave(object sender, EventArgs e) => isDrawing = false;

        private void BtnFill_Click(object sender, EventArgs e)
        {
            fillMode = true;
            btnFill.BackColor = Color.LightGreen;
            btnDraw.BackColor = SystemColors.Control;
        }

        private void BtnDraw_Click(object sender, EventArgs e)
        {
            fillMode = false;
            btnDraw.BackColor = Color.LightGreen;
            btnFill.BackColor = SystemColors.Control;
        }

        private void Fill4ConnectedOptimized(Point startPoint, Color fillColor, Color targetColor)
        {
            // Проверка границ изображения
            if (startPoint.X < 0 || startPoint.X >= bmp.Width ||
                startPoint.Y < 0 || startPoint.Y >= bmp.Height)
                return;

            // Получаем цвет начальной точки
            Color startColor = bmp.GetPixel(startPoint.X, startPoint.Y);

            // Проверка совпадения цвета
            if (startColor.ToArgb() != targetColor.ToArgb())
                return;

            // Оптимизированная заливка с использованием очереди
            var queue = new Queue<Point>();
            queue.Enqueue(startPoint);

            // Массив для отслеживания посещенных точек
            bool[,] visited = new bool[bmp.Width, bmp.Height];

            // Основной цикл заливки
            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();

                // Пропускаем точки вне изображения
                if (p.X < 0 || p.X >= bmp.Width || p.Y < 0 || p.Y >= bmp.Height)
                    continue;

                // Пропускаем уже посещенные точки
                if (visited[p.X, p.Y])
                    continue;

                // Получаем цвет текущего пикселя
                Color currentColor = bmp.GetPixel(p.X, p.Y);

                // Проверяем соответствие целевому цвету
                if (currentColor.ToArgb() == targetColor.ToArgb())
                {
                    // Заливаем пиксель
                    bmp.SetPixel(p.X, p.Y, fillColor);
                    visited[p.X, p.Y] = true;

                    // Добавляем соседей в очередь
                    queue.Enqueue(new Point(p.X - 1, p.Y)); // Влево
                    queue.Enqueue(new Point(p.X + 1, p.Y)); // Вправо
                    queue.Enqueue(new Point(p.X, p.Y - 1)); // Вверх
                    queue.Enqueue(new Point(p.X, p.Y + 1)); // Вниз
                }
            }

            pictureBox1.Refresh();
        }

        private void CheckAndEnqueuePixel(int x, int y, Color targetColor, bool[,] visited, Queue<Point> queue)
        {
            // Проверяем границы изображения
            if (x < 0 || x >= bmp.Width || y < 0 || y >= bmp.Height)
                return;

            // Проверяем, не посещали ли уже эту точку
            if (visited[x, y])
                return;

            // Проверяем цвет пикселя
            if (bmp.GetPixel(x, y).ToArgb() == targetColor.ToArgb())
            {
                visited[x, y] = true;
                queue.Enqueue(new Point(x, y));
            }
        }

        private bool IsPointInPolygon(PointF[] polygon, Point point)
        {
            if (polygon == null || polygon.Length < 3)
                return false;

            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
                    (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private PointF[] CalculateStarPoints(int numPoints, PointF center, float outerRadius, float innerRadius)
        {
            PointF[] points = new PointF[numPoints * 2];
            double angle = Math.PI / numPoints;

            for (int i = 0; i < numPoints * 2; i++)
            {
                double radius = i % 2 == 0 ? outerRadius : innerRadius;
                points[i] = new PointF(
                    center.X + (float)(radius * Math.Sin(i * angle)),
                    center.Y - (float)(radius * Math.Cos(i * angle))
                );
            }

            return points;
        }
    }
}