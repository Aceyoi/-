namespace WinFormsApp8
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            btnFill = new Button();
            btnDraw = new Button();
            btnAlgorithm = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(12, 70);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(776, 368);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // btnFill
            // 
            btnFill.Location = new Point(365, 12);
            btnFill.Name = "btnFill";
            btnFill.Size = new Size(85, 23);
            btnFill.TabIndex = 2;
            btnFill.Text = "Заливка";
            btnFill.UseVisualStyleBackColor = true;
            // 
            // btnDraw
            // 
            btnDraw.Location = new Point(365, 41);
            btnDraw.Name = "btnDraw";
            btnDraw.Size = new Size(85, 23);
            btnDraw.TabIndex = 3;
            btnDraw.Text = "Рисование";
            btnDraw.UseVisualStyleBackColor = true;
            // 
            // btnAlgorithm
            // 
            btnAlgorithm.Location = new Point(456, 12);
            btnAlgorithm.Name = "btnAlgorithm";
            btnAlgorithm.Size = new Size(85, 23);
            btnAlgorithm.TabIndex = 4;
            btnAlgorithm.Text = "8 алгоритм";
            btnAlgorithm.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnAlgorithm);
            Controls.Add(btnDraw);
            Controls.Add(btnFill);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private Button btnFill;
        private Button btnDraw;
        private Button btnAlgorithm;
    }
}
