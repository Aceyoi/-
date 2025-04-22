using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WinFormsApp15
{
    public partial class Form1 : Form
    {
        private Bitmap _texture;
        private float _textureScale = 1.0f;
        private PointF[] _triangleVertices = new PointF[3];
        private PointF[] _textureCoords = new PointF[3];

        public Form1()
        {
            InitializeComponent();
            this.Text = "���������������� �����������";
            this.DoubleBuffered = true;
            this.ClientSize = new Size(800, 600);

            // ������������� ��������
            InitializeTexture();

            // ��������� ������ ������������
            SetupTriangle();

            // ���������� ��������� ����������
            AddControls();
        }

        private void InitializeTexture()
        {
            try
            {
                _texture = new Bitmap("skin.png");
            }
            catch
            {
                // ������� �������� ��������, ���� �������� �� �������
                _texture = new Bitmap(256, 256);
                using (var g = Graphics.FromImage(_texture))
                {
                    g.Clear(Color.LightBlue);
                    for (int i = 0; i < 256; i += 32)
                    {
                        g.DrawLine(Pens.Black, i, 0, i, 255);
                        g.DrawLine(Pens.Black, 0, i, 255, i);
                    }
                    g.DrawRectangle(Pens.Red, 0, 0, 255, 255);
                }
            }
        }

        private void SetupTriangle()
        {
            // ������� ������������ (�������� ����������)
            _triangleVertices[0] = new PointF(200, 100);
            _triangleVertices[1] = new PointF(600, 150);
            _triangleVertices[2] = new PointF(400, 500);

            // ���������� ���������� (��������������� 0-1)
            _textureCoords[0] = new PointF(0, 0);   // ������� ����� ���� ��������
            _textureCoords[1] = new PointF(1, 0);   // ������� ������ ����
            _textureCoords[2] = new PointF(0.5f, 1); // �������� ������� ����
        }

        private void AddControls()
        {
            // TrackBar ��� ��������������� ��������
            var scaleTrackBar = new TrackBar
            {
                Minimum = 10,
                Maximum = 200,
                Value = 100,
                Dock = DockStyle.Bottom,
                TickFrequency = 10
            };
            scaleTrackBar.ValueChanged += (s, e) =>
            {
                _textureScale = scaleTrackBar.Value / 100f;
                this.Invalidate();
            };

            // ������ ��� ������ ��������
            var resetButton = new Button
            {
                Text = "�������� �������",
                Dock = DockStyle.Bottom
            };
            resetButton.Click += (s, e) =>
            {
                scaleTrackBar.Value = 100;
                _textureScale = 1.0f;
                this.Invalidate();
            };

            this.Controls.Add(resetButton);
            this.Controls.Add(scaleTrackBar);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // ������� ���
            e.Graphics.Clear(Color.White);

            // ������� ���������������� ��������
            var scaledTexture = GetScaledTexture();

            // ������ ���������������� �����������
            DrawTexturedTriangle(e.Graphics, scaledTexture);

            // ������� ����������
            e.Graphics.DrawString($"������� ��������: {_textureScale:F2}",
                SystemFonts.DefaultFont, Brushes.Black, 10, 10);

            scaledTexture.Dispose();
        }

        private Bitmap GetScaledTexture()
        {
            int newWidth = (int)(_texture.Width * _textureScale);
            int newHeight = (int)(_texture.Height * _textureScale);

            var scaledTexture = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(scaledTexture))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(_texture, 0, 0, newWidth, newHeight);
            }
            return scaledTexture;
        }

        private void DrawTexturedTriangle(Graphics g, Bitmap texture)
        {
            // ������� ���������������� �����
            using (var textureBrush = new TextureBrush(texture, WrapMode.Tile))
            {
                // ������� ����������� ����� ��� ������������ ���������������� ���������
                using (var gradientBrush = new LinearGradientBrush(
                    new Point(0, 0), new Point(150, 150),
                    Color.FromArgb(100, Color.Red), Color.FromArgb(100, Color.Blue)))
                {
                    // ������� ���� ��� ������������
                    using (var path = new GraphicsPath())
                    {
                        path.AddPolygon(_triangleVertices);

                        // ��������� �������� � ��������������� ���������
                        var transform = GetTextureTransform(texture);
                        textureBrush.Transform = transform;

                        // ������ ���������������� �����������
                        g.FillPath(textureBrush, path);

                        // ������������� ������ �������
                        g.DrawPath(Pens.Black, path);
                    }
                }
            }
        }

        private Matrix GetTextureTransform(Bitmap texture)
        {
            // ������� ������� �������������� ��� ���������� ���������
            var transform = new Matrix();

            // ����������� ���������� ���������� � ��������
            PointF[] destPoints = _triangleVertices;
            PointF[] srcPoints = new PointF[3];

            for (int i = 0; i < 3; i++)
            {
                srcPoints[i] = new PointF(
                    _textureCoords[i].X * texture.Width,
                    _textureCoords[i].Y * texture.Height);
            }

            // ������������� ��������������
            transform = new Matrix(
                new RectangleF(0, 0, texture.Width, texture.Height),
                destPoints);

            return transform;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        _texture?.Dispose();
        //        _renderTarget?.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}