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

            private PictureBox pictureBox;
            private PictureBox rgbHistogramBox;
            private PictureBox rHistogramBox;
            private PictureBox gHistogramBox;
            private PictureBox bHistogramBox;
            private PictureBox rUserHistogramBox;
            private PictureBox gUserHistogramBox;
            private PictureBox bUserHistogramBox;
            private Button applyButton;

            private int[] rUserHist = new int[256];
            private int[] gUserHist = new int[256];
            private int[] bUserHist = new int[256];

            private bool isDrawingR = false;
            private bool isDrawingG = false;
            private bool isDrawingB = false;

            private void InitializeUI()
            {
                this.Size = new Size(1500, 800);

                var openButton = new Button
                {
                    Text = "Открыть",
                    Location = new Point(10, 10),
                    Size = new Size(100, 30)
                };
                openButton.Click += OpenButton_Click;
                this.Controls.Add(openButton);

                applyButton = new Button
                {
                    Text = "Изменить",
                    Location = new Point(120, 10),
                    Size = new Size(100, 30),
                    Enabled = false
                };
                applyButton.Click += ApplyButton_Click;
                this.Controls.Add(applyButton);

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

                rUserHistogramBox = new PictureBox
                {
                    Location = new Point(820, 210),
                    Size = new Size(450, 100),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };
                rUserHistogramBox.MouseDown += (s, e) => { isDrawingR = true; UpdateUserHistogram(e.X, e.Y, rUserHist, rUserHistogramBox); };
                rUserHistogramBox.MouseMove += (s, e) => { if (isDrawingR) UpdateUserHistogram(e.X, e.Y, rUserHist, rUserHistogramBox); };
                rUserHistogramBox.MouseUp += (s, e) => { isDrawingR = false; };
                this.Controls.Add(rUserHistogramBox);

                gUserHistogramBox = new PictureBox
                {
                    Location = new Point(820, 320),
                    Size = new Size(450, 100),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };
                gUserHistogramBox.MouseDown += (s, e) => { isDrawingG = true; UpdateUserHistogram(e.X, e.Y, gUserHist, gUserHistogramBox); };
                gUserHistogramBox.MouseMove += (s, e) => { if (isDrawingG) UpdateUserHistogram(e.X, e.Y, gUserHist, gUserHistogramBox); };
                gUserHistogramBox.MouseUp += (s, e) => { isDrawingG = false; };
                this.Controls.Add(gUserHistogramBox);

                bUserHistogramBox = new PictureBox
                {
                    Location = new Point(820, 430),
                    Size = new Size(450, 100),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };
                bUserHistogramBox.MouseDown += (s, e) => { isDrawingB = true; UpdateUserHistogram(e.X, e.Y, bUserHist, bUserHistogramBox); };
                bUserHistogramBox.MouseMove += (s, e) => { if (isDrawingB) UpdateUserHistogram(e.X, e.Y, bUserHist, bUserHistogramBox); };
                bUserHistogramBox.MouseUp += (s, e) => { isDrawingB = false; };
                this.Controls.Add(bUserHistogramBox);
            }

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
                            InitializeUserHistograms();
                            applyButton.Enabled = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка загрузки картинки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

            private void InitializeUserHistograms()
            {
                for (int i = 0; i < 256; i++)
                {
                    rUserHist[i] = i;
                    gUserHist[i] = i;
                    bUserHist[i] = i;
                }

                DrawUserHistogram(rUserHist, rUserHistogramBox, Color.Red);
                DrawUserHistogram(gUserHist, gUserHistogramBox, Color.Green);
                DrawUserHistogram(bUserHist, bUserHistogramBox, Color.Blue);
            }

            private void UpdateUserHistogram(int x, int y, int[] hist, PictureBox box)
            {
                float xScale = (float)box.Width / 256;
                int index = (int)(x / xScale);
                if (index < 0) index = 0;
                if (index > 255) index = 255;

                float yScale = (float)box.Height / 255;
                int value = 255 - (int)(y / yScale);
                if (value < 0) value = 0;
                if (value > 255) value = 255;

                hist[index] = value;
                DrawUserHistogram(hist, box, box == rUserHistogramBox ? Color.Red :
                                  box == gUserHistogramBox ? Color.Green : Color.Blue);
            }

            private void DrawUserHistogram(int[] hist, PictureBox box, Color color)
            {
                Bitmap bmp = new Bitmap(box.Width, box.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);

                    float xScale = (float)box.Width / 256;
                    float yScale = (float)box.Height / 255;

                    using (Pen pen = new Pen(color, 2))
                    {
                        for (int i = 1; i < 256; i++)
                        {
                            g.DrawLine(pen,
                                (i - 1) * xScale, box.Height - hist[i - 1] * yScale,
                                i * xScale, box.Height - hist[i] * yScale);
                        }
                    }
                }
                box.Image = bmp;
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

        private void GenerateHistograms2(Bitmap image)
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

            rgbHistogramBox.Image = DrawCombinedHistogram(rUserHist, gUserHist, bUserHist, 255, rgbHistogramBox.Width, rgbHistogramBox.Height);
            rHistogramBox.Image = DrawSingleHistogram(rUserHist, 255, Color.Red, rHistogramBox.Width, rHistogramBox.Height);
            gHistogramBox.Image = DrawSingleHistogram(gUserHist, 255, Color.Green, gHistogramBox.Width, gHistogramBox.Height);
            bHistogramBox.Image = DrawSingleHistogram(bUserHist, 255, Color.Blue, bHistogramBox.Width, bHistogramBox.Height);
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
                        g.DrawLine(Pens.Red, i * xScale, height, i * xScale, height - rHeight);

                        float gHeight = gHist[i] * yScale;
                        g.DrawLine(Pens.Green, i * xScale, height, i * xScale, height - gHeight);

                        float bHeight = bHist[i] * yScale;
                        g.DrawLine(Pens.Blue, i * xScale, height, i * xScale, height - bHeight);
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
                            g.DrawLine(pen, i * xScale, height, i * xScale, height - histHeight);
                        }
                    }

                    string channelName = color.R == 255 ? "Red" : color.G == 255 ? "Green" : "Blue";
                    g.DrawString($"{channelName}", SystemFonts.DefaultFont, Brushes.Black, 5, 5);
                }
                return bmp;
            }

            private void ApplyButton_Click(object sender, EventArgs e)
            {
                if (pictureBox.Image == null) return;



            Bitmap original = (Bitmap)pictureBox.Image;
                Bitmap modified = new Bitmap(original.Width, original.Height);

                for (int y = 0; y < original.Height; y++)
                {
                    for (int x = 0; x < original.Width; x++)
                    {
                        Color pixel = original.GetPixel(x, y);

                        int r = rUserHist[pixel.R];
                        int g = gUserHist[pixel.G];
                        int b = bUserHist[pixel.B];

                        r = Math.Max(0, Math.Min(255, r));
                        g = Math.Max(0, Math.Min(255, g));
                        b = Math.Max(0, Math.Min(255, b));

                        modified.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }

                pictureBox.Image = modified;
                GenerateHistograms2(modified);
            }
        }
    }