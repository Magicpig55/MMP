using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using TagLib;

namespace MMP {
    class Playlist {

        public List<Song> Songs;
        public string PlaylistURL = "";

        public Playlist() {
            Songs = new List<Song>();
        }
        public Playlist(List<String> ListToUse) {
            foreach (String str in ListToUse) {
                AddSong(str);
            }
        }

        public bool Load(String FileLocation) {
            using (StreamReader read = new StreamReader(FileLocation)) {
                if (read == null)
                    return false;
                PlaylistURL = FileLocation;
                while (!read.EndOfStream) {
                    AddSong(read.ReadLine());
                }
                return true;
            }
        }

        public bool Save(bool saveNew = false) {
            if (PlaylistURL == "" || saveNew) {
                using (SaveFileDialog sfd = new SaveFileDialog()) {
                    sfd.Filter = "Playlist File|*.mpl";
                    sfd.InitialDirectory = "c://";
                    sfd.AddExtension = true;
                    if (sfd.ShowDialog() == DialogResult.OK) {
                        return Save(sfd.FileName);
                    }
                    return false;
                }
            } else {
                return Save(PlaylistURL);
            }
        }

        public void Shuffle() {
            Random r = new Random();
            // Shuffling twice, just to make sure.
            Songs.OrderBy(item => r.Next());
            Songs.Shuffle();
        }

        public bool Save(string FileLocation) {
            using (StreamWriter write = new StreamWriter(FileLocation)) {
                if (write == null)
                    return false;
                foreach (Song song in Songs) {
                    write.WriteLine(song.url);
                }
                return true;
            }
        }

        public void AddSong(String url) {
            Song song = new Song(url);
            Songs.Add(song);
        }
    }
}
