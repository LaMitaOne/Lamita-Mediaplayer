using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LaMita
{
    public class WaterEffectControl : Panel
    {
        private Timer effectTimer;
        private System.ComponentModel.IContainer components;

        private Bitmap _bmp;
        private short[,,] _waves;
        private int _waveWidth;
        private int _waveHeight;
        private int _activeBuffer = 0;
        private bool _weHaveWaves;
        private int _bmpHeight, _bmpWidth;
        private byte[] _bmpBytes;
        private BitmapData _bmpBitmapData;
        private int _scale;


        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.effectTimer = new System.Windows.Forms.Timer(this.components);
            this.effectTimer.Tick += new System.EventHandler(this.effectTimer_Tick);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.WaterEffectControl_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WaterEffectControl_MouseMove);
        }

        public WaterEffectControl()
        {
            InitializeComponent();
            effectTimer.Enabled = true;
            effectTimer.Interval = 49;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            this.BackColor = Color.White;
            _weHaveWaves = false;
            _scale = 1;

        }

        public WaterEffectControl(Bitmap bmp) : this()
        {
            this.ImageBitmap = bmp;
        }

        public void effectTimer_Tick(object sender, System.EventArgs e)
        {
            if (this.Visible)
            {
                Invalidate();
                ProcessWaves();
                Random rx = new Random();
                int zz = rx.Next(1, 3);
                //Random drops
                int xx = rx.Next(0, Width);
                int yy = rx.Next(0, Height);
                int realX = (int)((xx / (double)this.ClientRectangle.Width) * _waveWidth);
                int realY = (int)((yy / (double)this.ClientRectangle.Height) * _waveHeight);
                PutDrop(realX, realY, 100);
            }
        }


        public void WaterEffectControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            using (Bitmap tmp = (Bitmap)_bmp.Clone())
            {
                int xOffset, yOffset;
                byte alpha;
                if (_weHaveWaves)
                {
                    BitmapData tmpData = tmp.LockBits(new Rectangle(0, 0, _bmpWidth, _bmpHeight), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                    byte[] tmpBytes = new Byte[_bmpWidth * _bmpHeight * 4];
                    Marshal.Copy(tmpData.Scan0, tmpBytes, 0, _bmpWidth * _bmpHeight * 4);
                    for (int x = 1; x < _bmpWidth - 1; x++)
                    {
                        for (int y = 1; y < _bmpHeight - 1; y++)
                        {
                            int waveX = (int)x >> _scale;
                            int waveY = (int)y >> _scale;
                            if (waveX <= 0) waveX = 1;
                            if (waveY <= 0) waveY = 1;
                            if (waveX >= _waveWidth - 1) waveX = _waveWidth - 2;
                            if (waveY >= _waveHeight - 1) waveY = _waveHeight - 2;
                            xOffset = (_waves[waveX - 1, waveY, _activeBuffer] - _waves[waveX + 1, waveY, _activeBuffer]) >> 3;
                            yOffset = (_waves[waveX, waveY - 1, _activeBuffer] - _waves[waveX, waveY + 1, _activeBuffer]) >> 3;

                            if ((xOffset != 0) || (yOffset != 0))
                            {
                                if (x + xOffset >= _bmpWidth - 1) xOffset = _bmpWidth - x - 1;
                                if (y + yOffset >= _bmpHeight - 1) yOffset = _bmpHeight - y - 1;
                                if (x + xOffset < 0) xOffset = -x;
                                if (y + yOffset < 0) yOffset = -y;
                                alpha = (byte)(200 - xOffset);
                                if (alpha < 0) alpha = 0;
                                if (alpha > 255) alpha = 254;
                                tmpBytes[4 * (x + y * _bmpWidth)] = _bmpBytes[4 * (x + xOffset + (y + yOffset) * _bmpWidth)];
                                tmpBytes[4 * (x + y * _bmpWidth) + 1] = _bmpBytes[4 * (x + xOffset + (y + yOffset) * _bmpWidth) + 1];
                                tmpBytes[4 * (x + y * _bmpWidth) + 2] = _bmpBytes[4 * (x + xOffset + (y + yOffset) * _bmpWidth) + 2];
                                tmpBytes[4 * (x + y * _bmpWidth) + 3] = alpha;
                            }
                        }
                    }
                    Marshal.Copy(tmpBytes, 0, tmpData.Scan0, _bmpWidth * _bmpHeight * 4);
                    tmp.UnlockBits(tmpData);
                }
                e.Graphics.DrawImage(tmp, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);
            }
        }

        private void ProcessWaves()
        {
            int newBuffer = (_activeBuffer == 0) ? 1 : 0;
            bool wavesFound = false;
            for (int x = 1; x < _waveWidth - 1; x++)
            {
                for (int y = 1; y < _waveHeight - 1; y++)
                {
                    _waves[x, y, newBuffer] = (short)(
                                            ((_waves[x - 1, y - 1, _activeBuffer] +
                                            _waves[x, y - 1, _activeBuffer] +
                                            _waves[x + 1, y - 1, _activeBuffer] +
                                            _waves[x - 1, y, _activeBuffer] +
                                            _waves[x + 1, y, _activeBuffer] +
                                            _waves[x - 1, y + 1, _activeBuffer] +
                                            _waves[x, y + 1, _activeBuffer] +
                                            _waves[x + 1, y + 1, _activeBuffer]) >> 2) - _waves[x, y, newBuffer]);
                    if (_waves[x, y, newBuffer] != 0)
                    {
                        _waves[x, y, newBuffer] -= (short)(_waves[x, y, newBuffer] >> 4);
                        wavesFound = true;
                    }
                }
            }
            _weHaveWaves = wavesFound;
            _activeBuffer = newBuffer;
        }


        private void PutDrop(int x, int y, short height)
        {
            _weHaveWaves = true;
            int radius = 5;
            double dist;
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    if (((x + i >= 0) && (x + i < _waveWidth - 1)) && ((y + j >= 0) && (y + j < _waveHeight - 1)))
                    {
                        dist = Math.Sqrt(i * i + j * j);
                        if (dist < radius)
                            _waves[x + i, y + j, _activeBuffer] = (short)(Math.Cos(dist * Math.PI / radius) * height);
                    }
                }
            }
        }

        private void WaterEffectControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int realX = (int)((e.X / (double)this.ClientRectangle.Width) * _waveWidth);
            int realY = (int)((e.Y / (double)this.ClientRectangle.Height) * _waveHeight);
            PutDrop(realX, realY, 100);
        }


        #region Properties
        public Bitmap ImageBitmap
        {
            get { return _bmp; }
            set
            {
                _bmp = value;
                _bmpHeight = _bmp.Height;
                _bmpWidth = _bmp.Width;
                _waveWidth = _bmpWidth >> _scale;
                _waveHeight = _bmpHeight >> _scale;
                _waves = new Int16[_waveWidth, _waveHeight, 2];
                _bmpBytes = new Byte[_bmpWidth * _bmpHeight * 4];
                _bmpBitmapData = _bmp.LockBits(new Rectangle(0, 0, _bmpWidth, _bmpHeight), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                Marshal.Copy(_bmpBitmapData.Scan0, _bmpBytes, 0, _bmpWidth * _bmpHeight * 4);
                effectTimer.Enabled = true;
            }
        }

        #endregion
    }
}
