using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace MMP {
    struct Song {
        public string url;
        public string albumArtists;
        public string album;
        public string title;
        public Image art;
        public bool HasArt;

        public Song(string url, bool loadimage = false) {
            using (TagLib.File file = TagLib.File.Create(url)) {
                TagLib.Tag tags = file.Tag;
                this.url = url;
                albumArtists = (tags.JoinedPerformers != "" ? tags.JoinedPerformers : tags.JoinedAlbumArtists);
                album = tags.Album;
                title = tags.Title == null ? Path.GetFileNameWithoutExtension(url) : tags.Title;
                art = loadimage && tags.Pictures.Length > 0 ? Image.FromStream(new MemoryStream(tags.Pictures[0].Data.Data)) : new Bitmap(1, 1);
                HasArt = tags.Pictures.Length > 0;
                tags = null;
            }
        }
    }
}
