using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMP.MenuHandlers {
    class CloseMenuHandler : MenuHandler {

        public CloseMenuHandler(SongHandler songhandler, MenuController menu) : base(songhandler, menu) {
            Label = "Close";
            Enabled = true;
        }

        public override void Draw(Graphics g, Rectangle r) { }
        public override void MouseLeave() { }
        public override bool OnActivated() {
            MenuControls.Close();
            return false;
        }
        public override void OnClick() { }
        public override void OnCreate() { }
        public override void OnSongAdded(Song song) { }
        public override void SongLoaded(Song song) { }
    }
}
