using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace LaMita
{
    public partial class LMOSDForm : Form
    {
        public LMOSDForm()
        {
            InitializeComponent();
        }

        private void LMOSDForm_Load(object sender, EventArgs e)
        {
            TopMost = true;
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width
                                 , Screen.PrimaryScreen.WorkingArea.Height - 220);
        }

        private void LMOSDForm_Shown(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            Hidetimer.Enabled = true;
            this.BringToFront();
        }

        private void Hidetimer_Tick(object sender, EventArgs e)
        {
            Hidetimer.Enabled = false;
            for (int i = 0; i < Width / 5; i++)
            {
                Location = new Point(Location.X + 5, Screen.PrimaryScreen.WorkingArea.Height - 220);
                Thread.Sleep(1);
            }
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height - 220);
            Hide();
        }

        private void lblOSDText_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void ShowTimer_Tick(object sender, EventArgs e)
        {
            ShowTimer.Enabled = false;
            if (Visible == false)
            {
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width
                                 , Screen.PrimaryScreen.WorkingArea.Height - 220);
                Opacity = 0.85;                
                Show();
                for (int i = 0; i < Width / 5; i++)
                {
                    Location = new Point(Location.X - 5, Screen.PrimaryScreen.WorkingArea.Height - 220);
                    Thread.Sleep(1);
                }
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width, Screen.PrimaryScreen.WorkingArea.Height - 220);
                this.BringToFront();
                Hidetimer.Enabled = true;
            }
            else
            {
                //falls schon sichtbar, timer reset
                this.BringToFront();
                Hidetimer.Enabled = false;
                Hidetimer.Enabled = true;
            }
        }
    }
}
