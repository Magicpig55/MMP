using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MMP.MenuHandlers {
    class VolumeMenuHandler : MenuHandler {

        private float anim = 0;
        private float animNext = 0;

        private float mAnim = 0;
        private float mAnimNext = 0;

        private bool active = false;

        public VolumeMenuHandler(SongHandler songhandler, MenuController menu) : base(songhandler, menu) {
            Label = "Volume"; // Text displayed when selecting stuff
            Enabled = true; // Enables the control all the time, including when no songs are playing
        }

        public override void Draw(Graphics g, Rectangle r) {
            anim = (anim + animNext) / 2;
            mAnim = (mAnim + mAnimNext) / 2;
            g.FillPie(new SolidBrush(Color.FromArgb(64, 255, 255, 255)), r, 270f, mAnim);
            g.FillPie(new SolidBrush(Color.FromArgb(64, 0, 0, 0)), r, 270f, anim);
            if (active)
                mAnimNext = MenuControls.MouseDegrees;
            else {
                if (anim < 1 && mAnim < 1) {
                    Deactivate();
                }
            }
        }

        public override void OnClick() {
            SongControls.Volume = MenuControls.MouseDegrees / 360f;
            animNext = SongControls.Volume * 360f;
        }
        public override bool OnActivated() {
            anim = 0;
            mAnim = 0;
            animNext = SongControls.Volume * 360f;
            mAnimNext = MenuControls.MouseDegrees;
            active = true;
            MenuControls.ShowSongInfo = false;
            return true;
        }
        public override void MouseLeave() {
            mAnimNext = 0;
            animNext = 0;
            active = false;
        }

        public override void OnCreate() { }
        public override void OnSongAdded(Song song) { }
        public override void SongLoaded(Song song) { }
    }
}
