using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Runtime.InteropServices;
using TagLib;
using NAudio;
using NAudio.Wave;
using MMP.Hotkey;
using MMP.MenuHandlers;

namespace MMP {
    // This is public so you can change it in the settings.
    public enum ColorMode {
        Random,
        Static,
        System,
        Image
    }

    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        bool playing = false;
        bool loaded = false;
        WaveOut wave;
        WaveStream strm;
        WaveChannel32 wvcn;

        GlobalHotkey hk_MediaNext;
        GlobalHotkey hk_MediaPrev;
        GlobalHotkey hk_MediaPlay;

        GlobalHotkey hk_StandNext;
        GlobalHotkey hk_StandPrev;
        GlobalHotkey hk_StandPlay;

        float curval = 0;
        float nextval = 0;

        float curpos = 0;
        float nextpos = 0;

        float cursel = 0;
        public float nextsel = 0;
        bool mouseInside = false;

        float curact = 0;
        public float nextact = 0;

        float curanim = 0;
        float nextanim = 1;

        public bool selecting = false;

        Point lastLocation = new Point();
        bool windowMoving = false;

        public float mouseDistance = 0;
        bool inCenter = false;

        MenuController menuControl;
        SongHandler songHandler;

        ColorMode colorMode = ColorMode.Random;

        Song currentSong;
        Playlist currentPlaylist = new Playlist();
        public bool playlistActive;
        int currentIndex = 0;

        Font MenuSelectionFont = new Font("Verdana", 21f);
        SolidBrush InnerBrush_NoImage = new SolidBrush(Color.FromArgb(255, Color.White));
        SolidBrush InnerBrush_Image = new SolidBrush(Color.FromArgb(128, Color.White));

        Image SongInformation = new Bitmap(200, 200);
        Font SongInfoTitleFont = new Font("Verdana", 14f);
        Font SongInfoSubFont = new Font("Verdana", 12f);

        private float volume = 0.5f;

        public float Volume {
            get {
                return volume;
            }
            set {
                volume = value;
                wave.Volume = value;
            }
        }
        public long Position {
            get {
                return wvcn.Length * (wvcn.CurrentTime.Milliseconds / wvcn.TotalTime.Milliseconds);
            }
            set {
                wvcn.Seek(value, SeekOrigin.Begin);
            }
        }
        public long SongLength {
            get {
                return wvcn.Length;
            }
        }
        public bool Paused {
            get {
                return !playing;
            }
            set {
                if (loaded) {
                    if (value)
                        wave.Pause();
                    else
                        wave.Play();
                    playing = !value;
                }
            }
        }
        public bool ShowSongInfo = false;

        double cred = 128, cgreen = 128, cblue = 128, nred = 128, ngreen = 128, nblue = 128, rred, rgreen, rblue;

        public Color CurrentColor {
            get {
                return Color.FromArgb((int)cred, (int)cgreen, (int)cblue);
            }
            set {
                nred = value.R;
                ngreen = value.G;
                nblue = value.B;
            }
        }

        Color SysColor = Color.FromArgb(0);
        Color SongColor = Color.FromArgb(0);

        private void Form1_Load(object sender, EventArgs e) {
            timer1.Interval = 1000 / 60;
            int argb = (int)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM","ColorizationColor", null);
            SysColor = Color.FromArgb(255, Color.FromArgb(argb));
            hk_MediaNext = new GlobalHotkey(Constants.NOMOD, Keys.MediaNextTrack, this);
            hk_MediaPrev = new GlobalHotkey(Constants.NOMOD, Keys.MediaPreviousTrack, this);
            hk_MediaPlay = new GlobalHotkey(Constants.NOMOD, Keys.MediaPlayPause, this);
            hk_StandNext = new GlobalHotkey(Constants.CTRL | Constants.SHIFT, Keys.F12, this);
            hk_StandPrev = new GlobalHotkey(Constants.CTRL | Constants.SHIFT, Keys.F11, this);
            hk_StandPlay = new GlobalHotkey(Constants.CTRL | Constants.SHIFT, Keys.F10, this);
            hk_MediaNext.Register();
            hk_MediaPrev.Register();
            hk_MediaPlay.Register();
            hk_StandNext.Register();
            hk_StandPrev.Register();
            hk_StandPlay.Register();
            Enum.TryParse(Properties.MMP.Default.DrawMode, out colorMode);
            menuControl = new MenuController(this);
            songHandler = new SongHandler(this);
            menuControl.Add(new VolumeMenuHandler(songHandler, menuControl));
            menuControl.Add(new NextSongMenuHandler(songHandler, menuControl));
            menuControl.Add(new PositionMenuHandler(songHandler, menuControl));
            menuControl.Add(new TrackMenuHandler(songHandler, menuControl));
            menuControl.Add(new PreviousSongMenuHandler(songHandler, menuControl));
            menuControl.Add(new CloseMenuHandler(songHandler, menuControl));
            menuControl.DoCreate();
            Paused = true;
        }

        protected override void WndProc(ref Message m) {
            if(m.Msg == Constants.WM_HOTKEY_MSG_ID)
                HandleHotkey();
            base.WndProc(ref m);
        }

        private void HandleHotkey() {
            if (Keyboard.IsKeyDown(Keys.MediaNextTrack) || Keyboard.IsKeyDown(Keys.F12))
                LoadNext();
            if (Keyboard.IsKeyDown(Keys.MediaPreviousTrack) || Keyboard.IsKeyDown(Keys.F11))
                LoadPrevious();
            if (Keyboard.IsKeyDown(Keys.MediaPlayPause) || Keyboard.IsKeyDown(Keys.F10))
                Paused = !Paused;
        }

        public void SongDialog() {
            if (loaded) {
                Paused = true;
            }
            using (OpenFileDialog fd = new OpenFileDialog()) {
                fd.InitialDirectory = "c://";
                fd.Multiselect = true;
                fd.Filter = "MP3 Files|*.mp3|WAV Files|*.wav|Playlist Files|*.mpl";
                if (fd.ShowDialog() == DialogResult.OK) {
                    loaded = false;
                    if (fd.FileNames.Length < 2) {
                        if (Path.GetExtension(fd.FileName).IndexOf("mpl") >= 0)
                            LoadPlaylist(fd.FileName);
                        else
                            LoadSong(fd.FileName);
                    } else {
                        if (playlistActive) {
                            if (MessageBox.Show("Add songs to current playlist?", "Add to playlist", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                                foreach (string url in fd.FileNames) {
                                    currentPlaylist.AddSong(url);
                                }
                                currentPlaylist.Shuffle();
                                if (MessageBox.Show("Save Playlist?", "Save Playlist", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                    if (currentPlaylist.PlaylistURL != "")
                                        if (MessageBox.Show("Overwrite?", "Overwrite", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                            currentPlaylist.Save();
                                        else
                                            currentPlaylist.Save(true);
                                    else
                                        currentPlaylist.Save();
                            } else {
                                currentPlaylist = new Playlist();
                                foreach (string url in fd.FileNames) {
                                    currentPlaylist.AddSong(url);
                                }
                            }
                        } else {
                            currentPlaylist = new Playlist();
                            foreach (string url in fd.FileNames) {
                                currentPlaylist.AddSong(url);
                            }
                            currentPlaylist.Shuffle();
                        }
                        playlistActive = true;
                        currentIndex = 0;
                        LoadSong(currentPlaylist.Songs[0].url, true);
                    }
                }
            }
        }

        public void LoadPlaylist(string url) {
            currentPlaylist.Load(url);
            playlistActive = true;
            currentIndex = 0;
            LoadSong(currentPlaylist.Songs[0].url, true);
        }

        public void LoadNext() {
            if (!playlistActive) return;
            currentIndex = (currentIndex + 1) % currentPlaylist.Songs.Count;
            LoadSong(currentPlaylist.Songs[currentIndex].url, true);
        }
        public void LoadPrevious() {
            if (!playlistActive) return;
            currentIndex = (currentIndex - 1) < 0 ? currentPlaylist.Songs.Count - 1 : currentIndex - 1;
            LoadSong(currentPlaylist.Songs[currentIndex].url, true);
        }

        public void LoadSong(string url, bool fromPlaylist = false) {
            string ext = Path.GetExtension(url);
            if (loaded) {
                wave.Dispose();
                wvcn.Dispose();
                strm.Dispose();
            }
            if (ext.IndexOf("wav") >= 0) {
                strm = new WaveFileReader(url);
            } else if (ext.IndexOf("mp3") >= 0) {
                strm = new Mp3FileReader(url);
            } else {
                return;
            }
            if (!fromPlaylist && playlistActive) {
                DialogResult res = MessageBox.Show("Add this song to the current Playlist?", "Update Playlist", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes) {
                    currentPlaylist.AddSong(url);
                    DialogResult mes = MessageBox.Show("Save Playlist?", "Save Playlist", MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes) {
                        currentPlaylist.Save();
                    }
                } else {
                    playlistActive = false;
                }
            }
            wave = new WaveOut();
            wvcn = new WaveChannel32(strm);
            wvcn.PadWithZeroes = false;
            wvcn.Sample += new EventHandler<SampleEventArgs>(SampleEvent);
            wave.PlaybackStopped += (pbss, e) => {
                playing = false;
                nextpos = 0;
                LoadNext();
            };
            wave.Volume = volume;
            wave.Init(wvcn);
            loaded = true;
            Paused = false;
            currentSong = new Song(url, true);
            if (currentSong.HasArt) {
                SongColor = ImageProcessor.AverageColor(new Bitmap(currentSong.art));
                colorMode = ColorMode.Image;
            } else {
                colorMode = ColorMode.Random;
            }
            wave.Play();
            menuControl.DoSongLoaded(currentSong);

            using (Graphics g = Graphics.FromImage(SongInformation)) {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                StringFormat centered = new StringFormat();
                centered.Alignment = StringAlignment.Center;
                centered.LineAlignment = StringAlignment.Center;
                RectangleF borp = new RectangleF(0, 10, SongInformation.Width, SongInformation.Height);
                borp.Inflate(-5, -10);
                g.FillRectangle(Brushes.Lime, borp);
                int amt = 1 + (currentSong.album != null ? 1 : 0) + (currentSong.albumArtists != "" ? 1 : 0);
                borp.Height /= amt;
                g.DrawString(currentSong.title, SongInfoTitleFont, Brushes.Black, borp, centered);
                borp.Y += borp.Height;
                if (currentSong.album != null) {
                    g.DrawString(currentSong.album, SongInfoSubFont, Brushes.Black, borp, centered);
                    borp.Y += borp.Height + 5;
                }
                if (currentSong.albumArtists != "") {
                    g.DrawString(currentSong.albumArtists, SongInfoSubFont, Brushes.Black, borp, centered);
                    borp.Y += borp.Height + 5;
                }
            }
        }

        private void SampleEvent(object sender, SampleEventArgs e) {
            nextval = Math.Abs(e.Left + e.Right / 2);
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (playing) {
                nextpos = (float)(strm.CurrentTime.TotalMilliseconds / strm.TotalTime.TotalMilliseconds);
            }
            this.Invalidate();
        }

        const int maxcoltick = 180;
        int curtick = 0;

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            curval = (curval + nextval) / 2;
            curpos = (curpos + nextpos) / 2;
            cursel = (cursel + nextsel) / 2;
            curact = (curact + nextact) / 2;
            curanim = (curanim + nextanim) / 2f;
            if (curtick == maxcoltick) {
                Random r = new Random();
                

                nred = r.Next(255);
                ngreen = r.Next(255);
                nblue = r.Next(255);

                rred = (nred - cred) / maxcoltick;
                rgreen = (ngreen - cgreen) / maxcoltick;
                rblue = (nblue - cblue) / maxcoltick;

                curtick = 0;
            }
            curtick++;
            cred += rred;
            cgreen += rgreen;
            cblue += rblue;
            Rectangle rect = e.ClipRectangle;
            Bitmap b = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage(b);
            Rectangle q = rect;
            q.Inflate(-50, -50);
            Brush RandomColorBG = new SolidBrush(Color.FromArgb((int)Clamp(cred + 32, 0, 255), (int)Clamp(cgreen + 32, 0, 255), (int)Clamp(cblue + 32, 0, 255)));
            if(loaded)
                if(currentSong.HasArt)
                    e.Graphics.DrawImage(currentSong.art, q);
                else
                    e.Graphics.FillRectangle(colorMode == ColorMode.System ? new SolidBrush(SysColor) : (colorMode == ColorMode.Image ? new SolidBrush(SongColor) : RandomColorBG), q);
            Rectangle c = rect;
            if (!Paused) {
                float per = curval > 1 ? 1 : curval;
                c.Inflate(-50, -50);
                c.Inflate((int)(50 * per), (int)(50 * per));
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            Brush RandomColor = new SolidBrush(Color.FromArgb((int)cred, (int)cgreen, (int)cblue));
            g.FillEllipse(colorMode == ColorMode.System ? new SolidBrush(SysColor) : (colorMode == ColorMode.Image ? new SolidBrush(SongColor) : RandomColor), rect);
            g.FillPie(new SolidBrush(Color.FromArgb(128, (SongColor.R + SongColor.G + SongColor.B) / 3 < 32 && colorMode != ColorMode.Random ? Color.White : Color.Black)), c, 270f, curpos * 360f);

            StringFormat strfmt = new StringFormat();
            strfmt.Alignment = StringAlignment.Center;
            strfmt.LineAlignment = StringAlignment.Center;

            if (selecting) {
                if (loaded && !menuControl.MenuActive && !inCenter && currentSong.HasArt)
                    e.Graphics.FillRectangle(InnerBrush_Image, q);
            }
            if (menuControl.MenuActive)
                menuControl.ActiveMenu.Draw(g, rect);
            if (curact > 0.5){
                for (int i = 0; i < menuControl.ActiveMenus; i++) {
                    g.FillPie(new SolidBrush(Color.FromArgb(64, Color.White)), rect, 270f + (i * curact), curact);
                    g.DrawPie(new Pen(Color.FromArgb(172, Color.White), 2), rect, 270f + (i * curact), curact);
                }
            }
            g.FillEllipse(Brushes.Lime, q);

            ImageAttributes ia = new ImageAttributes();
            ia.SetColorKey(Color.Green, Color.Lime);

            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix33 = curanim;
            ia.SetColorMatrix(matrix);
            if (loaded && ShowSongInfo && (selecting && inCenter)) {
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb((int)(128 * curanim), Color.White)), q);
                e.Graphics.DrawImage(SongInformation, new Rectangle(50, 50, 200, 200), 0f, 0f, 200f, 200f, GraphicsUnit.Pixel, ia);
            }

            ia.ClearColorMatrix();

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(b, rect, rect.X, rect.Y, rect.Width, rect.Height, GraphicsUnit.Pixel, ia);

            if (loaded && selecting && !menuControl.MenuActive && !inCenter) {
                e.Graphics.DrawString(menuControl[(int)((nextsel / 360f) * menuControl.ActiveMenus)].Label, MenuSelectionFont, Brushes.Black, new PointF(150f, 150f), strfmt);
            }
            g.Dispose();
            b.Dispose();
        }

        private double Clamp(double val, double min, double max) {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        private float DegreesBetweenPoints(Point p1, Point p2) {
            float n = (float)(Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180.0 / Math.PI) - 90f;
            return NormalizeDegrees(n);
        }
        private float NormalizeDegrees(float n) {
            return n < 0 ? 360f + n : n;
        }
        private float DistanceBetweenPoints(Point p1, Point p2) {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                SongDialog();
                return;
            }
            if (loaded) {
                if (selecting) {
                    if (inCenter)
                        Paused = !Paused;
                    else {
                        menuControl.SetActiveMenu((int)((nextsel / 360f) * menuControl.ActiveMenus));
                        if(menuControl.ActiveMenu.OnActivated()) {
                            menuControl.ShowControls = false;
                        }
                    }
                } else {
                    if (menuControl.MenuActive) {
                        menuControl.ActiveMenu.OnClick();
                    }
                }
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e) {
            if (mouseInside) {
                nextsel = DegreesBetweenPoints(new Point(e.X, e.Y), new Point(150, 150));
                mouseDistance = DistanceBetweenPoints(new Point(e.X, e.Y), new Point(150, 150));
                inCenter = mouseDistance < 100;
            }
            if (windowMoving) {
                this.Location = new Point((this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);
            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e) {
            mouseInside = true;
            ShowSongInfo = true;
            timer2.Enabled = false;
        }

        private void Form1_MouseLeave(object sender, EventArgs e) {
            if (menuControl.MenuActive)
                menuControl.ActiveMenu.MouseLeave();
            selecting = false;
            mouseInside = false;
            nextact = 0f;
            timer2.Enabled = true;
            ShowSongInfo = false;
        }

        private void Form1_MouseHover(object sender, EventArgs e) {
            if (windowMoving || !loaded) return;
            selecting = true;
            nextact = 360f / menuControl.ActiveMenus;
            nextanim = 1;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e) {
            if (!selecting && e.Button == MouseButtons.Left && !menuControl.MenuActive) {
                windowMoving = true;
                lastLocation = e.Location;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e) {
            if (windowMoving)
                windowMoving = false;
        }

        private void timer2_Tick(object sender, EventArgs e) {
            if(loaded)
                if(currentSong.HasArt)
                    nextanim = 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            hk_MediaPrev.Unregister();
            hk_MediaPlay.Unregister();
            hk_MediaNext.Unregister();
        }
    }
}
