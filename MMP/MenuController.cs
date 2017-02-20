using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MMP {
    class MenuController {

        private Form1 form;

        public Color CurrentColor {
            get {
                return form.CurrentColor;
            }
            set {
                form.CurrentColor = value;
            }
        }
        public float MouseDegrees {
            get {
                return form.nextsel;
            }
        }
        public float MouseDistance {
            get {
                return form.mouseDistance;
            }
        }
        public bool ShowControls {
            get {
                return form.selecting;
            }
            set {
                form.selecting = false;
                form.nextact = 0;
            }
        }

        public bool ShowSongInfo {
            get {
                return form.ShowSongInfo;
            }
            set {
                form.ShowSongInfo = value;
            }
        }

        private MenuHandler activeMenu;
        public MenuHandler ActiveMenu {
            get {
                return activeMenu;
            }
            set {
                activeMenu = value;
                MenuActive = value != null;
            }
        }
        public bool MenuActive = false;

        public void SetActiveMenu(int i) {
            ActiveMenu = this[i];
        }

        private List<MenuHandler> handlerList = new List<MenuHandler>();
        public MenuHandler this[int i] {
            get {
                return handlerList.Where(e => e.Enabled).ElementAt(i);
            }
            set {
                handlerList[i] = value;
            }
        }

        public void Add(MenuHandler menuHandler) {
            handlerList.Add(menuHandler);
        }

        public void DoCreate() {
            foreach(MenuHandler mh in handlerList) {
                mh.OnCreate();
            }
        }
        public void DoSongLoaded(Song song) {
            foreach (MenuHandler mh in handlerList) {
                mh.SongLoaded(song);
            }
        }

        public int Size {
            get {
                return handlerList.Count;
            }
        }
        public int ActiveMenus {
            get {
                return handlerList.Where(item => item.Enabled).Count();
            }
        }

        public MenuController(Form1 form) {
            this.form = form;
        }

        public void Close() {
            form.Close();
        }
    }
}
