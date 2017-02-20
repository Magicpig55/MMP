using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMP.MenuHandlers {
    class TrackMenuHandler : MenuHandler {

        public TrackMenuHandler(SongHandler songhandler, MenuController menu) : base(songhandler, menu) {
            Label = "Track";
        }

        public override void Draw(Graphics g, Rectangle r) {
        }
        public override void MouseLeave() {
            throw new NotImplementedException();
        }
        public override bool OnActivated() {
            throw new NotImplementedException();
        }
        public override void OnClick() {
            throw new NotImplementedException();
        }
        public override void OnCreate() {}
        public override void OnSongAdded(Song song) {}
        public override void SongLoaded(Song song) {}
    }
}
