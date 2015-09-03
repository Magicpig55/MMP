using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MMP {
    public class Keyboard {
        [DllImport("user32.dll")]
        static extern ushort GetAsyncKeyState(Keys vKey);

        public static bool IsKeyDown(Keys key) {
            return 0 != (GetAsyncKeyState(key) & 0x8000);
        }
    }
}
