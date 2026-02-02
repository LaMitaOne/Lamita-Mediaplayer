
namespace LaMita
{
    partial class LMPlayer
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LMPlayer));
            this.VLCVideowindow = new LibVLCSharp.WinForms.VideoView();
            this.popupMainMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.titelZurPlaylisteHinzufügenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoSnapshotSpeichernToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.playerPositionZurücksetzenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sidebarSeiteWechselnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.poiClose = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.btnMute = new System.Windows.Forms.PictureBox();
            this.pnlHideVolumeTrackbarFocusLine = new System.Windows.Forms.Panel();
            this.pnlHidePositionTrackbarFocusLine = new System.Windows.Forms.Panel();
            this.ctrlFav = new System.Windows.Forms.PictureBox();
            this.btnClickThrough = new System.Windows.Forms.PictureBox();
            this.btnTransparency = new System.Windows.Forms.PictureBox();
            this.tbVolume = new System.Windows.Forms.TrackBar();
            this.ctrlNext = new System.Windows.Forms.PictureBox();
            this.ctrlPrev = new System.Windows.Forms.PictureBox();
            this.ctrlPause = new System.Windows.Forms.PictureBox();
            this.ctrlStop = new System.Windows.Forms.PictureBox();
            this.ctrlPlay = new System.Windows.Forms.PictureBox();
            this.lblPos = new System.Windows.Forms.Label();
            this.tbPosition = new System.Windows.Forms.TrackBar();
            this.btnAspect = new System.Windows.Forms.PictureBox();
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.timerPlayNext = new System.Windows.Forms.Timer(this.components);
            this.timerHideControls = new System.Windows.Forms.Timer(this.components);
            this.TimerShow = new System.Windows.Forms.Timer(this.components);
            this.pbCover = new System.Windows.Forms.PictureBox();
            this.pbBackground = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.VLCVideowindow)).BeginInit();
            this.popupMainMenu.SuspendLayout();
            this.pnlControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnMute)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlFav)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClickThrough)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnTransparency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlNext)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlPrev)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlStop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlPlay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosition)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnAspect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCover)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).BeginInit();
            this.SuspendLayout();
            // 
            // VLCVideowindow
            // 
            this.VLCVideowindow.BackColor = System.Drawing.Color.Black;
            this.VLCVideowindow.ContextMenuStrip = this.popupMainMenu;
            this.VLCVideowindow.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.VLCVideowindow.Location = new System.Drawing.Point(14, 14);
            this.VLCVideowindow.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.VLCVideowindow.MediaPlayer = null;
            this.VLCVideowindow.Name = "VLCVideowindow";
            this.VLCVideowindow.Size = new System.Drawing.Size(220, 119);
            this.VLCVideowindow.TabIndex = 20;
            this.VLCVideowindow.Visible = false;
            this.VLCVideowindow.DoubleClick += new System.EventHandler(this.VLCVideowindow_DoubleClick);
            this.VLCVideowindow.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LMPlayer_MouseDown);
            this.VLCVideowindow.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LMPlayer_MouseMove);
            // 
            // popupMainMenu
            // 
            this.popupMainMenu.BackColor = System.Drawing.Color.White;
            this.popupMainMenu.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.popupMainMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.popupMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem,
            this.titelZurPlaylisteHinzufügenToolStripMenuItem,
            this.videoSnapshotSpeichernToolStripMenuItem,
            this.toolStripMenuItem2,
            this.playerPositionZurücksetzenToolStripMenuItem,
            this.sidebarSeiteWechselnToolStripMenuItem,
            this.toolStripMenuItem1,
            this.poiClose});
            this.popupMainMenu.Name = "popupMainMenu";
            this.popupMainMenu.ShowItemToolTips = false;
            this.popupMainMenu.Size = new System.Drawing.Size(340, 160);
            this.popupMainMenu.Opening += new System.ComponentModel.CancelEventHandler(this.popupMainMenu_Opening);
            // 
            // aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem
            // 
            this.aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem.Name = "aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem";
            this.aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem.Size = new System.Drawing.Size(339, 24);
            this.aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem.Text = "aktuellen Titel zu Favouriten hinzufügen";
            this.aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem.Click += new System.EventHandler(this.aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem_Click);
            // 
            // titelZurPlaylisteHinzufügenToolStripMenuItem
            // 
            this.titelZurPlaylisteHinzufügenToolStripMenuItem.Name = "titelZurPlaylisteHinzufügenToolStripMenuItem";
            this.titelZurPlaylisteHinzufügenToolStripMenuItem.Size = new System.Drawing.Size(339, 24);
            this.titelZurPlaylisteHinzufügenToolStripMenuItem.Text = "Dateien zur Playliste hinzufügen";
            this.titelZurPlaylisteHinzufügenToolStripMenuItem.Click += new System.EventHandler(this.titelZurPlaylisteHinzufügenToolStripMenuItem_Click);
            // 
            // videoSnapshotSpeichernToolStripMenuItem
            // 
            this.videoSnapshotSpeichernToolStripMenuItem.Name = "videoSnapshotSpeichernToolStripMenuItem";
            this.videoSnapshotSpeichernToolStripMenuItem.Size = new System.Drawing.Size(339, 24);
            this.videoSnapshotSpeichernToolStripMenuItem.Text = "Video Snapshot speichern";
            this.videoSnapshotSpeichernToolStripMenuItem.Click += new System.EventHandler(this.videoSnapshotSpeichernToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(336, 6);
            // 
            // playerPositionZurücksetzenToolStripMenuItem
            // 
            this.playerPositionZurücksetzenToolStripMenuItem.Name = "playerPositionZurücksetzenToolStripMenuItem";
            this.playerPositionZurücksetzenToolStripMenuItem.Size = new System.Drawing.Size(339, 24);
            this.playerPositionZurücksetzenToolStripMenuItem.Text = "Player Position zurücksetzen";
            this.playerPositionZurücksetzenToolStripMenuItem.Click += new System.EventHandler(this.playerPositionZurücksetzenToolStripMenuItem_Click);
            // 
            // sidebarSeiteWechselnToolStripMenuItem
            // 
            this.sidebarSeiteWechselnToolStripMenuItem.Name = "sidebarSeiteWechselnToolStripMenuItem";
            this.sidebarSeiteWechselnToolStripMenuItem.Size = new System.Drawing.Size(339, 24);
            this.sidebarSeiteWechselnToolStripMenuItem.Text = "Sidebar Seite wechseln";
            this.sidebarSeiteWechselnToolStripMenuItem.Click += new System.EventHandler(this.sidebarSeiteWechselnToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(336, 6);
            // 
            // poiClose
            // 
            this.poiClose.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.poiClose.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.poiClose.Name = "poiClose";
            this.poiClose.Size = new System.Drawing.Size(339, 24);
            this.poiClose.Text = "La\'Mita beenden";
            this.poiClose.Click += new System.EventHandler(this.poiClose_Click);
            // 
            // pnlControls
            // 
            this.pnlControls.BackColor = System.Drawing.Color.Black;
            this.pnlControls.Controls.Add(this.ctrlPlay);
            this.pnlControls.Controls.Add(this.ctrlStop);
            this.pnlControls.Controls.Add(this.ctrlPause);
            this.pnlControls.Controls.Add(this.ctrlPrev);
            this.pnlControls.Controls.Add(this.ctrlNext);
            this.pnlControls.Controls.Add(this.ctrlFav);
            this.pnlControls.Controls.Add(this.btnClickThrough);
            this.pnlControls.Controls.Add(this.btnTransparency);
            this.pnlControls.Controls.Add(this.btnAspect);
            this.pnlControls.Controls.Add(this.btnMute);
            this.pnlControls.Controls.Add(this.pnlHideVolumeTrackbarFocusLine);
            this.pnlControls.Controls.Add(this.pnlHidePositionTrackbarFocusLine);
            this.pnlControls.Controls.Add(this.tbVolume);
            this.pnlControls.Controls.Add(this.lblPos);
            this.pnlControls.Controls.Add(this.tbPosition);
            this.pnlControls.Location = new System.Drawing.Point(8, 228);
            this.pnlControls.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(550, 61);
            this.pnlControls.TabIndex = 24;
            // 
            // btnMute
            // 
            this.btnMute.BackgroundImage = global::LaMita.Properties.Resources.Speaker_Volume_16;
            this.btnMute.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMute.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMute.Location = new System.Drawing.Point(408, 5);
            this.btnMute.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMute.Name = "btnMute";
            this.btnMute.Size = new System.Drawing.Size(32, 32);
            this.btnMute.TabIndex = 37;
            this.btnMute.TabStop = false;
            this.btnMute.Click += new System.EventHandler(this.btnMute_Click);
            // 
            // pnlHideVolumeTrackbarFocusLine
            // 
            this.pnlHideVolumeTrackbarFocusLine.Location = new System.Drawing.Point(436, 8);
            this.pnlHideVolumeTrackbarFocusLine.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlHideVolumeTrackbarFocusLine.Name = "pnlHideVolumeTrackbarFocusLine";
            this.pnlHideVolumeTrackbarFocusLine.Size = new System.Drawing.Size(118, 6);
            this.pnlHideVolumeTrackbarFocusLine.TabIndex = 43;
            // 
            // pnlHidePositionTrackbarFocusLine
            // 
            this.pnlHidePositionTrackbarFocusLine.Location = new System.Drawing.Point(-10, 34);
            this.pnlHidePositionTrackbarFocusLine.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlHidePositionTrackbarFocusLine.Name = "pnlHidePositionTrackbarFocusLine";
            this.pnlHidePositionTrackbarFocusLine.Size = new System.Drawing.Size(575, 5);
            this.pnlHidePositionTrackbarFocusLine.TabIndex = 42;
            // 
            // ctrlFav
            // 
            this.ctrlFav.BackgroundImage = global::LaMita.Properties.Resources.Favorites_16;
            this.ctrlFav.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ctrlFav.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ctrlFav.Location = new System.Drawing.Point(181, 5);
            this.ctrlFav.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ctrlFav.Name = "ctrlFav";
            this.ctrlFav.Size = new System.Drawing.Size(32, 32);
            this.ctrlFav.TabIndex = 40;
            this.ctrlFav.TabStop = false;
            this.ctrlFav.Click += new System.EventHandler(this.ctrlFav_Click);
            // 
            // btnClickThrough
            // 
            this.btnClickThrough.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClickThrough.BackgroundImage = global::LaMita.Properties.Resources.Touch_16;
            this.btnClickThrough.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnClickThrough.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClickThrough.Location = new System.Drawing.Point(295, 5);
            this.btnClickThrough.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClickThrough.Name = "btnClickThrough";
            this.btnClickThrough.Size = new System.Drawing.Size(32, 32);
            this.btnClickThrough.TabIndex = 39;
            this.btnClickThrough.TabStop = false;
            this.btnClickThrough.Click += new System.EventHandler(this.btnClickThrough_Click);
            // 
            // btnTransparency
            // 
            this.btnTransparency.BackgroundImage = global::LaMita.Properties.Resources.View_16;
            this.btnTransparency.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTransparency.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTransparency.Location = new System.Drawing.Point(333, 5);
            this.btnTransparency.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnTransparency.Name = "btnTransparency";
            this.btnTransparency.Size = new System.Drawing.Size(32, 32);
            this.btnTransparency.TabIndex = 36;
            this.btnTransparency.TabStop = false;
            this.btnTransparency.Click += new System.EventHandler(this.btnTransparency_Click);
            // 
            // tbVolume
            // 
            this.tbVolume.AutoSize = false;
            this.tbVolume.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.tbVolume.LargeChange = 10;
            this.tbVolume.Location = new System.Drawing.Point(436, 9);
            this.tbVolume.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbVolume.Maximum = 100;
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(120, 28);
            this.tbVolume.TabIndex = 35;
            this.tbVolume.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbVolume.Value = 100;
            this.tbVolume.ValueChanged += new System.EventHandler(this.tbVolume_ValueChanged);
            // 
            // ctrlNext
            // 
            this.ctrlNext.BackgroundImage = global::LaMita.Properties.Resources.Media_Next_16;
            this.ctrlNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ctrlNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ctrlNext.Location = new System.Drawing.Point(145, 5);
            this.ctrlNext.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ctrlNext.Name = "ctrlNext";
            this.ctrlNext.Size = new System.Drawing.Size(32, 32);
            this.ctrlNext.TabIndex = 34;
            this.ctrlNext.TabStop = false;
            this.ctrlNext.Click += new System.EventHandler(this.ctrlNext_Click);
            // 
            // ctrlPrev
            // 
            this.ctrlPrev.BackgroundImage = global::LaMita.Properties.Resources.Media_Back_16;
            this.ctrlPrev.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ctrlPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ctrlPrev.Location = new System.Drawing.Point(110, 5);
            this.ctrlPrev.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ctrlPrev.Name = "ctrlPrev";
            this.ctrlPrev.Size = new System.Drawing.Size(32, 32);
            this.ctrlPrev.TabIndex = 33;
            this.ctrlPrev.TabStop = false;
            this.ctrlPrev.Click += new System.EventHandler(this.ctrlPrev_Click);
            // 
            // ctrlPause
            // 
            this.ctrlPause.BackgroundImage = global::LaMita.Properties.Resources.Media_Pause_16;
            this.ctrlPause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ctrlPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ctrlPause.Location = new System.Drawing.Point(74, 5);
            this.ctrlPause.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ctrlPause.Name = "ctrlPause";
            this.ctrlPause.Size = new System.Drawing.Size(32, 32);
            this.ctrlPause.TabIndex = 32;
            this.ctrlPause.TabStop = false;
            this.ctrlPause.Click += new System.EventHandler(this.ctrlPause_Click);
            // 
            // ctrlStop
            // 
            this.ctrlStop.BackgroundImage = global::LaMita.Properties.Resources.Media_Stop;
            this.ctrlStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ctrlStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ctrlStop.Location = new System.Drawing.Point(38, 5);
            this.ctrlStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ctrlStop.Name = "ctrlStop";
            this.ctrlStop.Size = new System.Drawing.Size(32, 32);
            this.ctrlStop.TabIndex = 31;
            this.ctrlStop.TabStop = false;
            this.ctrlStop.Click += new System.EventHandler(this.ctrlStop_Click);
            // 
            // ctrlPlay
            // 
            this.ctrlPlay.BackgroundImage = global::LaMita.Properties.Resources.Media_Play_16;
            this.ctrlPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ctrlPlay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ctrlPlay.Location = new System.Drawing.Point(2, 5);
            this.ctrlPlay.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ctrlPlay.Name = "ctrlPlay";
            this.ctrlPlay.Size = new System.Drawing.Size(32, 32);
            this.ctrlPlay.TabIndex = 30;
            this.ctrlPlay.TabStop = false;
            this.ctrlPlay.Click += new System.EventHandler(this.ctrlPlay_Click);
            // 
            // lblPos
            // 
            this.lblPos.AutoSize = true;
            this.lblPos.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblPos.Location = new System.Drawing.Point(218, 9);
            this.lblPos.Name = "lblPos";
            this.lblPos.Size = new System.Drawing.Size(73, 20);
            this.lblPos.TabIndex = 24;
            this.lblPos.Text = "00:00:00";
            // 
            // tbPosition
            // 
            this.tbPosition.AutoSize = false;
            this.tbPosition.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.tbPosition.LargeChange = 10;
            this.tbPosition.Location = new System.Drawing.Point(-6, 36);
            this.tbPosition.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbPosition.Maximum = 100;
            this.tbPosition.Name = "tbPosition";
            this.tbPosition.Size = new System.Drawing.Size(562, 28);
            this.tbPosition.TabIndex = 25;
            this.tbPosition.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbPosition.ValueChanged += new System.EventHandler(this.tbPosition_ValueChanged);
            this.tbPosition.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tbPosition_MouseDown);
            this.tbPosition.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tbPosition_MouseUp);
            // 
            // btnAspect
            // 
            this.btnAspect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAspect.BackgroundImage = global::LaMita.Properties.Resources.Media_Play_01_16;
            this.btnAspect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnAspect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAspect.Location = new System.Drawing.Point(371, 5);
            this.btnAspect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnAspect.Name = "btnAspect";
            this.btnAspect.Size = new System.Drawing.Size(32, 32);
            this.btnAspect.TabIndex = 41;
            this.btnAspect.TabStop = false;
            this.btnAspect.Click += new System.EventHandler(this.btnAspect_Click);
            // 
            // TrayIcon
            // 
            this.TrayIcon.ContextMenuStrip = this.popupMainMenu;
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "La\'Mita Player";
            // 
            // timerPlayNext
            // 
            this.timerPlayNext.Interval = 20;
            this.timerPlayNext.Tick += new System.EventHandler(this.PlayNextTimer_Tick);
            // 
            // timerHideControls
            // 
            this.timerHideControls.Interval = 500;
            this.timerHideControls.Tick += new System.EventHandler(this.HideControlsTimer_Tick);
            // 
            // TimerShow
            // 
            this.TimerShow.Interval = 500;
            this.TimerShow.Tick += new System.EventHandler(this.TimerShow_Tick);
            // 
            // pbCover
            // 
            this.pbCover.BackColor = System.Drawing.Color.Black;
            this.pbCover.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbCover.BackgroundImage")));
            this.pbCover.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbCover.ContextMenuStrip = this.popupMainMenu;
            this.pbCover.Location = new System.Drawing.Point(9, 11);
            this.pbCover.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pbCover.Name = "pbCover";
            this.pbCover.Size = new System.Drawing.Size(548, 276);
            this.pbCover.TabIndex = 0;
            this.pbCover.TabStop = false;
            this.pbCover.Visible = false;
            this.pbCover.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LMPlayer_MouseDown);
            this.pbCover.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LMPlayer_MouseMove);
            // 
            // pbBackground
            // 
            this.pbBackground.BackgroundImage = global::LaMita.Properties.Resources.Panels;
            this.pbBackground.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbBackground.Location = new System.Drawing.Point(-2, -2);
            this.pbBackground.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pbBackground.Name = "pbBackground";
            this.pbBackground.Size = new System.Drawing.Size(575, 308);
            this.pbBackground.TabIndex = 21;
            this.pbBackground.TabStop = false;
            this.pbBackground.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LMPlayer_MouseDown);
            this.pbBackground.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LMPlayer_MouseMove);
            // 
            // LMPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(570, 300);
            this.ContextMenuStrip = this.popupMainMenu;
            this.Controls.Add(this.pnlControls);
            this.Controls.Add(this.VLCVideowindow);
            this.Controls.Add(this.pbCover);
            this.Controls.Add(this.pbBackground);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "LMPlayer";
            this.Opacity = 0D;
            this.Text = "Player";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.LMPlayer_Shown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LMPlayer_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LMPlayer_MouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.VLCVideowindow)).EndInit();
            this.popupMainMenu.ResumeLayout(false);
            this.pnlControls.ResumeLayout(false);
            this.pnlControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnMute)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlFav)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClickThrough)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnTransparency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlNext)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlPrev)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlStop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctrlPlay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPosition)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnAspect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCover)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBackground)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolStripMenuItem poiClose;
        private System.Windows.Forms.PictureBox ctrlNext;
        private System.Windows.Forms.PictureBox ctrlPrev;
        private System.Windows.Forms.PictureBox ctrlPause;
        private System.Windows.Forms.PictureBox ctrlStop;
        private System.Windows.Forms.PictureBox ctrlPlay;
        public System.Windows.Forms.PictureBox pbCover;
        public LibVLCSharp.WinForms.VideoView VLCVideowindow;
        public System.Windows.Forms.Panel pnlControls;
        public System.Windows.Forms.PictureBox pbBackground;
        public System.Windows.Forms.Label lblPos;
        public System.Windows.Forms.ContextMenuStrip popupMainMenu;
        public System.Windows.Forms.TrackBar tbPosition;
        private System.Windows.Forms.Timer timerPlayNext;
        private System.Windows.Forms.Timer TimerShow;
        private System.Windows.Forms.ToolStripMenuItem videoSnapshotSpeichernToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        public System.Windows.Forms.TrackBar tbVolume;
        private System.Windows.Forms.ToolStripMenuItem aktuellenTitelZuFavouritenHinzufügenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem titelZurPlaylisteHinzufügenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sidebarSeiteWechselnToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem playerPositionZurücksetzenToolStripMenuItem;
        public System.Windows.Forms.Timer timerHideControls;
        private System.Windows.Forms.PictureBox btnTransparency;
        private System.Windows.Forms.PictureBox btnMute;
        private System.Windows.Forms.PictureBox btnClickThrough;
        public System.Windows.Forms.PictureBox ctrlFav;
        public System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.PictureBox btnAspect;
        private System.Windows.Forms.Panel pnlHidePositionTrackbarFocusLine;
        private System.Windows.Forms.Panel pnlHideVolumeTrackbarFocusLine;
    }
}

