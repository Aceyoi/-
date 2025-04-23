using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace WinFormsApp16
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Size = new Size(800, 600);

            var openButton = new Button
            {
                Text = "Открыть",
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            openButton.Click += OpenButton_Click;

            this.Controls.Add(openButton);

            pictureBox = new PictureBox
            {
                Location = new Point(10, 50),
                Size = new Size(300, 300),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pictureBox);

            rgbHistogramBox = new PictureBox
            {
                Location = new Point(320, 50),
                Size = new Size(450, 150),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(rgbHistogramBox);

            rHistogramBox = new PictureBox
            {
                Location = new Point(320, 210),
                Size = new Size(450, 100),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(rHistogramBox);

            gHistogramBox = new PictureBox
            {
                Location = new Point(320, 320),
                Size = new Size(450, 100),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(gHistogramBox);

            bHistogramBox = new PictureBox
            {
                Location = new Point(320, 430),
                Size = new Size(450, 100),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(bHistogramBox);
        }

        private PictureBox pictureBox;
        private PictureBox rgbHistogramBox;
        private PictureBox rHistogramBox;
        private PictureBox gHistogramBox;
        private PictureBox bHistogramBox;

        private void OpenButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Bitmap image = new Bitmap(openFileDialog.FileName);
                        pictureBox.Image = image;
                        GenerateHistograms(image);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void GenerateHistograms(Bitmap image)
        {
            int[] rHist = new int[256];
            int[] gHist = new int[256];
            int[] bHist = new int[256];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    rHist[pixel.R]++;
                    gHist[pixel.G]++;
                    bHist[pixel.B]++;
                }
            }

            int maxR = rHist.Max();
            int maxG = gHist.Max();
            int maxB = bHist.Max();
            int maxAll = new[] { maxR, maxG, maxB }.Max();

            rgbHistogramBox.Image = DrawCombinedHistogram(rHist, gHist, bHist, maxAll, rgbHistogramBox.Width, rgbHistogramBox.Height);
            rHistogramBox.Image = DrawSingleHistogram(rHist, maxR, Color.Red, rHistogramBox.Width, rHistogramBox.Height);
            gHistogramBox.Image = DrawSingleHistogram(gHist, maxG, Color.Green, gHistogramBox.Width, gHistogramBox.Height);
            bHistogramBox.Image = DrawSingleHistogram(bHist, maxB, Color.Blue, bHistogramBox.Width, bHistogramBox.Height);
        }

        private Bitmap DrawCombinedHistogram(int[] rHist, int[] gHist, int[] bHist, int maxValue, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                float xScale = (float)width / 256;
                float yScale = (float)height / maxValue;

                for (int i = 0; i < 255; i++)
                {
                    float rHeight = rHist[i] * yScale;
                    g.DrawLine(Pens.Red,
                        i * xScale, height,
                        i * xScale, height - rHeight);

                    float gHeight = gHist[i] * yScale;
                    g.DrawLine(Pens.Green,
                        i * xScale, height,
                        i * xScale, height - gHeight);

                    float bHeight = bHist[i] * yScale;
                    g.DrawLine(Pens.Blue,
                        i * xScale, height,
                        i * xScale, height - bHeight);
                }

                g.DrawString("RGB", SystemFonts.DefaultFont, Brushes.Black, 5, 5);
            }
            return bmp;
        }

        private Bitmap DrawSingleHistogram(int[] hist, int maxValue, Color color, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                float xScale = (float)width / 256;
                float yScale = (float)height / maxValue;

                using (Pen pen = new Pen(color))
                {
                    for (int i = 0; i < 255; i++)
                    {
                        float histHeight = hist[i] * yScale;
                        g.DrawLine(pen,
                            i * xScale, height,
                            i * xScale, height - histHeight);
                    }
                }

                string channelName = color.R == 255 ? "Red" : color.G == 255 ? "Green" : "Blue";
                g.DrawString($"{channelName} ", SystemFonts.DefaultFont, Brushes.Black, 5, 5);
            }
            return bmp;
        }
    }
}