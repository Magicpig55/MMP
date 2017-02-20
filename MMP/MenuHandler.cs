using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MMP {
    abstract class MenuHandler {

        abstract public void Draw(Graphics g, Rectangle r);
        abstract public void OnClick();
        abstract public bool OnActivated();
        abstract public void OnSongAdded(Song song);
        abstract public void OnCreate();
        abstract public void MouseLeave();
        abstract public void SongLoaded(Song song);

        public readonly SongHandler SongControls;
        public readonly MenuController MenuControls;
        public bool Enabled = false;
        public string Label = "";

        public MenuHandler(SongHandler songhandler, MenuController menu) {
            SongControls = songhandler;
            MenuControls = menu;
        }

        public void Deactivate() {
            MenuControls.ActiveMenu = null;
            MenuControls.MenuActive = false;
            MenuControls.ShowControls = false;
        }
    }
}
