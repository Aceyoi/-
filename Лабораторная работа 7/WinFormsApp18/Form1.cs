using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp18
{
    public partial class Form1 : Form
    {
        private List<Point> currentLine = new List<Point>();
        private List<List<Point>> allLines = new List<List<Point>>();
        private Rectangle? clipRectangle = null;
        private bool isDrawing = false;
        private bool isSelectingClipArea = false;
        private Point selectionStart;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            // Создаем кнопки для управления
            var btnClip = new Button { Text = "Выделить область", Top = 10, Left = 10 };
            var btnClear = new Button { Text = "Очистить", Top = 10, Left = 120 };

            btnClip.Click += (s, e) => { isSelectingClipArea = true; };
            btnClear.Click += (s, e) => { allLines.Clear(); clipRectangle = null; this.Invalidate(); };

            this.Controls.Add(btnClip);
            this.Controls.Add(btnClear);

            // Обработчики событий мыши
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            this.Paint += Form1_Paint;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (isSelectingClipArea)
            {
                selectionStart = e.Location;
            }
            else
            {
                isDrawing = true;
                currentLine = new List<Point>();
                currentLine.Add(e.Location);
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelectingClipArea && e.Button == MouseButtons.Left)
            {
                // Обновляем прямоугольник выделения
                int x = Math.Min(selectionStart.X, e.X);
                int y = Math.Min(selectionStart.Y, e.Y);
                int width = Math.Abs(e.X - selectionStart.X);
                int height = Math.Abs(e.Y - selectionStart.Y);

                clipRectangle = new Rectangle(x, y, width, height);
                this.Invalidate();
            }
            else if (isDrawing && e.Button == MouseButtons.Left)
            {
                currentLine.Add(e.Location);
                this.Invalidate();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isSelectingClipArea)
            {
                isSelectingClipArea = false;
            }
            else if (isDrawing)
            {
                isDrawing = false;
                if (currentLine.Count > 1)
                {
                    allLines.Add(currentLine);
                }
                currentLine = null;
            }
            this.Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Рисуем все сохраненные линии с отсечением
            foreach (var line in allLines)
            {
                DrawClippedLine(g, line);
            }

            // Рисуем текущую линию (если есть) с отсечением
            if (currentLine != null && currentLine.Count > 1)
            {
                DrawClippedLine(g, currentLine);
            }

            // Рисуем прямоугольник выделения (если есть)
            if (clipRectangle.HasValue)
            {
                g.DrawRectangle(Pens.Red, clipRectangle.Value);
            }
        }

        private void DrawClippedLine(Graphics g, List<Point> line)
        {
            if (clipRectangle.HasValue)
            {
                // Отсекаем линию по прямоугольнику
                List<Point> clippedLine = new List<Point>();

                for (int i = 0; i < line.Count - 1; i++)
                {
                    Point p1 = line[i];
                    Point p2 = line[i + 1];

                    if (clipRectangle.Value.Contains(p1) && clipRectangle.Value.Contains(p2))
                    {
                        // Оба точки внутри - добавляем как есть
                        if (clippedLine.Count == 0 || clippedLine[clippedLine.Count - 1] != p1)
                            clippedLine.Add(p1);
                        clippedLine.Add(p2);
                    }
                    else
                    {
                        // Реализация простого отсечения - можно улучшить алгоритмом Коэна-Сазерленда
                        if (clipRectangle.Value.Contains(p1))
                        {
                            if (clippedLine.Count == 0 || clippedLine[clippedLine.Count - 1] != p1)
                                clippedLine.Add(p1);
                        }

                        if (clipRectangle.Value.Contains(p2))
                        {
                            clippedLine.Add(p2);
                        }
                    }
                }

                if (clippedLine.Count > 1)
                {
                    g.DrawLines(Pens.Black, clippedLine.ToArray());
                }
            }
            else
            {
                // Если нет отсечения, рисуем всю линию
                g.DrawLines(Pens.Black, line.ToArray());
            }
        }
    }
}