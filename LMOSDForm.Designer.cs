
namespace LaMita
{
    partial class LMOSDForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LMOSDForm));
            this.lblOSDText = new System.Windows.Forms.Label();
            this.Hidetimer = new System.Windows.Forms.Timer(this.components);
            this.pbOSDCover = new System.Windows.Forms.PictureBox();
            this.ShowTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pbOSDCover)).BeginInit();
            this.SuspendLayout();
            // 
            // lblOSDText
            // 
            this.lblOSDText.AutoEllipsis = true;
            this.lblOSDText.BackColor = System.Drawing.Color.Transparent;
            this.lblOSDText.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOSDText.ForeColor = System.Drawing.Color.White;
            this.lblOSDText.Location = new System.Drawing.Point(187, 0);
            this.lblOSDText.Name = "lblOSDText";
            this.lblOSDText.Size = new System.Drawing.Size(358, 126);
            this.lblOSDText.TabIndex = 0;
            this.lblOSDText.Text = "OSD";
            this.lblOSDText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblOSDText.Click += new System.EventHandler(this.lblOSDText_Click);
            // 
            // Hidetimer
            // 
            this.Hidetimer.Interval = 3500;
            this.Hidetimer.Tick += new System.EventHandler(this.Hidetimer_Tick);
            // 
            // pbOSDCover
            // 
            this.pbOSDCover.BackgroundImage = global::LaMita.Properties.Resources.coverWide;
            this.pbOSDCover.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbOSDCover.Location = new System.Drawing.Point(0, 0);
            this.pbOSDCover.Name = "pbOSDCover";
            this.pbOSDCover.Size = new System.Drawing.Size(186, 126);
            this.pbOSDCover.TabIndex = 1;
            this.pbOSDCover.TabStop = false;
            // 
            // ShowTimer
            // 
            this.ShowTimer.Interval = 50;
            this.ShowTimer.Tick += new System.EventHandler(this.ShowTimer_Tick);
            // 
            // LMOSDForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(550, 127);
            this.Controls.Add(this.pbOSDCover);
            this.Controls.Add(this.lblOSDText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "LMOSDForm";
            this.Opacity = 0.9D;
            this.Text = "LMOSDForm";
            this.Load += new System.EventHandler(this.LMOSDForm_Load);
            this.Shown += new System.EventHandler(this.LMOSDForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pbOSDCover)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblOSDText;
        public System.Windows.Forms.Timer Hidetimer;
        public System.Windows.Forms.PictureBox pbOSDCover;
        public System.Windows.Forms.Timer ShowTimer;
    }
}