namespace CLHistogramEqualization
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnEqualize = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnEqua = new System.Windows.Forms.Button();
            this.btnHue = new System.Windows.Forms.Button();
            this.btnSat = new System.Windows.Forms.Button();
            this.btnLum = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnEqualize
            // 
            this.btnEqualize.Location = new System.Drawing.Point(119, 13);
            this.btnEqualize.Name = "btnEqualize";
            this.btnEqualize.Size = new System.Drawing.Size(132, 46);
            this.btnEqualize.TabIndex = 1;
            this.btnEqualize.Text = "Equalize (conventional)";
            this.btnEqualize.UseVisualStyleBackColor = true;
            this.btnEqualize.Click += new System.EventHandler(this.btnEqualize_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(12, 12);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(101, 46);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnEqua
            // 
            this.btnEqua.Location = new System.Drawing.Point(257, 12);
            this.btnEqua.Name = "btnEqua";
            this.btnEqua.Size = new System.Drawing.Size(132, 46);
            this.btnEqua.TabIndex = 1;
            this.btnEqua.Text = "Equalize (OpenCL)";
            this.btnEqua.UseVisualStyleBackColor = true;
            this.btnEqua.Click += new System.EventHandler(this.btnEqua_Click);
            // 
            // btnHue
            // 
            this.btnHue.Location = new System.Drawing.Point(395, 13);
            this.btnHue.Name = "btnHue";
            this.btnHue.Size = new System.Drawing.Size(78, 45);
            this.btnHue.TabIndex = 3;
            this.btnHue.Text = "Hue";
            this.btnHue.UseVisualStyleBackColor = true;
            this.btnHue.Click += new System.EventHandler(this.btnHue_Click);
            // 
            // btnSat
            // 
            this.btnSat.Location = new System.Drawing.Point(479, 12);
            this.btnSat.Name = "btnSat";
            this.btnSat.Size = new System.Drawing.Size(78, 45);
            this.btnSat.TabIndex = 3;
            this.btnSat.Text = "Saturation";
            this.btnSat.UseVisualStyleBackColor = true;
            this.btnSat.Click += new System.EventHandler(this.btnSat_Click);
            // 
            // btnLum
            // 
            this.btnLum.Location = new System.Drawing.Point(563, 13);
            this.btnLum.Name = "btnLum";
            this.btnLum.Size = new System.Drawing.Size(78, 45);
            this.btnLum.TabIndex = 3;
            this.btnLum.Text = "Luminance";
            this.btnLum.UseVisualStyleBackColor = true;
            this.btnLum.Click += new System.EventHandler(this.btnLum_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(12, 65);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(635, 443);
            this.panel1.TabIndex = 4;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(626, 437);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 520);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnLum);
            this.Controls.Add(this.btnSat);
            this.Controls.Add(this.btnHue);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnEqua);
            this.Controls.Add(this.btnEqualize);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnEqualize;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnEqua;
        private System.Windows.Forms.Button btnHue;
        private System.Windows.Forms.Button btnSat;
        private System.Windows.Forms.Button btnLum;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

