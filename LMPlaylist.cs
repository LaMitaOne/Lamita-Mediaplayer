using AnimatorNS;
using Id3;
using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace LaMita
{
    public partial class LMPlaylist : Form
    {

        //Actueller Titel
        public int ActualIndex = -1;
        public string ActualFile = "";
        public bool StreamPlaying = false;
        public string ActualStream = "";
        public int ActualStreamIndex = -1;

        //Mediaplayer library und Komponente für effect
        public MediaPlayer VLCEffectplayer;
        public bool LoadingNewFile = false;

        //RepeatMode 0=normal 1=repeatlist 2= random
        public int RepeatMode = 0;
        bool RandomSimiliar = false; //funktioniert zwar, aber player dll hängt sich auf...

        //Verweis auf Playerform
        public LMPlayer LMPlayerform;

        //Playerform ist durchklickbar 
        public bool isClickThrough = false;

        //ist aktuelles Cover std
        bool stdCover = true;
        string ActualCover = "";
        bool isfirstCoverLoaded = true;

        //für listview sort, dragndrop usw
        ListViewItem NewItem;
        private ColumnHeader SortingColumn = null;
        bool privateDrag = false;

        //Liste mit zuletzt gespielten titeln
        List<string> RecentlyPlayedTitles = new List<string>(10);

        //Such Position
        int SearchPos = 0;
        TreeNodeCollection SearchPosNodes;

        //Verweis auf OSDform
        LMOSDForm LMOSD;

        //Für Reaktion auf Resolutionchange
        Point LastScreenRes;

        //für Pause Befehl von Android Remote gesendet bei Anruf
        bool pausedFromCall = false;

        public class InternetCS
        {
            //Check ob eine Internetverbindung besteht
            [DllImport("wininet.dll")]
            private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
            public static bool IsConnectedToInternet()
            {
                int Desc;
                return InternetGetConnectedState(out Desc, 0);
            }
        }

        //für durchklickbares Fenster
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        public LMPlaylist()
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

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        //Drag Accept files
        void FDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        //Drag Accept files
        void FDragDrop(object sender, DragEventArgs e)
        {
            //noch ordner machen
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            int x = 0;
            foreach (string file in files)
            {
                if (isAudioFile(file) || isVideoFile(file))
                    AddnewTitle(file);
            }
            if (x > 0)
                ShowOSDInfos("" + x + " Titel hinzugefügt");
        }

        private void LMPlaylist_Load(object sender, EventArgs e)
        {
            TopMost = true;
            DoubleBuffered = true;
            pnlPlaylist.Show();
            cbTypes.SelectedIndex = 0;
            tvPlaylist.Dock = DockStyle.Fill;
            lstStreams.Dock = DockStyle.Fill;

            //um auf Auflösungsänderung zu reagieren, aktuelle auflösung
            LastScreenRes = new Point(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);

            //Liste für Mediafiles vorbereiten
            lstTitles.Dock = DockStyle.Fill;
            lstTitles.View = View.Details;
            lstTitles.FullRowSelect = true;
            lstTitles.Columns.Add("Pfad");
            lstTitles.Columns.Add("Typ");
            lstTitles.Columns[0].Width = 300;

            //Favouriten Liste vorbereiten
            lstFavourites.Dock = DockStyle.Fill;
            lstFavourites.View = View.Details;
            lstFavourites.FullRowSelect = true;
            lstFavourites.Columns.Add("Pfad");
            lstFavourites.Columns.Add("Typ");
            lstFavourites.Columns[0].Width = 300;

            //Liste für similiar Titles vorbereiten
            lstResults.View = View.Details;
            lstResults.FullRowSelect = true;
            lstResults.Columns.Add("Name");
            lstResults.Columns.Add("Wikipedia URL");
            lstResults.Columns.Add("Youtube URL");
            lstResults.Columns[0].Width = 100;
            lstResults.Columns[1].Width = 150;
            lstResults.Columns[2].Width = 150;

            //Playliste laden
            try
            {
                if (File.Exists("C:\\temp\\Playlist.txt"))
                {
                    string[] Founditems = File.ReadAllLines("C:\\temp\\Playlist.txt");
                    foreach (string filepath in Founditems)
                        AddnewTitle(filepath);
                    //treeview füllen
                    PopulateTreeView(tvPlaylist, Founditems, '\\');
                    tvPlaylist.Sort();
                    tvPlaylist.Nodes[0].Expand();
                }
            }
            catch
            {
                //log
            }

            //RecentlyPlayedTitles laden
            try
            {
                if (File.Exists("C:\\temp\\Recent.txt"))
                {
                    string[] Founditems = File.ReadAllLines("C:\\temp\\Recent.txt");
                    foreach (string filepath in Founditems)
                        RecentlyPlayedTitles.Add(filepath);
                }
            }
            catch
            {
                //log
            }

            //Favouriten laden
            try
            {
                if (File.Exists("C:\\temp\\Favourites.txt"))
                {
                    string[] Founditems = File.ReadAllLines("C:\\temp\\Favourites.txt");
                    foreach (string filepath in Founditems)
                        AddnewFavourite(filepath);
                }
            }
            catch
            {
                //log
            }
        }

        public void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            //Wenn die Auflösung sich geändert hat
            if (Screen.PrimaryScreen.WorkingArea.Width != LastScreenRes.X
                || Screen.PrimaryScreen.WorkingArea.Height != LastScreenRes.Y)
            {
                timerSlide.Enabled = false;
                //Positionen auf Standard zurücksetzen
                if (LMPlayerform.Fullscreen)
                    LMPlayerform.VLCVideowindow_DoubleClick(null, null);
                LastScreenRes = new Point(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                Size = new Size(400, Screen.PrimaryScreen.WorkingArea.Height);
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width, 0);
                btnSideSwitch.BackgroundImage = Properties.Resources.Left_Arrow_16;
                ctrlHold.BackgroundImage = Properties.Resources.Shape57_16;
                LMPlayerform.Location = new Point(100, 100);
                timerSlide.Enabled = true;
            }
        }


        public void ShowOSDInfos(string text)
        {
            if (text != "")
            {
                LMOSD.lblOSDText.Text = text;
                LMOSD.ShowTimer.Enabled = true;
            }
        }

        private void btnPlaylistTab_Click(object sender, EventArgs e)
        {
            SearchPos = 0;
            btnSimiliarTab.FlatStyle = FlatStyle.Standard;
            btnTreeTab.FlatStyle = FlatStyle.Standard;
            btnPlaylistTab.FlatStyle = FlatStyle.Popup;
            btnFavourites.FlatStyle = FlatStyle.Standard;
            btnStreams.FlatStyle = FlatStyle.Standard;
            animator1.AnimationType = AnimationType.Transparent;
            animator1.MaxAnimationTime = 500;
            if (pnlStreams.Visible == true)
                animator1.Hide(pnlStreams);
            if (pnlSimiliar.Visible == true)
                animator1.Hide(pnlSimiliar);
            if (pnlTreeview.Visible == true)
                animator1.Hide(pnlTreeview);
            if (pnlFavourites.Visible == true)
                animator1.Hide(pnlFavourites);
            animator1.AnimationType = AnimationType.Transparent;
            if (pnlPlaylist.Visible == false)
                animator1.Show(pnlPlaylist);
            animator1.WaitAllAnimations();
            animator1.MaxAnimationTime = 1000;
        }

        private void btnTreeTab_Click(object sender, EventArgs e)
        {
            SearchPos = 0;
            btnSimiliarTab.FlatStyle = FlatStyle.Standard;
            btnTreeTab.FlatStyle = FlatStyle.Popup;
            btnPlaylistTab.FlatStyle = FlatStyle.Standard;
            btnFavourites.FlatStyle = FlatStyle.Standard;
            btnStreams.FlatStyle = FlatStyle.Standard;
            animator1.AnimationType = AnimationType.Transparent;
            animator1.MaxAnimationTime = 500;
            if (pnlStreams.Visible == true)
                animator1.Hide(pnlStreams);
            if (pnlSimiliar.Visible == true)
                animator1.Hide(pnlSimiliar);
            if (pnlPlaylist.Visible == true)
                animator1.Hide(pnlPlaylist);
            if (pnlFavourites.Visible == true)
                animator1.Hide(pnlFavourites);
            animator1.AnimationType = AnimationType.Transparent;
            if (pnlTreeview.Visible == false)
                animator1.Show(pnlTreeview);
            animator1.WaitAllAnimations();
            animator1.MaxAnimationTime = 1000;
        }

        private void btnSimiliarTab_Click(object sender, EventArgs e)
        {
            SearchPos = 0;
            btnSimiliarTab.FlatStyle = FlatStyle.Popup;
            btnTreeTab.FlatStyle = FlatStyle.Standard;
            btnPlaylistTab.FlatStyle = FlatStyle.Standard;
            btnFavourites.FlatStyle = FlatStyle.Standard;
            btnStreams.FlatStyle = FlatStyle.Standard;
            animator1.AnimationType = AnimationType.Transparent;
            animator1.MaxAnimationTime = 500;
            if (pnlStreams.Visible == true)
                animator1.Hide(pnlStreams);
            if (pnlTreeview.Visible == true)
                animator1.Hide(pnlTreeview);
            if (pnlPlaylist.Visible == true)
                animator1.Hide(pnlPlaylist);
            if (pnlFavourites.Visible == true)
                animator1.Hide(pnlFavourites);
            animator1.AnimationType = AnimationType.Transparent;
            if (pnlSimiliar.Visible == false)
                animator1.Show(pnlSimiliar);
            animator1.WaitAllAnimations();
            animator1.MaxAnimationTime = 1000;
        }

        private void btnFavourites_Click(object sender, EventArgs e)
        {
            SearchPos = 0;
            btnSimiliarTab.FlatStyle = FlatStyle.Standard;
            btnTreeTab.FlatStyle = FlatStyle.Standard;
            btnPlaylistTab.FlatStyle = FlatStyle.Standard;
            btnFavourites.FlatStyle = FlatStyle.Popup;
            btnStreams.FlatStyle = FlatStyle.Standard;
            animator1.AnimationType = AnimationType.Transparent;
            animator1.MaxAnimationTime = 500;
            if (pnlStreams.Visible == true)
                animator1.Hide(pnlStreams);
            if (pnlTreeview.Visible == true)
                animator1.Hide(pnlTreeview);
            if (pnlPlaylist.Visible == true)
                animator1.Hide(pnlPlaylist);
            if (pnlSimiliar.Visible == true)
                animator1.Hide(pnlSimiliar);
            animator1.AnimationType = AnimationType.Transparent;
            if (pnlFavourites.Visible == false)
                animator1.Show(pnlFavourites);
            animator1.WaitAllAnimations();
            animator1.MaxAnimationTime = 1000;
        }

        private void btnStreams_Click(object sender, EventArgs e)
        {
            SearchPos = 0;
            btnSimiliarTab.FlatStyle = FlatStyle.Standard;
            btnTreeTab.FlatStyle = FlatStyle.Standard;
            btnPlaylistTab.FlatStyle = FlatStyle.Standard;
            btnFavourites.FlatStyle = FlatStyle.Standard;
            btnStreams.FlatStyle = FlatStyle.Popup;
            animator1.AnimationType = AnimationType.Transparent;
            animator1.MaxAnimationTime = 500;
            if (pnlTreeview.Visible == true)
                animator1.Hide(pnlTreeview);
            if (pnlPlaylist.Visible == true)
                animator1.Hide(pnlPlaylist);
            if (pnlFavourites.Visible == true)
                animator1.Hide(pnlFavourites);
            if (pnlSimiliar.Visible == true)
                animator1.Hide(pnlSimiliar);
            animator1.AnimationType = AnimationType.Transparent;
            if (pnlStreams.Visible == false)
                animator1.Show(pnlStreams);
            animator1.WaitAllAnimations();
            animator1.MaxAnimationTime = 1000;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //check ob Internetverbindung besteht
            if (InternetCS.IsConnectedToInternet() == false)
                ShowOSDInfos("Internetverbindung nicht möglich!");
            else
            if (edtSearchText.Text == "")
                ShowOSDInfos("Bitte Suchbegriff eingeben!");
            else
            {
                lstResults.Items.Clear();

                //Request Url zusammensetzen
                String searchtype = "";
                String searchtext = edtSearchText.Text;
                searchtext = searchtext.Replace(" ", "+");

                //Suchen nach welchem Typ
                switch (cbTypes.SelectedIndex)
                {
                    case 0:
                        searchtype = "";  //sucht nach allen typen
                        break;
                    case 1:
                        searchtype = "&type=music";
                        break;
                    case 2:
                        searchtype = "&type=movies";
                        break;
                    case 3:
                        searchtype = "&type=shows";
                        break;
                    case 4:
                        searchtype = "&type=podcasts";
                        break;
                    case 5:
                        searchtype = "&type=authors";
                        break;
                    case 6:
                        searchtype = "&type=games";
                        break;
                }

                //Url zusammensetzen
                String URL = "https://tastedive.com/api/similar?q=" + searchtext + "&verbose=1" + searchtype + "&limit=100&k=API";

                try
                {
                    WebClient client = new WebClient();

                    //Request senden
                    Stream data = client.OpenRead(@URL);

                    if (data != null)
                    {
                        //Ergebnis einlesen
                        StreamReader reader = new StreamReader(data);
                        string ResultString = "";
                        ResultString = reader.ReadToEnd();
                        String temp = ResultString;
                        String[] Results = Regex.Split(temp, "\"Name\": \"");

                        //Ich zerleg den String manuell anstatt JSON Komponente zu benutzen
                        //ab 2 weil die ersten sind nur die Band nach der man gesucht hat
                        for (int x = 2; x < Results.Length; x++)
                        {
                            //Name
                            if (Results[x].Contains("\", \"Type\":") == true)
                            {
                                int Startpos = 0;
                                int Endpos = Results[x].IndexOf("\", \"Type\":");
                                string newname = Results[x].Substring(Startpos, Endpos);
                                newname = newname.Replace(":", "");
                                NewItem = new ListViewItem(newname, 2);
                            }

                            //Wikipedia URL
                            if (Results[x].Contains("wUrl\": \"") == true)
                            {
                                int Startpos2 = Results[x].IndexOf("wUrl\": \"");
                                int Endpos2 = Results[x].IndexOf("\", \"yUrl\": \"");
                                int strlen = Endpos2 - Startpos2;
                                string newwUrl = Results[x].Substring(Startpos2 + 8, strlen - 8);
                                NewItem.SubItems.Add(newwUrl);
                            }

                            //Youtube URL
                            if (Results[x].Contains("\", \"yUrl\": \"") == true)
                            {
                                int Startpos3 = Results[x].IndexOf("\", \"yUrl\": \"");
                                int Endpos3 = Results[x].IndexOf("\", \"yID\": \"");
                                int strlen2 = Endpos3 - Startpos3;
                                string newYUrl = Results[x].Substring(Startpos3 + 12, strlen2 - 12);
                                NewItem.SubItems.Add(newYUrl);
                            }

                            //List Item hinzufügen
                            if (NewItem != null)
                            {
                                lstResults.Items.Add(NewItem);
                                NewItem = null;
                            }
                        }
                    }
                    else
                        ShowOSDInfos("Keine ähnlichen Titel gefunden");
                }
                catch (WebException exp)
                {
                    ShowOSDInfos(exp.Message);
                }
            }
        }

        private void lstTitles_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                bool wasStopped = false;
                int delCount = 0;
                //Titel stoppen wenn dieser aktuell läuft
                for (int i = 0; i < ((ListView)sender).SelectedItems.Count; i++)
                    if (((ListView)sender).SelectedItems[i].Index == ActualIndex)
                    {
                        ctrlStop_Click(null, null);
                        wasStopped = true;
                    }
                foreach (ListViewItem listViewItem in lstTitles.SelectedItems)
                {
                    //auch alle dazugehörigen TreeNodes entfernen
                    TreeNode tn = GetNodeByText(tvPlaylist.Nodes, listViewItem.Text);
                    if (tn != null)
                    {
                        tvPlaylist.SelectedNode = tn;
                        string upLevelNode = tvPlaylist.SelectedNode.FullPath;
                        upLevelNode = upLevelNode.Replace("\\" + tn.Text, "");
                        TreeNode tprev = GetNodeByText(tvPlaylist.Nodes, upLevelNode);
                        tn.Remove();
                        if (tprev.GetNodeCount(true) < 1)
                            tprev.Remove();
                    }
                    listViewItem.Remove();
                    delCount++;
                }
                if (delCount > 0)
                    ShowOSDInfos("" + delCount + " Titel wurden entfernt");
                if (lstTitles.Items.Count == 0)
                {
                    ActualIndex = -1;
                    ActualFile = "";
                }
                else //aktuellen index wieder finden - könnte sich geändert haben
                {
                    if (wasStopped == false)
                        FindActualTitle();
                }
            }
        }

        private void FindActualTitle()
        {
            //Aktuellen titel wieder finden nach Sort oder entfernen von titeln usw
            for (int i = 0; i < lstTitles.Items.Count; i++)
                if (lstTitles.Items[i].Text == ActualFile)
                {
                    lstTitles.Items[i].ForeColor = Color.Lime;
                    ActualIndex = i;
                    break;
                }
        }

        public void btnAdd_Click(object sender, EventArgs e)
        {
            //OpenDialog vorbereiten
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Media files (*.mp3;*.mp4;*.avi;*.mkv;*.mpg;*.mpeg;*.wmv;*.flv;*.vob;*.mov;*.3gp)" +
                "|*.mp3;*.mp4;*.avi;*.mkv;*.mpg;*.mpeg;*.wmv;*.flv:*.vob;*.mov;*.3gp";
            opf.Multiselect = true;

            //OpenDialog starten
            DialogResult result = opf.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (lstTitles.Items.Count == 0)
                {
                    //wenn Liste leer war, Actualtitle auf 0 setzen
                    ActualIndex = 0;
                }
                for (int i = 0; i < opf.FileNames.Length; i++)
                {
                    //Gewählte Titel zur Liste hinzufügen
                    AddnewTitle(opf.FileNames[i]);
                }
                //todo - Thread der weitere infos mit MediaInfo.dll einliest
                PopulateTreeView(tvPlaylist, opf.FileNames, '\\');
                tvPlaylist.Sort();
                tvPlaylist.Nodes[0].Expand();
                ShowOSDInfos("" + opf.FileNames.Length + " Titel hinzugefügt");
            }
        }

        public void AddnewTitle(string path)
        {
            ListViewItem NewItem = new ListViewItem(path, 2);
            if (isAudioFile(path))
            {
                NewItem.SubItems.Add("Audio");
                NewItem.ImageIndex = 1;
            }
            else
            {
                NewItem.SubItems.Add("Video");
                NewItem.ImageIndex = 2;
            }
            lstTitles.Items.Add(NewItem);
        }

        public void AddnewFavourite(string path)
        {
            ListViewItem NewItem = new ListViewItem(path, 2);
            if (isAudioFile(path))
            {
                NewItem.SubItems.Add("Audio");
                NewItem.ImageIndex = 1;
            }
            else
            {
                NewItem.SubItems.Add("Video");
                NewItem.ImageIndex = 2;
            }
            lstFavourites.Items.Add(NewItem);
        }

        public void ctrlPlay_Click(object sender, EventArgs e)
        {
            if (LoadingNewFile == false)
            {
                //Wenn gerade ein Stream läuft, stoppen
                if (StreamPlaying)
                    ctrlStop_Click(null, null);

                //Wenn keine title in der liste, dann erstmal Openfiledialog starten
                if (lstTitles.Items.Count == 0)
                    btnAdd_Click(null, null);

                //Abspielen starten wenn Titel in der Playliste sind
                if (lstTitles.Items.Count > 0)
                    if (LMPlayerform.VLCMediaplayer.State == VLCState.Paused)
                        ctrlPause_Click(null, null);
                    else
                        Playthis(lstTitles.Items[ActualIndex].Text);
            }
        }

        public void Playthis(String Path)
        {
            bool SlidetimerPrevSetting = false;
            if (LoadingNewFile == false)
            {
                LoadingNewFile = true;
                StreamPlaying = false;
                LMPlayerform.timerHideControls.Enabled = false;
                SlidetimerPrevSetting = timerSlide.Enabled;
                timerSlide.Enabled = false;
                if (File.Exists(@Path))
                {
                    try
                    {
                        ActualFile = Path;

                        //wenn Video dann Videowindow anzeigen
                        if (isVideoFile(ActualFile) == true)
                        {
                            //Cover ausblenden
                            HideCovers();
                            //Video anzeigen
                            ShowvideoWindow();
                        }
                        else
                            HideVideoWindow();

                        //Titel starten
                        ThreadPool.QueueUserWorkItem(_ => LMPlayerform.VLCMediaplayer.Stop());
                        ThreadPool.QueueUserWorkItem(_ => LMPlayerform.VLCMediaplayer.Play(new Media(LMPlayerform.VLClib, @ActualFile)));

                        //in Liste mit zuletzt gespielten titeln hinzufügen, wenn nicht schon drin ist
                        //bei titlerepeat zb, wäre ja sinnlos da 10 mal denselben titel zu haben
                        bool isAlreadyinList = false;
                        for (int i = 0; i < RecentlyPlayedTitles.Count; i++)
                        {
                            if (ActualFile == RecentlyPlayedTitles[i])
                            {
                                isAlreadyinList = true;
                                break;
                            }
                        }

                        //ist noch nicht in Recent-Liste, also hinzufügen
                        if (isAlreadyinList == false)
                            RecentlyPlayedTitles.Insert(0, ActualFile);

                        //liste auf max 10 beschränken
                        if (RecentlyPlayedTitles.Count > 10)
                            RecentlyPlayedTitles.RemoveRange(10, RecentlyPlayedTitles.Count - 10);

                        //in liste zu aktuellem titel scrollen
                        FindActualTitle();
                        lstTitles.EnsureVisible(ActualIndex);

                        //CoverTimer starten, sucht cover und zeigt auch OSD an
                        timerCover.Enabled = true;

                        //Aktuellen Titel in der Liste farblich markieren
                        ColorizeActualTitle();
                    }
                    catch (Exception err)
                    {
                        ShowOSDInfos(err.ToString());
                    }
                    finally
                    {
                        LoadingNewFile = false;
                    }
                }
                else
                {
                    ShowOSDInfos("Datei kann ncht gefunden werden!");
                }
            }
            LoadingNewFile = false;
            LMPlayerform.timerHideControls.Enabled = true;
            timerSlide.Enabled = SlidetimerPrevSetting;
        }

        private void ColorizeActualTitle()
        {
            //aktuellen titel farblich hervorheben
            for (int i = 0; i < lstTitles.Items.Count; i++)
            {
                if (i == ActualIndex)
                    lstTitles.Items[i].ForeColor = Color.Lime;
                else
                {
                    if (lstTitles.Items[i].ForeColor != Color.White)
                        lstTitles.Items[i].ForeColor = Color.White;
                }
            }
        }
        public void ctrlPrev_Click(object sender, EventArgs e)
        {
            if (LoadingNewFile == false)
            {
                if (StreamPlaying == false)
                {
                    try
                    {
                        //Actualtitle - 1
                        if (ActualIndex > 0)
                        {
                            //vorherigen Titel starten
                            ActualIndex = ActualIndex - 1;
                            lstTitles.Items[ActualIndex].ForeColor = Color.Lime;
                            Playthis(lstTitles.Items[ActualIndex].Text);
                        }
                    }
                    catch (Exception err)
                    {
                        ShowOSDInfos(err.ToString());
                    }
                }
                else
                {
                    //vorherigen Stream starten
                    try
                    {
                        //ActualStreamIndex - 1
                        if (ActualStreamIndex > 0)
                        {
                            //vorherigen Stream starten
                            ActualStreamIndex = ActualStreamIndex - 1;
                            PlaythisStream(lstStreams.Items[ActualStreamIndex].SubItems[2].Text, ActualStreamIndex);
                        }
                    }
                    catch (Exception err)
                    {
                        ShowOSDInfos(err.ToString());
                    }
                }
            }
        }


        public void ctrlStop_Click(object sender, EventArgs e)
        {
            //Player stoppen
            if (LoadingNewFile == false)
            {
                ThreadPool.QueueUserWorkItem(_ => LMPlayerform.VLCMediaplayer.Stop());

                HideCovers();
                ResetCovers();
                LMPlayerform.lblPos.Text = "00:00:00";
                stdCover = true;
                StreamPlaying = false;
                HideVideoWindow();
            }
        }


        public void ctrlPause_Click(object sender, EventArgs e)
        {
            //Pause Player
            if (LoadingNewFile == false)
            {
                try
                {
                    if (StreamPlaying == false)
                        ThreadPool.QueueUserWorkItem(_ => LMPlayerform.VLCMediaplayer.Pause());
                }
                catch (Exception err)
                {
                    ShowOSDInfos(err.Message);
                }
            }
        }


        public void ctrlNext_Click(object sender, EventArgs e)
        {
            if (LoadingNewFile == false)
            {
                if (StreamPlaying == false)
                {
                    if (RepeatMode == 0 || RepeatMode == 3)
                    {
                        //NormalPlayback - oder bei Titlerepeat
                        if (ActualIndex < lstTitles.Items.Count - 1)
                        {
                            ActualIndex = ActualIndex + 1;
                            lstTitles.Items[ActualIndex].ForeColor = Color.Lime;
                            Playthis(lstTitles.Items[ActualIndex].Text);
                        }
                    }
                    else if (RepeatMode == 1)
                    {
                        //Playliste wiederholen
                        if (ActualIndex < lstTitles.Items.Count - 1)
                            ActualIndex = ActualIndex + 1;
                        else
                            ActualIndex = 0;
                        lstTitles.Items[ActualIndex].ForeColor = Color.Lime;
                        Playthis(lstTitles.Items[ActualIndex].Text);
                    }
                    else if (RepeatMode == 2)
                    {
                        int OldIndex = ActualIndex;
                        //random
                        try
                        {
                            bool Similiarfound = false;
                            //Random wenn möglich nach ähnlichen Titeln
                            if (RandomSimiliar == true)
                            {
                                bool Resultsfound = false;
                                bool isMP3 = ActualFile.ToLower().Contains(".mp3");
                                string searchtext = "";
                                if (isMP3)
                                    searchtext = GetMP3TagArtistandTitle();

                                //wenn Bandname gefunden dann nach Similiar suchen
                                if (searchtext != "")
                                {
                                    edtSearchText.Text = searchtext;
                                    cbTypes.SelectedIndex = 0; //hier noch auf Bands oder shows oder movies später

                                    //similiar suche starten
                                    btnSearch_Click(null, null);

                                    //wurden similiar titles gefunden?
                                    if (lstResults.Items.Count > 0)
                                        Resultsfound = true;

                                    //es wurden welche gefunden
                                    if (Resultsfound)
                                    {
                                        int i = 0;
                                        Random r = new Random();
                                        do
                                        {
                                            if (Similiarfound == false)
                                                ActualIndex = r.Next(0, lstTitles.Items.Count);
                                            for (int x = 0; x < lstResults.Items.Count; x++)
                                            {
                                                if (lstTitles.Items[ActualIndex].Text.ToLower().Contains(lstResults.Items[x].Text.ToLower()) == true)
                                                {
                                                    Similiarfound = true;
                                                    break;
                                                }
                                            }
                                            i++;
                                        } while (Similiarfound == false || i < lstTitles.Items.Count);
                                    }
                                }
                                else
                                    Similiarfound = false;
                                if (Similiarfound)
                                {
                                    //similiar wurde gefunden also diesen abspielen
                                    lstTitles.Items[ActualIndex].ForeColor = Color.Lime;
                                    Playthis(lstTitles.Items[ActualIndex].Text);
                                }
                                else
                                {   //es wurde nichts gefunden also normal Random
                                    ActualIndex = GetNewRandom();
                                    lstTitles.Items[ActualIndex].ForeColor = Color.Lime;
                                    Playthis(lstTitles.Items[ActualIndex].Text);
                                }
                            }
                            else
                            {
                                //normal Random
                                ActualIndex = GetNewRandom();
                                lstTitles.Items[ActualIndex].ForeColor = Color.Lime;
                                Playthis(lstTitles.Items[ActualIndex].Text);
                            }
                        }
                        catch (Exception err)
                        {
                            //fehler beim einlesen der mp3 tags oder similiar suche oder gott weiß was                    
                            ShowOSDInfos(err.Message);
                            ctrlStop_Click(null, null);
                        }
                    }
                }
                else
                {
                    //nächsten Stream starten                
                    if (ActualStreamIndex < lstStreams.Items.Count - 1)
                        ActualStreamIndex = ActualStreamIndex + 1;
                    else
                        ActualStreamIndex = 0;
                    PlaythisStream(lstStreams.Items[ActualStreamIndex].SubItems[2].Text, ActualStreamIndex);
                }
            }
        }

        private int GetNewRandom()
        {
            Random r = new Random();
            int Y = 0;
            bool isNotinRecentList = false;
            int NewIndex;
            do
            {
                //schauen ob der titel nicht gerade erst gespielt wurde
                //ansonsten neuer random
                NewIndex = r.Next(0, lstTitles.Items.Count);
                for (int x = 0; x < RecentlyPlayedTitles.Count; x++)
                {
                    if (lstTitles.Items[NewIndex].Text == RecentlyPlayedTitles[x])
                    {
                        isNotinRecentList = false;
                        break;
                    }
                    else
                        isNotinRecentList = true;
                }
                Y++;
            }
            while (isNotinRecentList == false && Y < lstTitles.Items.Count);
            return NewIndex;
        }

        private void PopulateTreeView(TreeView treeView, string[] paths, char pathSeparator)
        {
            //Treeview füllen
            TreeNode lastNode = null;
            string subPathAgg;
            foreach (string path in paths)
            {
                subPathAgg = string.Empty;
                foreach (string subPath in path.Split(pathSeparator))
                {
                    subPathAgg += subPath + pathSeparator;
                    TreeNode[] nodes = treeView.Nodes.Find(subPathAgg, true);

                    if (nodes.Length == 0)
                        if (lastNode == null)
                            lastNode = treeView.Nodes.Add(subPathAgg, subPath);
                        else
                            lastNode = lastNode.Nodes.Add(subPathAgg, subPath);
                    else
                        lastNode = nodes[0];
                    FileInfo fi = new FileInfo(@lastNode.FullPath);
                    if (fi.Extension == "")
                        lastNode.ImageIndex = 0;
                    else
                     if (isAudioFile(lastNode.FullPath) == true)
                        lastNode.ImageIndex = 1;
                    else lastNode.ImageIndex = 2;
                    lastNode.ContextMenuStrip = popupTreeview;
                }
                lastNode = null;
            }
        }


        public bool isVideoFile(string path)
        {
            //Check ob dies eine Video Datei ist
            FileInfo fi = new FileInfo(@path);
            String ext = fi.Extension.ToLower();
            if (ext == ".mp4" || ext == ".mpg" || ext == ".avi" || ext == ".mpeg" || ext == ".dvb"
             || ext == ".wmv" || ext == ".mpe" || ext == ".asf" || ext == ".mkv" || ext == ".mpv"
             || ext == ".rm" || ext == ".vob" || ext == ".mov" || ext == ".m1v" || ext == ".mpv2"
             || ext == ".m4v" || ext == ".m2v" || ext == ".mp2v" || ext == ".mpgv" || ext == ".ogv"
             || ext == ".3gp" || ext == ".mpgx" || ext == ".flv" || ext == ".rmvb" || ext == ".fluxdvd"
             || ext == ".ratdvd" || ext == ".bik")
                return true;
            else
                return false;
        }


        public bool isAudioFile(string path)
        {
            //Check ob das eine Audio Datei ist
            FileInfo fi = new FileInfo(@path);
            String ext = fi.Extension.ToLower();
            if (ext == ".mp3" || ext == ".wma" || ext == ".ogg" || ext == ".cda" || ext == ".ac3"
             || ext == ".dts" || ext == ".mka" || ext == ".aac" || ext == ".ape" || ext == ".flac"
             || ext == ".mpa" || ext == ".fla" || ext == ".wav" || ext == ".m4a" || ext == ".ra"
             || ext == ".m4v")
                return true;
            else
                return false;
        }

        private string GetMP3TagArtistandTitle()
        {
            //bool mp3tagsFound = false;
            string Foundtext = "";
            bool mp3tagsFound = false;
            //Bandname und Titel aus mp3tag auslesen
            try
            {
                var mp3 = new Mp3(@ActualFile, Mp3Permissions.Read);
                //Versuchen V1 einzulesen
                Id3Tag tag = mp3.GetTag(Id3TagFamily.Version1X);
                if (tag != null)
                {
                    if (tag.Artists != null && tag.Artists != "null")
                    {
                        if (tag.Artists != String.Empty && tag.Title != "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")
                        {
                            Foundtext = tag.Artists + "\n" + tag.Title;
                            mp3tagsFound = true;
                        }
                    }
                }

                //Versuchen V2 einzulesen
                if (mp3tagsFound == false)
                {
                    tag = mp3.GetTag(Id3TagFamily.Version2X);
                    if (tag != null)
                    {
                        if (tag.Artists != null && tag.Artists != "null")
                        {
                            if (tag.Artists != String.Empty && tag.Title != "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")
                            {
                                Foundtext = tag.Artists + "\n" + tag.Title;
                                mp3tagsFound = true;
                            }
                        }
                    }
                }
                mp3.Dispose();

                if (mp3tagsFound == false)
                    Foundtext = "";
                return
                    Foundtext;
            }
            catch
            {
                return "";
            }
        }

        public Bitmap ScaleImage(Bitmap bmp, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / bmp.Width;
            var ratioY = (double)maxHeight / bmp.Height;
            var ratio = Math.Min(ratioX, ratioY);
            var newWidth = (int)(bmp.Width * ratio);
            var newHeight = (int)(bmp.Height * ratio);
            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(bmp, 0, 0, newWidth, newHeight);
            return newImage;
        }

        private void timerCover_Tick(object sender, EventArgs e)
        {
            //Cover suchen und OSD anzeigen
            timerCover.Enabled = false;

            //OSD mit titel anzeigen
            string Ftext = "";
            bool isMP3 = ActualFile.ToLower().Contains(".mp3");
            if (isMP3)
                Ftext = GetMP3TagArtistandTitle();
            FileInfo fi = new FileInfo(@ActualFile);
            DirectoryInfo dir = new DirectoryInfo(@fi.Directory.ToString());

            //es wurden keine tags gefunden oder es ist keine mp3 datei,
            //dann Ordner + Dateiname anzeigen
            if (Ftext == "")
            {
                string ext = fi.Extension;
                Ftext = fi.Name;
                Ftext = dir.Name + " - " + Ftext.Replace(ext, "");
            }

            //titel anzeigen
            ShowOSDInfos(Ftext);

            bool Coverloaded = false;
            if (isAudioFile(ActualFile))
                LMPlayerform.pbCover.BringToFront();
            LMPlayerform.pnlControls.BringToFront();

            //Cover suchen und anzeigen
            try
            {
                FileInfo[] files;
                files = dir.GetFiles();
                if (files.Length > 0)
                {
                    //Dateien im Ordner suchsehen und nach einem Bild suchen
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].Extension == ".jpg" || files[i].Extension == ".jpeg"
                         || files[i].Extension == ".bmp" || files[i].Extension == ".png")
                        {
                            //Bild gefunden und laden und anzeigen,
                            //nur wenn es ein anderes als zuletzt ist
                            if (files[i].FullName != ActualCover || LMPlayerform.pbCover.Visible == false)
                            {
                                LMPlayerform.waterImg.Hide();
                                if (isfirstCoverLoaded)
                                {
                                    //Bugfix, sonst wird das bild beim ersten mal nicht angezeigt
                                    //sondern nur ein verzerrtes std bild, vielleicht weil die
                                    //Paintbox unsichtbar war anfangs
                                    LMPlayerform.pbCover.Show();
                                    isfirstCoverLoaded = false;
                                }
                                else
                                    LMPlayerform.pbCover.Hide();
                                LMPlayerform.pbCover.BackgroundImage = Image.FromFile(files[i].FullName);
                                if (isAudioFile(ActualFile))
                                {
                                    animator1.Show(LMPlayerform.pbCover);
                                    animator1.WaitAllAnimations();

                                    //--------------Experimentell waterimage
                                    //im mom nur bei standard bild, sieht bei anderen noch nicht so optimal aus
                                    //ShowWaterImage();
                                    //--------------
                                }
                                ActualCover = files[i].FullName;
                            }
                            Coverloaded = true;
                            stdCover = false;
                            break;
                        }
                    }
                }
            }
            catch
            {
                //log
                Coverloaded = false;
            }

            //wenn kein Cover gefunden wurde, standard bild anzeigen
            if (Coverloaded == false)
            {
                if (stdCover == false || LMPlayerform.pbCover.Visible == false)
                {
                    LMPlayerform.waterImg.Hide();
                    LMPlayerform.pbCover.Hide();
                    ResetCovers();
                    if (isAudioFile(ActualFile))
                    {
                        //normales Bild
                        //animator1.Show(LMPlayerform.pbCover);
                        //animator1.WaitAllAnimations();

                        //Wasser effect bild zeigen, im mom nur bei std Bild, bei anderen siehts nicht so gut aus
                        ShowWaterImage();
                    }
                }
                ActualCover = "std";
                stdCover = true;
            }
            pbCoverPl.BackgroundImage = LMPlayerform.pbCover.BackgroundImage;
            LMOSD.pbOSDCover.BackgroundImage = LMPlayerform.pbCover.BackgroundImage;
        }

        public void SaveSettingsNow()
        {
            try
            {
                //kurz warten bis alles animationen beendet sind
                animator1.WaitAllAnimations();

                //alle timer deaktivieren
                LMPlayerform.timerHideControls.Enabled = false;
                timerCover.Enabled = false;
                timerSlide.Enabled = false;

                //Trayicon ausblenden
                LMPlayerform.TrayIcon.Visible = false;

                //aus fullscreen gehen wenn gerade video in fullscreen läuft
                if (LMPlayerform.Fullscreen)
                    LMPlayerform.VLCVideowindow_DoubleClick(null, null);

                //Einstellungen speichern
                var MyIni = new IniFile("C:\\temp\\Settings.ini");
                MyIni.Write("LastIndex", ActualIndex.ToString());
                MyIni.Write("RepeatMode", RepeatMode.ToString());
                //if (RandomSimiliar)
                //    MyIni.Write("RandomSimiliar", "true");
                //else
                //    MyIni.Write("RandomSimiliar", "false");
                MyIni.Write("LastPlayerPosition", LMPlayerform.VLCMediaplayer.Position.ToString());
                MyIni.Write("LastPosX", LMPlayerform.Location.X.ToString());
                MyIni.Write("LastPosY", LMPlayerform.Location.Y.ToString());
                MyIni.Write("LastVolume", LMPlayerform.tbVolume.Value.ToString());

                //Playliste speichern
                StreamWriter sw = new StreamWriter("C:\\temp\\Playlist.txt");
                for (int i = 0; i < lstTitles.Items.Count; i++)
                    sw.WriteLine(lstTitles.Items[i].Text);
                sw.Close();

                //RecentlyPlayedTitles speichern
                StreamWriter sws = new StreamWriter("C:\\temp\\Recent.txt");
                for (int i = 0; i < RecentlyPlayedTitles.Count; i++)
                    sws.WriteLine(RecentlyPlayedTitles[i]);
                sws.Close();

                //Favouritenliste speichern
                StreamWriter swf = new StreamWriter("C:\\temp\\Favourites.txt");
                for (int i = 0; i < lstFavourites.Items.Count; i++)
                    swf.WriteLine(lstFavourites.Items[i].Text);
                swf.Close();

                //alles ok also running auf false setzen,
                //kann somit beim nächsten Programmstart
                //beim letzten Titel wieder abspielen automatisch
                MyIni.Write("Running", "false");
            }
            finally
            {

            }
        }

        private void lstTitles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (LoadingNewFile == false)
            {
                //geklickten Titel starten
                var senderList = (ListView)sender;
                var clickedItem = senderList.HitTest(e.Location).Item;
                if (clickedItem != null)
                {
                    ActualIndex = clickedItem.Index;
                    Playthis(clickedItem.Text);
                }
            }
        }

        private void tvPlaylist_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (LoadingNewFile == false)
            {
                //geklickten Titel aus Treeview starten
                if (tvPlaylist.SelectedNode != null)
                {
                    string SelectedFile = tvPlaylist.SelectedNode.FullPath;
                    if (isVideoFile(SelectedFile) || isAudioFile(SelectedFile))
                    {
                        ActualFile = SelectedFile;
                        FindActualTitle();
                        Playthis(SelectedFile);
                    }
                }
            }
        }

        private void LMPlaylist_Shown(object sender, EventArgs e)
        {
            //OSDForm create
            LMOSD = new LMOSDForm();

            //init VLC für Slide effect sound der sidebar
            //hab auch schon versucht ob dieser das Problem manchmal ergibt, aber...nope
            VLCEffectplayer = new MediaPlayer(LMPlayerform.VLClib);
            VLCEffectplayer.Volume = 80;

            //Einstellungen laden
            this.ShowInTaskbar = false;  //nicht in taskbar anzeigen, stattdessen hab ich ein trayicon
            if (File.Exists("C:\\temp\\Settings.ini"))
            {
                var MyIni = new IniFile("C:\\temp\\Settings.ini");
                string LastIndex = MyIni.Read("LastIndex");
                string LastRepeatMode = MyIni.Read("RepeatMode");
                string LastRandomSimiliar = MyIni.Read("RandomSimiliar");
                string LastPlayPos = MyIni.Read("LastPlayerPosition");
                //RandomSimiliar = (LastRandomSimiliar == "true");
                string WasRunning = MyIni.Read("Running");
                if (LastIndex != "")
                    ActualIndex = Int32.Parse(LastIndex);
                if (LastRepeatMode != "")
                    RepeatMode = Int32.Parse(LastRepeatMode);

                //letzten Titel wieder starten - wenn zuletzt ordentlich geschlossen wurde
                if (ActualIndex != -1 && ActualIndex < lstTitles.Items.Count
                     && File.Exists(@lstTitles.Items[ActualIndex].Text))
                {
                    if (WasRunning == "false")
                    {
                        Playthis(lstTitles.Items[ActualIndex].Text);
                        if (LastPlayPos != "" && LastPlayPos != "-1")
                        {
                            float NewPos = float.Parse(LastPlayPos);
                            //kurz warten das der Player auch schon läuft, und dann an die letzte Position
                            //nicht ganz optimal so, aber im mom gehts nich anders mit dem Threadpool
                            try
                            {
                                Thread.Sleep(100);
                                ThreadPool.QueueUserWorkItem(_ => LMPlayerform.VLCMediaplayer.Position = NewPos);
                            }
                            finally
                            {

                            }
                        }
                    }
                }

                //ich speichere das der Player jetzt läuft, und beim schliessen auf false.
                //Beim nächsten start 
                //wenn das nicht false ist, dann hat sich der Player aufgehängt oder sonstwie fehlerhaft beendet
                //und startet dann nicht sofort beim letzten Titel um sich nicht wieder aufzuhängen
                //falls dieser fehlerhaft sein könnte
                var MyXIni = new IniFile("C:\\temp\\Settings.ini");
                MyXIni.Write("Running", "true");

                //UDPServer für Remote Control von Android APP
                try
                {
                    UdpClient socket = new UdpClient(1777);
                    socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
                    IPEndPoint target = new IPEndPoint(IPAddress.Parse("192.168.0.233"), 1777);
                }
                catch
                {
                    //gibt nen fehler wenn schon ein udpclient auf port 1777 läuft
                }
            }
            //SlideTimer für die Sidebar aktivieren
            timerSlide.Enabled = true;
        }

        void OnUdpData(IAsyncResult result)
        {
            try
            {
                //wartet auf Befehle von Android remote
                UdpClient socket = result.AsyncState as UdpClient;
                IPEndPoint source = new IPEndPoint(0, 0);
                byte[] message = socket.EndReceive(result, ref source);
                String befehl = Encoding.UTF8.GetString(message, 0, message.Length);

                //Befehl ausführen
                //hier überall Invoke weil sonst gibts Thread Fehler
                if (befehl == "MRPLAY")
                    Invoke(new Action(() =>
                    {
                        if (LMPlayerform.VLCMediaplayer.State == VLCState.Playing
                         || LMPlayerform.VLCMediaplayer.State == VLCState.Paused)
                            ctrlPause_Click(null, null);
                        else
                            ctrlPlay_Click(null, null);
                    }));
                else if (befehl == "MRSTOP")
                    Invoke(new Action(() =>
                    {
                        ctrlStop_Click(null, null);
                    }));
                else if (befehl == "MRFORWARD")
                    Invoke(new Action(() =>
                    {
                        ctrlNext_Click(null, null);
                    }));
                else if (befehl == "MRASPECT")
                    Invoke(new Action(() =>
                    {
                        LMPlayerform.btnAspect_Click(null, null);
                    }));
                else if (befehl == "MRBACKWARD")
                    Invoke(new Action(() =>
                    {
                        ctrlPrev_Click(null, null);
                    }));
                //Handy bekommt Anruf, player pausieren
                else if (befehl == "MRPAUSECALL")
                {
                    Invoke(new Action(() =>
                    {
                        if (LMPlayerform.VLCMediaplayer.State == VLCState.Playing)
                        {
                            pausedFromCall = true;
                            ctrlPause_Click(null, null);
                        }
                    }));
                }
                else if (befehl == "MRPLAYANDROID")
                {
                    Invoke(new Action(() =>
                    {
                        //Player war pausiert wegen Anruf
                        //Anruf ist beendet, also Player wieder starten
                        if (pausedFromCall)
                        {
                            pausedFromCall = false;
                            ctrlPause_Click(null, null);
                        }
                    }));
                }
                //warten auf neuen Befehl
                socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            }
            catch
            {
                //log
            }
        }

        private void btnRepeat_Click(object sender, EventArgs e)
        {
            popupRepeat.Show(this, pnlControls.Location.X + btnRepeat.Location.X,
                                   pnlControls.Location.Y + btnRepeat.Location.Y - 92);
        }

        private void popupRepeat_Click(object sender, EventArgs e)
        {
            //RepeatMode ändern
            RepeatMode = int.Parse((sender as ToolStripMenuItem).Tag.ToString());
        }

        private void popupRepeat_Opening(object sender, CancelEventArgs e)
        {
            popupRepeat.BackColor = Color.Black;
            popupRepeat.ForeColor = Color.DarkGray;
            normalAbspielenToolStripMenuItem.Checked = false;
            playlisteWiederholenToolStripMenuItem.Checked = false;
            zufallswiedergabeToolStripMenuItem.Checked = false;
            titelWiederholenToolStripMenuItem.Checked = false;
            zufallswiedergabeMitÄhnlichenTitelnBevorzugenToolStripMenuItem.Checked = RandomSimiliar;
            switch (RepeatMode)
            {
                case 0:
                    normalAbspielenToolStripMenuItem.Checked = true;
                    break;
                case 1:
                    playlisteWiederholenToolStripMenuItem.Checked = true;
                    break;
                case 2:
                    zufallswiedergabeToolStripMenuItem.Checked = true;
                    break;
                case 3:
                    titelWiederholenToolStripMenuItem.Checked = true;
                    break;
            }
        }

        private void zufallswiedergabeMitÄhnlichenTitelnBevorzugenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RandomSimiliar == true)
                RandomSimiliar = false;
            else
                RandomSimiliar = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            LMPlayerform.poiClose_Click(null, null);
        }

        private void animator1_AnimationCompleted(object sender, AnimationCompletedEventArg e)
        {
            //Random Animation einstellen
            Random r = new Random();
            int x = r.Next(0, 12);
            switch (x)
            {
                case 0:
                    animator1.AnimationType = AnimationType.HorizBlind;
                    break;
                case 1:
                    animator1.AnimationType = AnimationType.HorizSlide;
                    break;
                case 2:
                    animator1.AnimationType = AnimationType.HorizSlideAndRotate;
                    break;
                case 3:
                    animator1.AnimationType = AnimationType.Leaf;
                    break;
                case 4:
                    animator1.AnimationType = AnimationType.Mosaic;
                    break;
                case 5:
                    animator1.AnimationType = AnimationType.Particles;
                    break;
                case 6:
                    animator1.AnimationType = AnimationType.Rotate;
                    break;
                case 7:
                    animator1.AnimationType = AnimationType.Scale;
                    break;
                case 8:
                    animator1.AnimationType = AnimationType.ScaleAndHorizSlide;
                    break;
                case 9:
                    animator1.AnimationType = AnimationType.ScaleAndRotate;
                    break;
                case 10:
                    animator1.AnimationType = AnimationType.Transparent;
                    break;
                case 11:
                    animator1.AnimationType = AnimationType.VertBlind;
                    break;
                case 12:
                    animator1.AnimationType = AnimationType.VertSlide;
                    break;
            }
        }


        private void ctrlHold_Click(object sender, EventArgs e)
        {
            //Automatisches ausblenden deaktivieren
            if (timerSlide.Enabled == true)
            {
                timerSlide.Enabled = false;
                ctrlHold.BackgroundImage = Properties.Resources.Shape18_16;
                ShowOSDInfos("Sidebar wird fest gehalten");
            }
            else
            {
                //Automatisches ausblenden aktivieren
                timerSlide.Enabled = true;
                ctrlHold.BackgroundImage = Properties.Resources.Shape57_16;
                ShowOSDInfos("Sidebar wird automatisch ausgeblendet");
            }
        }

        public void ctrlFav_Click(object sender, EventArgs e)
        {
            if (StreamPlaying == false)
            {
                //Aktuellen Titel zur Liste hinzufügen
                if (lstTitles.Items.Count > 0 && ActualFile != "")
                {
                    AddnewFavourite(ActualFile);
                    ShowOSDInfos("Favourit wurde hinzugefügt");
                }
            }
            else
            {
                //Stream wird gespielt

            }
        }

        private void lstFavourites_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (LoadingNewFile == false)
            {
                var senderList = (ListView)sender;
                var clickedItem = senderList.HitTest(e.Location).Item;
                bool Favfound = false;
                if (clickedItem != null)
                {
                    //in playliste nach diesem titel suchen
                    for (int i = 0; i < lstTitles.Items.Count; i++)
                    {
                        if (clickedItem.Text == lstTitles.Items[i].Text)
                        {
                            ActualIndex = i;
                            Favfound = true;
                            break;
                        }
                    }
                    //wurde evtl aus der liste entfernt
                    if (Favfound == false)
                    {
                        if (File.Exists(@clickedItem.Text))
                        {
                            //Titel zur Liste hinzufügen
                            AddnewTitle(clickedItem.Text);
                            Favfound = true;
                            string[] Founditems = new string[1];
                            Founditems[0] = clickedItem.Text;
                            PopulateTreeView(tvPlaylist, Founditems, '\\');
                            tvPlaylist.Sort();
                            ActualIndex = lstTitles.Items.Count - 1;
                        }
                    }
                    //wenn gefunden diesen starten
                    if (Favfound == true)
                        Playthis(clickedItem.Text);
                    else
                        MessageBox.Show("Favourit kann nicht gefunden werden! Datei wurde möglicherweise gelöscht.", "Fehler");
                }
            }
        }

        private void lstFavourites_KeyUp(object sender, KeyEventArgs e)
        {
            //Favourit entfernen
            int delCount = 0;
            if (e.KeyCode == Keys.Delete)
            {
                foreach (ListViewItem listViewItem in ((ListView)sender).SelectedItems)
                {
                    listViewItem.Remove();
                    delCount++;
                }
            }
            if (delCount > 0)
                ShowOSDInfos("" + delCount + " Favouriten wurden entfernt");
        }

        private void gewähltenTitelZuFavouritenHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvPlaylist.SelectedNode != null)
            {
                //Aktuellen Titel zur Liste hinzufügen
                AddnewFavourite(tvPlaylist.SelectedNode.FullPath);
                ShowOSDInfos("Favourit wurde hinzugefügt");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            popupDelete.Show(this, pnlControls.Location.X + btnDelete.Location.X,
                                   pnlControls.Location.Y + btnDelete.Location.Y - 70);
        }

        private void playlisteLeerenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "Soll die Playliste komplett geleert werden?", "Playliste leeren",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                //stoppen und Liste leeren
                if (lstTitles.Items.Count > 0)
                {
                    if (LMPlayerform.VLCMediaplayer.State == VLCState.Playing || LMPlayerform.VLCMediaplayer.State == VLCState.Paused)
                        ctrlStop_Click(null, null);
                    lstTitles.Items.Clear();
                    tvPlaylist.Nodes.Clear();
                    ActualFile = "";
                    ActualIndex = -1;
                    ShowOSDInfos("Playliste geleert");
                }
            }
        }

        private void popupTreeview_Opening(object sender, CancelEventArgs e)
        {
            popupTreeview.BackColor = Color.Black;
            popupTreeview.ForeColor = Color.DarkGray;
            try
            {
                aktuellenTitelFindenToolStripMenuItem.Visible = ActualFile != "";
                if (tvPlaylist.SelectedNode != null)
                {
                    string SelectedFile = tvPlaylist.SelectedNode.FullPath;
                    ordnerNachNeuenDateienDurchsuchenToolStripMenuItem.Visible = Directory.Exists(@SelectedFile);
                    gewähltenTitelZuFavouritenHinzufügenToolStripMenuItem.Visible = File.Exists(@SelectedFile);
                }
                else
                {
                    ausPlaylisteEntfernenToolStripMenuItem.Visible = false;
                    ordnerNachNeuenDateienDurchsuchenToolStripMenuItem.Visible = false;
                    gewähltenTitelZuFavouritenHinzufügenToolStripMenuItem.Visible = false;
                }
            }
            finally
            { }
        }

        private void ordnerNachNeuenDateienDurchsuchenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvPlaylist.SelectedNode != null)
            {
                //Alle dateien aus dem Ordner auflisten und mit Playliste vergleichen
                //todo - wäre als thread besser....
                string SelectedFile = tvPlaylist.SelectedNode.FullPath;
                List<String> Ffiles = new List<String>();
                String[] extensions = new String[] { "*.mp3", "*.avi", "*.mp4", "*.mkv", "*.flv" };

                //Alle dateien einlesen in diesem Ordner und unterordnern
                foreach (String extension in extensions)
                {
                    String[] files = Directory.GetFiles(SelectedFile, extension, SearchOption.AllDirectories);
                    foreach (String file in files)
                        Ffiles.Add(file);
                }
                bool FFound = false;
                int FoundNR = 0;

                //Schauen ob diese bereits in der liste sind
                for (int i = 0; i < Ffiles.Count; i++)
                {
                    FFound = false;
                    for (int x = 0; x < lstTitles.Items.Count; x++)
                    {
                        if (Ffiles[i] == lstTitles.Items[x].Text)
                        {
                            //ist bereits in der liste
                            FFound = true;
                            break;
                        }
                    }

                    //ist noch nicht in der Liste, also hinzufügen
                    if (FFound == false)
                    {
                        AddnewTitle(Ffiles[i]);
                        string[] Founditems = new string[1];
                        Founditems[0] = Ffiles[i];
                        PopulateTreeView(tvPlaylist, Founditems, '\\');
                        FoundNR++;
                    }
                }

                //Anzeige wieviele neue Titel hinzugefügt wurden
                if (FoundNR > 0)
                {
                    tvPlaylist.Sort();
                    ShowOSDInfos("" + FoundNR + " Titel hinzugefügt");
                }
                else
                    ShowOSDInfos("Keine neuen Titel gefunden");
            }
        }

        private void defekteTitelEntfernenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //entweder aus playliste oder favouritenliste            
            ListView inThisList = null;
            if (pnlPlaylist.Visible)
                inThisList = lstTitles;
            else if (pnlFavourites.Visible)
                inThisList = lstFavourites;
            if (inThisList != null)
            {
                int x = 0;

                if (inThisList.Items.Count > 0)
                {
                    //Titel die nicht mehr auffindbar sind aus liste entfernen
                    for (int i = inThisList.Items.Count - 1; i > 0; i--)
                        if (File.Exists(@inThisList.Items[i].Text) == false)
                        {
                            inThisList.Items.RemoveAt(i);
                            x++;
                        }
                    if (x > 0)
                        ShowOSDInfos("" + x + " Titel wurden entfernt");
                    else
                        ShowOSDInfos(" Alle Titel sind OK");
                }

                //titel wurden aus playliste entfernt also actualindex aktualisieren
                if (pnlPlaylist.Visible)
                    FindActualTitle();
            }
        }

        private void UnselectAll(ListView inThisList)
        {
            //Alle deselektieren
            if (inThisList.SelectedIndices.Count > 0)
                for (int i = 0; i < inThisList.SelectedIndices.Count; i++)
                {
                    inThisList.Items[inThisList.SelectedIndices[i]].Selected = false;
                }
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (lstTitles.Items.Count > 0)
            {
                //Suchen in sichtbarer Liste, playlist oder favouriten liste
                if (pnlPlaylist.Visible)
                    //In Playliste suchen
                    SearchInListView(lstTitles);     
                else if (pnlFavourites.Visible)
                    //In Favouriten suchen
                    SearchInListView(lstFavourites); 
                else if (pnlTreeview.Visible)
                {
                    //In Treeview suchen -- noch nicht ganz fertig, findet nur erstes soweit
                    if (SearchPosNodes == null)
                        SearchPosNodes = tvPlaylist.Nodes;
                    TreeNode foundNode = SearchInTreeview(SearchPosNodes, edtSearchInPlaylist.Text.ToLower());
                    if (foundNode != null)
                    {
                        //und sichtbar machen 
                        tvPlaylist.SelectedNode = foundNode;
                        foundNode.EnsureVisible();
                    }
                }
                else
                    ShowOSDInfos("Suche nur in Playlist, Treeview & Favouriten");
            }
        }

        private TreeNode SearchInTreeview(TreeNodeCollection nodes, string searchtext)
        {
            //TreeNode finden nach Namen
            TreeNode foundNode = null;
            bool nodeFound = false;
         
            foreach (TreeNode node in nodes)
            {                
                if (node.FullPath.ToLower().Contains(searchtext))
                {
                    nodeFound = true;
                    foundNode = node;
                    SearchPosNodes = node.Nodes;
                    return foundNode;
                }
                if (!nodeFound)
                {
                    foundNode = SearchInTreeview(node.Nodes, searchtext);
                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }
                //zurück auf null
                //if (node == node.LastNode)
                //    SearchPosNodes = null;
            }
            return null;
        }

        private void SearchInListView(ListView inThisList)
        {
            //Suchen in Liste
            if (SearchPos >= inThisList.Items.Count - 1)
                SearchPos = 0;
            else
                SearchPos++;
            for (int i = SearchPos; i < inThisList.Items.Count - 1; i++)
            {
                String title = inThisList.Items[i].Text.ToLower();
                if (title.Contains(edtSearchInPlaylist.Text.ToLower()))
                {
                    //gefundenen titel sichtbar machen und selecten
                    UnselectAll(inThisList);
                    inThisList.Items[i].Selected = true;
                    inThisList.EnsureVisible(i);
                    SearchPos = i;
                    break;
                }
                //wieder von vorne starten mit suche
                if (i >= inThisList.Items.Count - 2)
                    SearchPos = 0;
            }

        }

        private void edtSearchInPlaylist_TextChanged(object sender, EventArgs e)
        {
            SearchPos = 0;
        }

        private void edtSearchInPlaylist_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnFind_Click(null, null);
        }

        // Compares two ListView items based on a selected column.
        public class ListViewComparer : System.Collections.IComparer
        {
            private int ColumnNumber;
            private SortOrder SortOrder;
            public ListViewComparer(int column_number,
                SortOrder sort_order)
            {
                ColumnNumber = column_number;
                SortOrder = sort_order;
            }

            // Compare 
            public int Compare(object object_x, object object_y)
            {
                ListViewItem item_x = object_x as ListViewItem;
                ListViewItem item_y = object_y as ListViewItem;
                string string_x;
                if (item_x.SubItems.Count <= ColumnNumber)
                    string_x = "";
                else
                    string_x = item_x.SubItems[ColumnNumber].Text;
                string string_y;
                if (item_y.SubItems.Count <= ColumnNumber)
                    string_y = "";
                else
                    string_y = item_y.SubItems[ColumnNumber].Text;
                int result;
                double double_x, double_y;
                if (double.TryParse(string_x, out double_x) &&
                    double.TryParse(string_y, out double_y))
                    result = double_x.CompareTo(double_y);
                else
                {
                    DateTime date_x, date_y;
                    if (DateTime.TryParse(string_x, out date_x) &&
                        DateTime.TryParse(string_y, out date_y))
                        result = date_x.CompareTo(date_y);
                    else
                        result = string_x.CompareTo(string_y);
                }
                if (SortOrder == SortOrder.Ascending)
                    return result;
                else
                    return -result;
            }
        }

        private void lstTitles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //sortieren nach Column clicked
            ColumnHeader new_sorting_column = lstTitles.Columns[e.Column];
            SortOrder sort_order;
            if (SortingColumn == null)
                sort_order = SortOrder.Ascending;
            else
            {
                if (new_sorting_column == SortingColumn)
                {
                    if (SortingColumn.Text.StartsWith("> "))
                        sort_order = SortOrder.Descending;
                    else
                        sort_order = SortOrder.Ascending;
                }
                else
                    sort_order = SortOrder.Ascending;
                SortingColumn.Text = SortingColumn.Text.Substring(2);
            }
            SortingColumn = new_sorting_column;
            if (sort_order == SortOrder.Ascending)
                SortingColumn.Text = "> " + SortingColumn.Text;
            else
                SortingColumn.Text = "< " + SortingColumn.Text;
            lstTitles.ListViewItemSorter =
                new ListViewComparer(e.Column, sort_order);
            lstTitles.Sort();

            //aktuellen titel wieder finden
            FindActualTitle();
            ColorizeActualTitle();
        }

        private TreeNode GetNodeByText(TreeNodeCollection nodes, string searchtext)
        {
            //TreeNode finden nach Namen
            TreeNode n_found_node = null;
            bool b_node_found = false;
            foreach (TreeNode node in nodes)
            {
                if (node.FullPath == searchtext)
                {
                    b_node_found = true;
                    n_found_node = node;
                    return n_found_node;
                }
                if (!b_node_found)
                {
                    n_found_node = GetNodeByText(node.Nodes, searchtext);
                    if (n_found_node != null)
                    {
                        return n_found_node;
                    }
                }
            }
            return null;
        }

        private void aktuellenTitelFindenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvPlaylist.Nodes.Count > 0 && ActualFile != "")
            {
                //aktuellen titel in treeview suchen             
                TreeNode tn = GetNodeByText(tvPlaylist.Nodes, ActualFile);

                //und sichtbar machen
                tvPlaylist.SelectedNode = tn;
                tn.EnsureVisible();
            }
        }


        private void ausPlaylisteEntfernenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvPlaylist.SelectedNode != null)
            {
                string QuestionText = "";

                //Fragetext unterschiedlich für Ordner oder Datei
                if (Directory.Exists(tvPlaylist.SelectedNode.FullPath))
                    QuestionText = "Möchten Sie alle Titel dieses Ordners aus der Playliste entfernen? \n\n"
                    + tvPlaylist.SelectedNode.FullPath;
                else
                    QuestionText = "Möchten Sie diesen Titel aus der Playliste entfernen? \n\n"
                    + tvPlaylist.SelectedNode.FullPath;

                //Abfrage ob dies wirklich entfernt werden soll 
                DialogResult dr = MessageBox.Show(
                    QuestionText, "Titel aus Playliste entfernen",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    bool wasStopped = false;

                    //treeview node entfernen, auch darüber wenn keine weiteren subnodes mehr da sind
                    string SelectedItem = tvPlaylist.SelectedNode.FullPath;
                    string upLevelNode = tvPlaylist.SelectedNode.FullPath;
                    upLevelNode = upLevelNode.Replace("\\" + tvPlaylist.SelectedNode.Text, "");
                    TreeNode tprev = GetNodeByText(tvPlaylist.Nodes, upLevelNode);
                    tvPlaylist.SelectedNode.Remove();
                    if (tprev.GetNodeCount(true) < 1)
                        tprev.Remove();

                    //und in Playlist diese auch entfernen
                    for (int i = lstTitles.Items.Count - 1; i > 0; i--)
                        if (lstTitles.Items[i].Text.Contains(SelectedItem))
                        {
                            if (lstTitles.Items[i].Index == ActualIndex)
                            {
                                ctrlStop_Click(null, null);
                                wasStopped = true;
                            }
                            lstTitles.Items.RemoveAt(i);
                        }

                    //aktuellen titel wieder finden
                    if (wasStopped == false)
                        FindActualTitle();
                }
            }
        }

        private void lstResults_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //bei doppelclick auf column 1 oder 2 wikipedia oder youtube url in browser öffnen
            var senderList = (ListView)sender;
            var clickedItem = senderList.HitTest(e.Location).Item;
            if (clickedItem != null)
            {
                ListViewHitTestInfo listViewHitTestInfo = senderList.HitTest(e.X, e.Y);
                int col = listViewHitTestInfo.Item.SubItems.IndexOf(listViewHitTestInfo.SubItem);
                if (col == 1 || col == 2)
                {
                    //URL in webbrowser starten
                    String URL = clickedItem.SubItems[col].Text;
                    Process.Start(URL);
                }
            }
        }

        private void popupDelete_Opening(object sender, CancelEventArgs e)
        {
            popupDelete.BackColor = Color.Black;
            popupDelete.ForeColor = Color.DarkGray;
        }

        public void btnSideSwitch_Click(object sender, EventArgs e)
        {
            //Sidebar auf andere Seite
            timerSlide.Enabled = false;
            if (Location.X == 0 || Location.X == -Size.Width)
            {
                btnSideSwitch.BackgroundImage = Properties.Resources.Left_Arrow_16;
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width, 0);
            }
            else
            {
                btnSideSwitch.BackgroundImage = Properties.Resources.Navigation_Right_16;
                Location = new Point(0, 0);
            }
            timerSlide.Enabled = true;
        }

        private void timerSlide_Tick(object sender, EventArgs e)
        {
            timerSlide.Enabled = false;

            //rechts - playliste ausblenden wenn Maus ausserhalb dieser ist
            if (MousePosition.X < Screen.PrimaryScreen.WorkingArea.Width - (Width + 10)
                && Location.X == Screen.PrimaryScreen.WorkingArea.Width - Width)
                HidePlaylist(true);

            //rechts - Playliste einblenden wenn Maus am Rand des Bildschirms ist
            else if (MousePosition.X >= Screen.PrimaryScreen.WorkingArea.Width - 2
                     && Location.X == Screen.PrimaryScreen.WorkingArea.Width)
                ShowPlaylist(true);

            //links - playliste ausblenden wenn Maus ausserhalb dieser ist 
            else if (MousePosition.X > Width + 10 && Location.X == 0)
                HidePlaylist(false);

            //links - Playliste einblenden wenn Maus am Rand des Bildschirms ist
            else if (MousePosition.X <= 2 && Location.X == -Size.Width)
                ShowPlaylist(false);
            timerSlide.Enabled = true;
        }

        private void ShowPlaylist(bool RightSide)
        {
            //SlideIN SoundEffekt abspielen
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(exePath);
            ThreadPool.QueueUserWorkItem(_ => VLCEffectplayer.Play(new Media(LMPlayerform.VLClib, @directory + @"\sounds\SlideIN.wav")));
            if (RightSide)
            {
                //rechts - Playliste einblenden
                for (int i = 0; i < Width / 2; i++)
                {
                    Location = new Point(Location.X - 2, 0);
                    Thread.Sleep(1);
                }
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width, 0);
            }
            else
            {
                //links - Playliste einblenden            
                for (int i = 0; i < Width / 2; i++)
                {
                    Location = new Point(Location.X + 2, 0);
                    Thread.Sleep(1);
                }
                Location = new Point(0, 0);
            }
        }

        private void HidePlaylist(bool RightSide)
        {
            //SlideOUT SoundEffekt abspielen
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(exePath);
            ThreadPool.QueueUserWorkItem(_ => VLCEffectplayer.Play(new Media(LMPlayerform.VLClib, @directory + @"\sounds\SlideOUT.wav")));
            if (RightSide)
            {
                //rechts - Playliste ausblenden
                for (int i = 0; i < Width / 2; i++)
                {
                    Location = new Point(Location.X + 2, 0);
                    Thread.Sleep(1);
                }
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width, 0);
            }
            else
            {
                //links - Playliste ausblenden              
                for (int i = 0; i < Width / 2; i++)
                {
                    Location = new Point(Location.X - 2, 0);
                    Thread.Sleep(1);
                }
                Location = new Point(-Size.Width, 0);
            }
        }

        public void btnClickThrough_Click(object sender, EventArgs e)
        {
            if (isClickThrough)
            {
                //zurück zu normal
                ShowOSDInfos("Player nicht mehr durchklickbar");
                uint initialStyle = GetWindowLong(LMPlayerform.Handle, -20);
                SetWindowLong(LMPlayerform.Handle, -20, initialStyle & 0x80000);
                LMPlayerform.timerHideControls.Enabled = true;
                LMPlayerform.TopMost = false;
                isClickThrough = false;
            }
            else
            {
                //fenster durchklickbar
                //so kann der Player zb beim Spielen eines Spieles im Vordergrund sein, und macht keine Probleme
                ShowOSDInfos("Player durchklickbar");
                uint initialStyle = GetWindowLong(LMPlayerform.Handle, -20);
                SetWindowLong(LMPlayerform.Handle, -20, initialStyle | 0x80000 | 0x20);
                LMPlayerform.TopMost = true;
                LMPlayerform.timerHideControls.Enabled = false;
                LMPlayerform.pnlControls.Hide();
                isClickThrough = true;
            }
        }

        private void edtSearchText_KeyUp(object sender, KeyEventArgs e)
        {
            //Similiar suche starten
            if (e.KeyCode == Keys.Enter)
                btnSearch_Click(null, null);
        }


        private void ToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            //wird zwar ausgeführt, funktioniert aber nicht, gott weiß warum
            (sender as ToolStripMenuItem).BackColor = Color.DarkBlue;
            (sender as ToolStripMenuItem).ForeColor = Color.White;
        }

        private void ToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            (sender as ToolStripMenuItem).BackColor = Color.Black;
            (sender as ToolStripMenuItem).ForeColor = Color.DarkGray;
        }

        private void RecentTitleClick(object sender, EventArgs e)
        {
            //zuletzt gespielten titel starten
            ActualFile = (sender as ToolStripMenuItem).Text;
            FindActualTitle();
            Playthis(ActualFile);
        }

        private void popupPlaylist_Opening(object sender, CancelEventArgs e)
        {
            (sender as ContextMenuStrip).BackColor = Color.Black;
            (sender as ContextMenuStrip).ForeColor = Color.DarkGray;

            //Menuitems mit zuletzt gespielten Titeln erstellen
            zuletztGespielteTitelToolStripMenuItem.DropDownItems.Clear();
            for (int i = 0; i < RecentlyPlayedTitles.Count; i++)
            {
                if (RecentlyPlayedTitles[i] != "")
                {
                    ToolStripMenuItem cms = new ToolStripMenuItem();
                    cms.BackColor = Color.Black;
                    cms.ForeColor = Color.DarkGray;
                    cms.Click += RecentTitleClick;
                    cms.Text = RecentlyPlayedTitles[i];
                    zuletztGespielteTitelToolStripMenuItem.DropDownItems.Add(cms);
                }
            }
        }

        private void tvPlaylist_KeyUp(object sender, KeyEventArgs e)
        {
            //in treeview selected aus playliste entfernen
            if (e.KeyCode == Keys.Delete)
                ausPlaylisteEntfernenToolStripMenuItem_Click(null, null);
        }

        private void aktuellenTitelSuchenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //zu aktuellem Titel scrollen
            if (lstTitles.Items.Count > 0)
                lstTitles.EnsureVisible(ActualIndex);
        }

        private void ganzNachObenScrollenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ganz nach oben in der Playlist scrollen
            if (lstTitles.Items.Count > 0)
                lstTitles.EnsureVisible(0);
        }

        private void ganzNachUntenScrollenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ganz nach unten in der Playlist scrollen
            if (lstTitles.Items.Count > 0)
                lstTitles.EnsureVisible(lstTitles.Items.Count - 1);
        }

        private void lstTitles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            //um item zu verschieben in der liste
            privateDrag = true;
            if (pnlPlaylist.Visible)
                DoDragDrop(lstTitles.SelectedItems, DragDropEffects.Move);
            else
                DoDragDrop(lstFavourites.SelectedItems, DragDropEffects.Move);
            privateDrag = false;
        }

        private void lstTitles_DragEnter(object sender, DragEventArgs e)
        {
            //um item zu verschieben in der liste
            if (privateDrag) e.Effect = e.AllowedEffect;
        }

        private void lstTitles_DragDrop(object sender, DragEventArgs e)
        {
            //je nachdem welche Liste gerade sichtbar ist
            ListView SelectedList = null;
            if (pnlPlaylist.Visible)
                SelectedList = lstTitles;
            else
                SelectedList = lstFavourites;

            if (SelectedList != null)
            {
                // items verschieben
                var pos = SelectedList.PointToClient(new Point(e.X, e.Y));
                var hit = SelectedList.HitTest(pos);
                if (hit != null)
                {
                    ListViewItem[] sel = new ListViewItem[SelectedList.SelectedItems.Count];
                    for (int i = 0; i <= SelectedList.SelectedItems.Count - 1; i++)
                    {
                        sel[i] = SelectedList.SelectedItems[i];
                    }
                    for (int i = 0; i < sel.GetLength(0); i++)
                    {
                        ListViewItem dragItem = sel[i];
                        int itemIndex = hit.Item.Index;
                        if (itemIndex == dragItem.Index)
                        {
                            return;
                        }
                        if (dragItem.Index < itemIndex)
                            itemIndex++;
                        else
                            itemIndex = hit.Item.Index + i;
                        ListViewItem insertItem = (ListViewItem)dragItem.Clone();
                        SelectedList.Items.Insert(itemIndex, insertItem);
                        SelectedList.Items.Remove(dragItem);
                    }

                    //aktuellen Titel wieder finden
                    FindActualTitle();
                }
            }
        }

        private void gewähltenTitelZuFavouritenHinzufügenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //geklickten Titel zu Favouriten hinzufügen
            if (lstTitles.Items.Count > 0)
            {
                for (int i = 0; i < lstTitles.SelectedItems.Count; i++)
                    AddnewFavourite(lstTitles.SelectedItems[i].Text);
                ShowOSDInfos("Favourit wurde hinzugefügt");
            }
        }


        private void ausgewählteTitelEntfernenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int delCount = 0;
            bool wasStopped = false;
            //Titel entfernen und stoppen wenn dieser aktuell läuft
            for (int i = 0; i < lstTitles.SelectedItems.Count; i++)
                if (lstTitles.SelectedItems[i].Index == ActualIndex)
                {
                    ctrlStop_Click(null, null);
                    wasStopped = true;
                }
            foreach (ListViewItem listViewItem in lstTitles.SelectedItems)
            {
                //auch alle dazugehörigen TreeNodes entfernen
                TreeNode tn = GetNodeByText(tvPlaylist.Nodes, listViewItem.Text);
                if (tn != null)
                {
                    tvPlaylist.SelectedNode = tn;
                    string upLevelNode = tvPlaylist.SelectedNode.FullPath;
                    upLevelNode = upLevelNode.Replace("\\" + tn.Text, "");
                    TreeNode tprev = GetNodeByText(tvPlaylist.Nodes, upLevelNode);
                    tn.Remove();
                    if (tprev.GetNodeCount(true) < 1)
                        tprev.Remove();
                }
                listViewItem.Remove();
                delCount++;
            }
            if (delCount > 0)
                ShowOSDInfos("" + delCount + " Titel wurden entfernt");
            if (lstTitles.Items.Count == 0)
            {
                ActualIndex = -1;
                ActualFile = "";
            }
            else //aktuellen index wieder finden - könnte sich geändert haben
                if (wasStopped == false)
                FindActualTitle();
        }

        private void lstStreams_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (LoadingNewFile == false)
            {
                //geklickten Stream starten
                var senderList = (ListView)sender;
                var clickedItem = senderList.HitTest(e.Location).Item;
                if (clickedItem != null)
                {
                    //check ob Internetverbindung besteht
                    if (InternetCS.IsConnectedToInternet() == false)
                        ShowOSDInfos("Internetverbindung nicht möglich!");
                    else
                    {
                        ctrlStop_Click(null, null);
                        //Stream starten
                        PlaythisStream(lstStreams.Items[clickedItem.Index].SubItems[2].Text, clickedItem.Index);
                    }
                }
            }
        }

        private void ShowvideoWindow()
        {
            //Videofenster anzeigen
            if (LMPlayerform.VLCVideowindow.Visible == false)
            {
                animator1.Show(LMPlayerform.VLCVideowindow);
                animator1.WaitAllAnimations();
            }
            LMPlayerform.VLCVideowindow.BringToFront();
            LMPlayerform.pnlControls.BringToFront();
        }

        private void HideVideoWindow()
        {
            //Videofenster ausblenden
            if (LMPlayerform.VLCVideowindow.Visible == true)
            {
                animator1.AnimationType = AnimationType.Scale;
                if (LMPlayerform.Fullscreen)
                    LMPlayerform.VLCVideowindow_DoubleClick(null, null);
                animator1.Hide(LMPlayerform.VLCVideowindow);
                animator1.WaitAllAnimations();
            }
        }

        private void HideCovers()
        {
            //Cover ausblenden
            if (LMPlayerform.pbCover.Visible == true)
            {
                animator1.Hide(LMPlayerform.pbCover);
                animator1.WaitAllAnimations();
            }

            //Waterimg ausblenden
            if (LMPlayerform.waterImg.Visible == true)
            {
                animator1.Hide(LMPlayerform.waterImg);
                animator1.WaitAllAnimations();
            }
            animator1.AnimationType = AnimationType.Scale;
        }

        private void ShowWaterImage()
        {
            //Wasser effect bild zeigen
            if (LMPlayerform.waterImg.Visible == false)
            {
                LMPlayerform.waterImg.ImageBitmap = (Bitmap)Properties.Resources.coverWide;
                LMPlayerform.waterImg.BringToFront();
                LMPlayerform.pnlControls.BringToFront();
                animator1.Show(LMPlayerform.waterImg);
                animator1.WaitAllAnimations();
            }
        }

        private void ResetCovers()
        {
            //Covers überall auf Standard zurück setzen
            LMPlayerform.pbCover.BackgroundImage = Properties.Resources.coverWide;
            LMPlayerform.waterImg.ImageBitmap = (Bitmap)Properties.Resources.coverWide;
            pbCoverPl.BackgroundImage = LMPlayerform.pbCover.BackgroundImage;
            LMOSD.pbOSDCover.BackgroundImage = LMPlayerform.pbCover.BackgroundImage;
        }

        private void PlaythisStream(string URL, int StreamIndex)
        {
            if (LoadingNewFile == false)
                try
                {
                    LoadingNewFile = true;
                    bool isVideo = false;
                    if (lstStreams.Items[StreamIndex].SubItems[1].Text == "Video")
                        isVideo = true;
                    if (isVideo)
                    {
                        HideCovers();
                        ShowvideoWindow();
                    }
                    else
                    {
                        HideVideoWindow();
                        ShowWaterImage();
                    }
                    ResetCovers();

                    //Streamtitel anzeigen                
                    ActualStream = lstStreams.Items[StreamIndex].Text;
                    ActualStreamIndex = StreamIndex;
                    ColorizeActualStream();
                    ShowOSDInfos(ActualStream);

                    //Stream starten
                    ThreadPool.QueueUserWorkItem(_ => LMPlayerform.VLCMediaplayer.Stop());
                    ThreadPool.QueueUserWorkItem(_ => LMPlayerform.VLCMediaplayer.Play(new Media(LMPlayerform.VLClib, new Uri(URL))));
                    StreamPlaying = true;
                }
                catch (Exception err)
                {
                    ShowOSDInfos(err.Message);
                }
                finally
                {
                    LoadingNewFile = false;
                }
        }

        private void ColorizeActualStream()
        {
            //aktuellen Stream farblich hervorheben
            for (int i = 0; i < lstStreams.Items.Count; i++)
            {
                if (i == ActualStreamIndex)
                    lstStreams.Items[i].ForeColor = Color.Lime;
                else
                    lstStreams.Items[i].ForeColor = Color.White;
            }
        }

    }

}
