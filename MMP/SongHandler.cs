using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MMP {
    // Provides an interface for menu handlers to access controls
    class SongHandler {

        private Song currentSong;
        public Song CurrentSong {
            get {
                return currentSong;
            }
            set {
                currentSong = value;
                form.LoadSong(value.url);
            }
        }

        private Playlist currentPlaylist;
        public Playlist CurrentPlaylist {
            get {
                return currentPlaylist;
            }
            set {
                currentPlaylist = value;
                form.LoadPlaylist(value.PlaylistURL);
            }
        }

        public bool Paused {
            get {
                return form.Paused;
            }
            set {
                form.Paused = value;
            }
        }

        private Form1 form;

        public SongHandler(Form1 form) {
            this.form = form;
        }

        public float Volume {
            get {
                return form.Volume;
            }
            set {
                form.Volume = value;
            }
        }
        public long Position {
            get {
                return form.Position;
            }
            set {
                form.Position = value;
            }
        }

        public long SongLength {
            get {
                return form.SongLength;
            }
        }

        public void SetSongPositionPercentage(float percent) {
            Position = (long)(SongLength * percent);
        }

        public bool UsingPlaylist {
            get {
                return form.playlistActive;
            }
        }

        public void LoadNext() {
            if(UsingPlaylist) 
                form.LoadNext();
        }
        public void LoadPrevious() {
            if (UsingPlaylist)
                form.LoadPrevious();
        }

    }
}
