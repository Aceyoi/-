//namespace WinFormsApp17
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp17
{
    public partial class Form1 : Form
    {
        private const int CELL_SIZE = 8;
        private const int GRID_WIDTH = 120;
        private const int GRID_HEIGHT = 90;

        private float[,] waterGrid = new float[GRID_WIDTH, GRID_HEIGHT];
        private bool[,] obstacles = new bool[GRID_WIDTH, GRID_HEIGHT];
        private bool[,] newObstacles = new bool[GRID_WIDTH, GRID_HEIGHT];

        private Bitmap canvas;
        private bool isDrawing = false;
        private bool simulationRunning = false;
        private Random random = new Random();

        // Параметры воды
        private float waterFlowRate = 0.5f;
        private float waterAddAmount = 0.15f;
        private float maxWaterLevel = 1.8f;

        // Физика воды
        private float flowDownSpeed = 0.25f;
        private float spreadSpeed = 0.15f;
        private float minFlowAmount = 0.01f;

        public Form1()
        {
            InitializeComponent();
            InitializeComponents();
            InitializeSimulation();
        }

        private void InitializeComponents()
        {
            // TrackBar для управления потоком
            TrackBar flowTrackBar = new TrackBar();
            flowTrackBar.Minimum = 1;
            flowTrackBar.Maximum = 10;
            flowTrackBar.Value = 5;
            flowTrackBar.TickFrequency = 1;
            flowTrackBar.Location = new Point(250, 650);
            flowTrackBar.Width = 200;
            flowTrackBar.Scroll += FlowTrackBar_Scroll;
            this.Controls.Add(flowTrackBar);

            Label flowLabel = new Label();
            flowLabel.Text = "Сила потока:";
            flowLabel.Location = new Point(250, 630);
            this.Controls.Add(flowLabel);

            // Кнопка паузы
            Button btnPause = new Button();
            btnPause.Text = "Пауза";
            btnPause.Location = new Point(150, 650);
            btnPause.Click += (s, e) => { simulationRunning = !simulationRunning; btnPause.Text = simulationRunning ? "Пауза" : "Продолжить"; };
            this.Controls.Add(btnPause);
        }

        private void FlowTrackBar_Scroll(object sender, EventArgs e)
        {
            TrackBar bar = (TrackBar)sender;
            waterFlowRate = 0.2f + bar.Value * 0.05f;
            waterAddAmount = 0.1f + bar.Value * 0.02f;
        }

        private void InitializeSimulation()
        {
            canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            ClearAll();
            pictureBox1.Image = canvas;
        }

        private void ClearAll()
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    waterGrid[x, y] = 0;
                    obstacles[x, y] = false;
                    newObstacles[x, y] = false;
                }
            }

            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
            }
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = true;
                UpdateObstacle(e.Location, true);
            }
            else if (e.Button == MouseButtons.Right)
            {
                isDrawing = true;
                UpdateObstacle(e.Location, false); // Удаление препятствий
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                // Определяем какое действие выполнять по текущей кнопке мыши
                bool addObstacle = Control.MouseButtons == MouseButtons.Left;
                UpdateObstacle(e.Location, addObstacle);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrawing = false;
            // Применяем новые препятствия
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    if (newObstacles[x, y])
                    {
                        obstacles[x, y] = true;
                        newObstacles[x, y] = false;
                    }
                }
            }
        }

        private void UpdateObstacle(Point location, bool addObstacle)
        {
            int gridX = location.X / CELL_SIZE;
            int gridY = location.Y / CELL_SIZE;

            if (gridX >= 0 && gridX < GRID_WIDTH && gridY >= 0 && gridY < GRID_HEIGHT)
            {
                if (addObstacle)
                {
                    newObstacles[gridX, gridY] = true;
                    // Если симуляция не запущена, сразу добавляем препятствие
                    if (!simulationRunning) obstacles[gridX, gridY] = true;
                }
                                else
                {
                    // Удаление препятствия
                    obstacles[gridX, gridY] = false;
                    newObstacles[gridX, gridY] = false;
                }
                Render();
            }
        }

        private void btnStart_Click_1(object sender, EventArgs e)
        {
            if (!simulationRunning)
            {
                simulationRunning = true;
                timerWaterFlow.Start();
            }
        }

        private void btnReset_Click_1(object sender, EventArgs e)
        {
            simulationRunning = false;
            timerWaterFlow.Stop();
            ClearAll();
        }

        private void timerWaterFlow_Tick(object sender, EventArgs e)
        {
            SimulateWater();
            Render();
        }

        private void SimulateWater()
        {
            // Добавляем новую воду
            if (random.NextDouble() < waterFlowRate)
            {
                int sourceX = GRID_WIDTH / 2;
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = sourceX + dx;
                    if (x >= 0 && x < GRID_WIDTH && waterGrid[x, 1] < maxWaterLevel)
                    {
                        waterGrid[x, 1] += waterAddAmount * (1 - Math.Abs(dx) * 0.3f);
                    }
                }
            }

            // Создаем временную копию для вычислений
            float[,] newWaterGrid = (float[,])waterGrid.Clone();

            // Обрабатываем сетку снизу вверх для правильного заполнения
            for (int y = GRID_HEIGHT - 2; y >= 0; y--)
            {
                for (int x = 0; x < GRID_WIDTH; x++)
                {
                    if (obstacles[x, y] || newObstacles[x, y]) continue;

                    float currentWater = waterGrid[x, y];
                    if (currentWater <= 0) continue;

                    // Течение вниз
                    if (!obstacles[x, y + 1] && !newObstacles[x, y + 1])
                    {
                        float flow = Math.Min(currentWater, flowDownSpeed);
                        newWaterGrid[x, y] -= flow;
                        newWaterGrid[x, y + 1] += flow;
                        currentWater = newWaterGrid[x, y];
                    }

                    // Заполнение "чаш" - подъем воды вверх при переполнении
                    if (currentWater > 1.0f && y > 0 &&
                        !obstacles[x, y - 1] && !newObstacles[x, y - 1])
                    {
                        float excess = currentWater - 1.0f;
                        newWaterGrid[x, y] -= excess * 0.5f;
                        newWaterGrid[x, y - 1] += excess * 0.5f;
                        currentWater = newWaterGrid[x, y];
                    }

                    // Растекание в стороны
                    if (currentWater > minFlowAmount)
                    {
                        SpreadWater(x, y, -1, currentWater, newWaterGrid); // Влево
                        SpreadWater(x, y, 1, currentWater, newWaterGrid);  // Вправо
                    }
                }
            }

            // Обновляем основную сетку
            waterGrid = newWaterGrid;
        }

        private void SpreadWater(int x, int y, int direction, float currentWater, float[,] grid)
        {
            int nx = x + direction;
            if (nx < 0 || nx >= GRID_WIDTH || obstacles[nx, y] || newObstacles[nx, y]) return;

            float neighborWater = grid[nx, y];
            float delta = currentWater - neighborWater;

            if (delta > minFlowAmount)
            {
                float flow = delta * spreadSpeed;
                flow = Math.Min(flow, flowDownSpeed * 0.8f);

                grid[x, y] -= flow;
                grid[nx, y] += flow;
            }
        }

        private void Render()
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);

                // Рисуем постоянные препятствия
                for (int x = 0; x < GRID_WIDTH; x++)
                {
                    for (int y = 0; y < GRID_HEIGHT; y++)
                    {
                        if (obstacles[x, y])
                        {
                            g.FillRectangle(Brushes.Black, x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                        }
                    }
                }

                // Рисуем новые препятствия (пока рисуем)
                for (int x = 0; x < GRID_WIDTH; x++)
                {
                    for (int y = 0; y < GRID_HEIGHT; y++)
                    {
                        if (newObstacles[x, y])
                        {
                            g.FillRectangle(Brushes.Gray, x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                        }
                    }
                }

                // Рисуем воду с учетом уровня
                for (int x = 0; x < GRID_WIDTH; x++)
                {
                    for (int y = 0; y < GRID_HEIGHT; y++)
                    {
                        float waterAmount = waterGrid[x, y];
                        if (waterAmount > 0)
                        {
                            int alpha = (int)(220 * Math.Min(waterAmount / maxWaterLevel, 1f));
                            int height = (int)(CELL_SIZE * Math.Min(waterAmount, 1f));

                            Color waterColor = Color.FromArgb(alpha, 50, 150, 255);
                            using (Brush waterBrush = new SolidBrush(waterColor))
                            {
                                g.FillRectangle(waterBrush, x * CELL_SIZE, y * CELL_SIZE + (CELL_SIZE - height), CELL_SIZE, height + 5);
                            }
                        }
                    }
                }
            }
            pictureBox1.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ClientSize = new Size(GRID_WIDTH * CELL_SIZE + 40, GRID_HEIGHT * CELL_SIZE + 100);
            pictureBox1.Size = new Size(GRID_WIDTH * CELL_SIZE, GRID_HEIGHT * CELL_SIZE);
        }
    }
}