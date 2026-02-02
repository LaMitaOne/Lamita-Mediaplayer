using LibVLCSharp.Shared;
using AnimatorNS;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace LaMita
{

    public partial class LMPlayer : Form
    {
        //Trackbar gedrückt
        private bool TrackbarChanging = false;

        //Mediaplayer library und Komponente
        public LibVLC VLClib;
        public MediaPlayer VLCMediaplayer;

        //Hilfsvariablen für Form verschieben on mousedown und cursorhide on fullscreen
        private Point m_offset;
        private Point m_Pos;

        //Verweis auf Playlistform
        LMPlaylist LMPlaylistForm = new LMPlaylist();

        //Original VideoWindow und Form sizes und location um zurück vom fullscreen zu kommen
        Point OrgFLocation;
        Point OrgVLocation;
        Size OrgVSize;
        Size OrgFSize;

        //Fullscreen und Transparency
        public bool Fullscreen = false;
        bool TransparencyOFF = false;

        //für Mutebutton
        int lastVolume = 100;

        //Watereffectimage
        public WaterEffectControl waterImg;

        public LMPlayer()
        {
            //VLC Core init
            if (!DesignMode)
            {
                Core.Initialize();
            }

            InitializeComponent();

            //Drag Accept files
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(FDragEnter);
            this.DragDrop += new DragEventHandler(FDragDrop);
        }

        //dragged files vom Explorer annehmen
        void FDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        //dragged files vom Explorer annehmen
        void FDragDrop(object sender, DragEventArgs e)
        {
            //todo - noch für ordner machen
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            int x = 0;
            foreach (string file in files)
            {
                if (LMPlaylistForm.isAudioFile(file) || LMPlaylistForm.isVideoFile(file))
                {
                    LMPlaylistForm.AddnewTitle(file);
                    x++;
                }
            }
            if (x > 0)
                LMPlaylistForm.ShowOSDInfos("" + x + " Titel hinzugefügt");
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            TopMost = true;
            DoubleBuffered = true;
            this.ShowInTaskbar = false;

            //init VLC
            VLClib = new LibVLC();
            VLCMediaplayer = new MediaPlayer(VLClib);

            //Player events zuordnen und weiteres einstellen
            VLCMediaplayer.EndReached += VLC_EndReached;
            VLCMediaplayer.PositionChanged += VLC_PosChanged;
            VLCMediaplayer.EncounteredError += VLC_EncounteredError;
            VLCMediaplayer.Buffering += VLC_Buffering;
            VLCMediaplayer.EnableHardwareDecoding = false;

            //Videofenster einrichten
            VLCVideowindow.Location = pbCover.Location;
            VLCVideowindow.MediaPlayer = VLCMediaplayer;
            VLCVideowindow.Size = pbCover.Size;
            OrgVLocation = VLCVideowindow.Location;
            OrgVSize = VLCVideowindow.Size;

            //events von vlcdll ausschalten und selbst handeln
            VLCMediaplayer.EnableMouseInput = false;
            VLCMediaplayer.EnableKeyInput = false;

            //Form transparent machen
            BackColor = Color.DarkGray;
            TransparencyKey = BackColor;

            //Settings laden
            var MyIni = new IniFile("C:\\temp\\Settings.ini");
            string LastPosX = MyIni.Read("LastPosX");
            string LastPosY = MyIni.Read("LastPosY");
            //todo check ob die letzte position nicht ausserhalb der bildschirme ist
            if (LastPosX != "" && LastPosY != "")
            {
                int x = Int32.Parse(LastPosX);
                int y = Int32.Parse(LastPosY);
                Location = new Point(x, y);
            }
            string LastVolume = MyIni.Read("LastVolume");
            if (LastVolume != "" && LastVolume != "-1")
            {
                tbVolume.Value = Int32.Parse(LastVolume);
                VLCMediaplayer.Volume = tbVolume.Value;
            }
            OrgFLocation = Location;

            //Watereffect Image
            waterImg = new WaterEffectControl();
            waterImg.Location = pbCover.Location;
            waterImg.Size = pbCover.Size;
            waterImg.ImageBitmap = (Bitmap)pbCover.BackgroundImage;
            waterImg.Parent = this;
            waterImg.Height = pbCover.Size.Height;
            waterImg.Width = pbCover.Size.Width;
            waterImg.MouseDown += LMPlayer_MouseDown;
            waterImg.MouseMove += LMPlayer_MouseMove;

            //auf Mouse Wheel reagieren
            this.MouseWheel += new MouseEventHandler(HandleMouseWheel);
        }

        private void HandleMouseWheel(object sender, MouseEventArgs e)
        {
            //Lautstärke ändern mit Mousewheel, nur wenn gerade abgespielt wird, sonst gehts nicht
            if (VLCMediaplayer.State == VLCState.Playing)
                if (e.Delta > 0)
                {
                    if (tbVolume.Value + 5 <= 95)
                        tbVolume.Value = tbVolume.Value + 5;
                }
                else
                {
                    if (tbVolume.Value - 5 >= 5)
                        tbVolume.Value = tbVolume.Value - 5;
                }
        }


        private void VLC_Buffering(object sender, MediaPlayerBufferingEventArgs e)
        {
            //Buffering bei streams
            this.Invoke(new Action(() =>
            {
                if (LMPlaylistForm.StreamPlaying)
                    LMPlaylistForm.ShowOSDInfos(LMPlaylistForm.ActualStream + "\nBuffering...");
            }));
        }
        private void VLC_EndReached(object sender, EventArgs e)
        {
            //Ende des titels erreicht, zum nächsten springen
            this.Invoke(new Action(() =>
            {
                //Bugfix, ohne Timer hängt sich das ganze immer auf, egal was man macht...
                //aber so gehts...
                if (LMPlaylistForm.StreamPlaying == false)
                    timerPlayNext.Enabled = true;
            }));
        }

        private void PlayNextTimer_Tick(object sender, EventArgs e)
        {
            //Bugfix, ohne Timer hängt sich das ganze immer auf
            timerPlayNext.Enabled = false;
            if (LMPlaylistForm.RepeatMode == 3)
                //TitleRepeat
                LMPlaylistForm.ctrlPlay_Click(null, null);
            else
                //andere RepeatModes oder normal Playback
                LMPlaylistForm.ctrlNext_Click(null, null);
        }

        private void VLC_EncounteredError(object sender, EventArgs e)
        {
            //Fehler beim Abspielen
            this.Invoke(new Action(() =>
            {
                LMPlaylistForm.LoadingNewFile = false;
                LMPlaylistForm.ShowOSDInfos("Fehler beim Abspielen des Titels");
            }));
        }

        private void VLC_PosChanged(object sender, EventArgs e)
        {
            if (LMPlaylistForm.LoadingNewFile == false)
            {
                try  //todo trackbar passt noch nich so wirklich
                {
                    //Position in Trackbar setzen, wenn nicht gerade mousedown
                    if (TrackbarChanging == false)
                    {
                        float PosX = VLCMediaplayer.Position * 100;
                        if (PosX < tbPosition.Maximum)
                            tbPosition.Invoke(new Action(() =>
                            { //ohne Invoke Thread fehler
                                tbPosition.Value = (int)PosX;
                            }));
                    }

                    //aktuelle Zeit anzeigen
                    TimeSpan ts = TimeSpan.FromMilliseconds((int)VLCMediaplayer.Time);
                    string output = ts.ToString(@"hh\:mm\:ss");

                    //ohne Invoke Thread fehler
                    lblPos.Invoke(new Action(() =>
                    {
                        lblPos.Text = output;
                    }));
                }
                catch
                {
                    //log
                }
            }
        }


        private void tbPosition_MouseDown(object sender, MouseEventArgs e)
        {
            //wenn gedrückt dann nicht automatisch setzen nach Playposition
            TrackbarChanging = true;
        }

        private void tbPosition_MouseUp(object sender, MouseEventArgs e)
        {
            //wieder automatischer progress
            TrackbarChanging = false;
        }

        private void tbPosition_ValueChanged(object sender, EventArgs e)
        {
            if (LMPlaylistForm.LoadingNewFile == false)
            {
                timerHideControls.Enabled = false;
                try
                {
                    //position im aktuell gespielten titel ändern  -- manchmal aufhänger völlig ohne grund
                    float NewPos = (float)tbPosition.Value / 100;
                    if (VLCMediaplayer.State == VLCState.Playing || VLCMediaplayer.State == VLCState.Paused)
                        if (TrackbarChanging == true)
                            ThreadPool.QueueUserWorkItem(_ => VLCMediaplayer.Position = NewPos);
                }
                finally
                {
                    timerHideControls.Enabled = true;
                }
            }
        }


        private void LMPlayer_Shown(object sender, EventArgs e)
        {
            //Playlistform anzeigen                   
            LMPlaylistForm.Show();

            //Playlistform Position und Größe setzen
            LMPlaylistForm.Size = new Size(400, Screen.PrimaryScreen.WorkingArea.Height);
            LMPlaylistForm.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - LMPlaylistForm.Width, 0);
            LMPlaylistForm.LMPlayerform = this;
            TimerShow.Enabled = true;

            //TrayIcon anzeigen
            TrayIcon.Visible = true;
        }

        private void LMPlayer_MouseDown(object sender, MouseEventArgs e)
        {
            //position für mousemove verschieben
            if (Fullscreen == false)
                m_offset = new Point(-e.X, -e.Y);
        }

        private void LMPlayer_MouseMove(object sender, MouseEventArgs e)
        {
            //verschieben on mousemove
            if (e.Button == MouseButtons.Left && Fullscreen == false)
            {
                m_Pos = Control.MousePosition;
                m_Pos.Offset(m_offset.X, m_offset.Y);
                Location = m_Pos;
            }
        }

        public void poiClose_Click(object sender, EventArgs e)
        {
            //Programm beenden

            //Einstellungen und Playlist speichern
            LMPlaylistForm.SaveSettingsNow();

            //Player stoppen
            ThreadPool.QueueUserWorkItem(_ => VLCMediaplayer.Stop());

            //Programm kill - etwas unschön, aber umgeht mögliche probleme mit der dll
            Process proc = Process.GetCurrentProcess();
            proc.Kill();
        }

        private void ctrlPlay_Click(object sender, EventArgs e)
        {
            LMPlaylistForm.ctrlPlay_Click(null, null);
        }

        private void ctrlStop_Click(object sender, EventArgs e)
        {
            LMPlaylistForm.ctrlStop_Click(null, null);
        }

        private void ctrlPause_Click(object sender, EventArgs e)
        {
            LMPlaylistForm.ctrlPause_Click(null, null);
        }

        private void ctrlPrev_Click(object sender, EventArgs e)
        {
            LMPlaylistForm.ctrlPrev_Click(null, null);
        }

        private void ctrlNext_Click(object sender, EventArgs e)
        {
            LMPlaylistForm.ctrlNext_Click(null, null);
        }

        public void VLCVideowindow_DoubleClick(object sender, EventArgs e)
        {
            //Fullscreen
            if (Fullscreen == false)
            {
                Fullscreen = true;
                OrgFSize = Size;
                OrgVSize = VLCVideowindow.Size;
                OrgFLocation = Location;
                OrgVLocation = VLCVideowindow.Location;
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Left,
                                     Screen.PrimaryScreen.WorkingArea.Top);
                //todo - für multimonitor noch erweitern
                Size = new Size(Screen.PrimaryScreen.WorkingArea.Width,
                                Screen.PrimaryScreen.WorkingArea.Height);
                VLCVideowindow.Location = new Point(0, 0);
                VLCVideowindow.Size = Size;
                TopMost = false;
                pnlControls.Hide();
                btnClickThrough.Hide();
                //btnTransparency.Hide();
                pnlControls.Location = new Point((VLCVideowindow.Size.Width / 2) - (pnlControls.Width / 2)
                                          , VLCVideowindow.Size.Height - pnlControls.Size.Height);
                Opacity = 1;
                if (LMPlaylistForm.StreamPlaying == false)
                {
                    FileInfo fx = new FileInfo(@LMPlaylistForm.ActualFile);
                    string ext = fx.Extension;
                    string Ftext = fx.Name;
                    Ftext = Ftext.Replace(ext, "");
                    LMPlaylistForm.ShowOSDInfos(Ftext);
                }
                else
                    LMPlaylistForm.ShowOSDInfos(LMPlaylistForm.ActualStream);
            }
            else
            {
                Fullscreen = false;
                Location = OrgFLocation;
                Size = OrgFSize;
                VLCVideowindow.Location = OrgVLocation;
                VLCVideowindow.Size = OrgVSize;
                TopMost = true;
                if (TransparencyOFF == false)
                    Opacity = 0.80;
                btnTransparency.Show();
                btnClickThrough.Show();
                pnlControls.Location = new Point(5, pbCover.Size.Height
                                                    - pnlControls.Size.Height + 8);
                pnlControls.Show();
                pnlControls.BringToFront();
            }
        }

        bool isInsideRect(double x1, double y1, double x2, double y2, double px, double py)
        {
            return px >= x1 && px <= x2 && py >= y1 && py <= y2;
        }

        private void HideControlsTimer_Tick(object sender, EventArgs e)
        {
            timerHideControls.Enabled = false;
            if (isInsideRect(Location.X, Location.Y, Location.X + Width, Location.Y + Height, MousePosition.X, MousePosition.Y))
            {
                //Wenn kein Fullscreen, oder Fullscreen aber Mouse in der Nähe der Controls in der mitte unten
                if (Fullscreen == false || (isInsideRect(pnlControls.Location.X - 100, pnlControls.Location.Y - 100,
                    pnlControls.Location.X + pnlControls.Size.Width + 100,
                    pnlControls.Location.Y + Height, MousePosition.X, MousePosition.Y)))
                {
                    if (pnlControls.Visible == false)
                    {
                        if (LMPlaylistForm.isClickThrough == false)
                        {
                            //Controls einblenden
                            LMPlaylistForm.animator1.AnimationType = AnimationType.Scale;
                            LMPlaylistForm.animator1.Show(pnlControls);
                            LMPlaylistForm.animator1.WaitAllAnimations();
                        }
                        if (Fullscreen)
                        {
                            if (LMPlaylistForm.StreamPlaying == false)
                            {
                                //OSD mit titel anzeigen
                                FileInfo fx = new FileInfo(@LMPlaylistForm.ActualFile);
                                string ext = fx.Extension;
                                string Ftext = fx.Name;
                                Ftext = Ftext.Replace(ext, "");
                                LMPlaylistForm.ShowOSDInfos(Ftext);
                            }
                            else
                                LMPlaylistForm.ShowOSDInfos(LMPlaylistForm.ActualStream);
                        }
                    }
                }
                else if (Fullscreen && MousePosition.Y < (Height - 110))
                {
                    if (pnlControls.Visible)
                    {
                        //Controls ausblenden
                        LMPlaylistForm.animator1.AnimationType = AnimationType.Scale;
                        LMPlaylistForm.animator1.Hide(pnlControls);
                        LMPlaylistForm.animator1.WaitAllAnimations();
                    }
                }
            }
            else
            {
                if (pnlControls.Visible)
                {
                    //Controls ausblenden
                    LMPlaylistForm.animator1.AnimationType = AnimationType.Scale;
                    LMPlaylistForm.animator1.Hide(pnlControls);
                    LMPlaylistForm.animator1.WaitAllAnimations();
                }
            }
            timerHideControls.Enabled = true;
        }


        private void TimerShow_Tick(object sender, EventArgs e)
        {
            //fix, sonst sieht man am Anfang kurz graues Fenster mit Title usw... sehr unschön
            TimerShow.Enabled = false;
            Opacity = 0.90;
            LMPlaylistForm.Opacity = 0.80;
            TimerShow.Dispose(); //kann weg, wird nie wieder benutzt
        }

        private void popupMainMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            popupMainMenu.BackColor = Color.Black;
            popupMainMenu.ForeColor = Color.DarkGray;
            videoSnapshotSpeichernToolStripMenuItem.Visible = VLCVideowindow.Visible;
        }

        private void videoSnapshotSpeichernToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Video Snapshot speichern
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "jpg Datei (*.jpg)|*.jpg";
            sf.RestoreDirectory = true;
            if (sf.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (VLCMediaplayer.State == VLCState.Playing)
                        ThreadPool.QueueUserWorkItem(_ => VLCMediaplayer.Pause());
                    string FileN = sf.FileName;
                    ThreadPool.QueueUserWorkItem(_ => VLCMediaplayer.TakeSnapshot(0, FileN, 0, 0));
                    ThreadPool.QueueUserWorkItem(_ => VLCMediaplayer.Pause());
                }
                catch (Exception ex)
                {
                    LMPlaylistForm.ShowOSDInfos("Snapshot fehlgeschlagen" + ex.Message);
                }
            }
        }

        private void tbVolume_ValueChanged(object sender, EventArgs e)
        {
            if (LMPlaylistForm.LoadingNewFile == false)
            {
                try
                {
                    //Lautstärke setzen -- manchmal aufhänger völlig ohne grund
                    timerHideControls.Enabled = false;
                    LMPlaylistForm.timerSlide.Enabled = false;

                    int Vol = tbVolume.Value;
                    ThreadPool.QueueUserWorkItem(_ => VLCMediaplayer.Volume = Vol);
                }
                finally
                {
                    LMPlaylistForm.timerSlide.Enabled = true;
                    timerHideControls.Enabled = true;
                }
            }
        }

        private void aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Aktuellen Titel zu Favouriten hinzufügen
            LMPlaylistForm.ctrlFav_Click(null, null);
        }

        private void titelZurPlaylisteHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Dateien zur Liste hinzufügen
            LMPlaylistForm.btnAdd_Click(null, null);
        }

        private void sidebarSeiteWechselnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Sidebar auf andere Seite
            LMPlaylistForm.btnSideSwitch_Click(null, null);
        }

        private void playerPositionZurücksetzenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //zB. falls nach änderung der Auflösung das Fenster ausserhalb des Bildschirms liegt
            //kann man es so wieder zurück auf Mainscreen bekommen
            Location = new Point(100, 100);
        }

        private void btnTransparency_Click(object sender, EventArgs e)
        {
            //Transparenz on-off
            if (Opacity == 1)
            {
                TransparencyOFF = false;
                Opacity = 0.90;
                LMPlaylistForm.ShowOSDInfos("Transparenz 90%");
            }
            else if (Opacity == 0.90)
            {
                TransparencyOFF = true;
                Opacity = 0.80;
                LMPlaylistForm.ShowOSDInfos("Transparenz 80%");
            }
            else if (Opacity == 0.80)
            {
                TransparencyOFF = true;
                Opacity = 0.70;
                LMPlaylistForm.ShowOSDInfos("Transparenz 70%");
            }
            else if (Opacity == 0.70)
            {
                TransparencyOFF = true;
                Opacity = 0.60;
                LMPlaylistForm.ShowOSDInfos("Transparenz 60%");
            }
            else if (Opacity == 0.60)
            {
                TransparencyOFF = true;
                Opacity = 0.50;
                LMPlaylistForm.ShowOSDInfos("Transparenz 50%");
            }
            else if (Opacity == 0.50)
            {
                TransparencyOFF = true;
                Opacity = 0.40;
                LMPlaylistForm.ShowOSDInfos("Transparenz 40%");
            }
            else if (Opacity == 0.40)
            {
                TransparencyOFF = true;
                Opacity = 0.30;
                LMPlaylistForm.ShowOSDInfos("Transparenz 30%");
            }
            else if (Opacity == 0.30)
            {
                TransparencyOFF = true;
                Opacity = 0.20;
                LMPlaylistForm.ShowOSDInfos("Transparenz 20%");
            }
            else if (Opacity == 0.20)
            {
                TransparencyOFF = true;
                Opacity = 0.10;
                LMPlaylistForm.ShowOSDInfos("Transparenz 10%");
            }
            else if (Opacity == 0.10)
            {
                TransparencyOFF = true;
                Opacity = 1;
                LMPlaylistForm.ShowOSDInfos("Transparenz deaktiviert");
            }
        }

        private void btnMute_Click(object sender, EventArgs e)
        {
            //Mute on-off
            if (tbVolume.Value == 0)
            {
                btnMute.BackgroundImage = Properties.Resources.Speaker_Volume_16;
                tbVolume.Value = lastVolume;
            }
            else
            {
                btnMute.BackgroundImage = Properties.Resources.Volume_Mute_16;
                lastVolume = tbVolume.Value;
                tbVolume.Value = 0;
            }
        }

        private void btnClickThrough_Click(object sender, EventArgs e)
        {
            //Fenster durchklickbar
            LMPlaylistForm.btnClickThrough_Click(null, null);
        }

        private void ctrlFav_Click(object sender, EventArgs e)
        {
            //aktuellen titel als Favourit hinzufügen
            LMPlaylistForm.ctrlFav_Click(null, null);
        }

        public void btnAspect_Click(object sender, EventArgs e)
        {
            //AspectRatio für Videos ändern
            if (VLCMediaplayer.AspectRatio == null)
            {
                VLCMediaplayer.AspectRatio = "16:9";
                LMPlaylistForm.ShowOSDInfos("Video AspectRatio 16:9");
            }
            else
                if (VLCMediaplayer.AspectRatio == "16:9")
            {
                VLCMediaplayer.AspectRatio = "21:9";
                LMPlaylistForm.ShowOSDInfos("Video AspectRatio 21:9");
            }
            else
                if (VLCMediaplayer.AspectRatio == "21:9")
            {
                VLCMediaplayer.AspectRatio = "4:3";
                LMPlaylistForm.ShowOSDInfos("Video AspectRatio 4:3");
            }
            else
                if (VLCMediaplayer.AspectRatio == "4:3")
            {
                VLCMediaplayer.AspectRatio = null;
                LMPlaylistForm.ShowOSDInfos("Video AspectRatio Automatic");
            }
        }
    }

}


