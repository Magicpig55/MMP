using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMP.MenuHandlers {
    class PositionMenuHandler : MenuHandler {

        private float anim = 0;
        private float animNext = 0;

        private bool active = false;

        public PositionMenuHandler(SongHandler songhandler, MenuController menu) : base(songhandler, menu) {
            Label = "Position";
        }

        public override void Draw(Graphics g, Rectangle r) {
            anim = (anim + animNext) / 2;
            g.FillPie(new SolidBrush(Color.FromArgb(64, 255, 255, 255)), r, 270f, anim);
            if (active)
                animNext = MenuControls.MouseDegrees;
            else if (anim < 1)
                Deactivate();
        }

        public override void SongLoaded(Song song) {
            Enabled = true;
        }

        public override void MouseLeave() {
            animNext = 0;
            active = false;
        }

        public override bool OnActivated() {
            active = true;
            anim = 0;
            animNext = 0;
            MenuControls.ShowSongInfo = false;
            return true;
        }

        public override void OnClick() {
            SongControls.SetSongPositionPercentage(MenuControls.MouseDegrees / 360f);
        }

        public override void OnCreate() { }
        public override void OnSongAdded(Song song) { }
    }
}
