using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MMP.MenuHandlers {
    class PreviousSongMenuHandler : MenuHandler {

        public PreviousSongMenuHandler(SongHandler songhandler, MenuController menu) : base(songhandler, menu) {
            Label = "Previous";
        }

        public override void Draw(Graphics g, Rectangle r) { }
        public override void MouseLeave() { }
        public override bool OnActivated() {
            SongControls.LoadPrevious();
            return false;
        }
        public override void OnClick() { }
        public override void OnCreate() { }
        public override void OnSongAdded(Song song) {

        }
        public override void SongLoaded(Song song) {
            if (SongControls.UsingPlaylist)
                Enabled = true;
        } 
    }
}
