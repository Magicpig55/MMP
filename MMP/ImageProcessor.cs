using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace MMP {
    class ImageProcessor {
        public static Color AverageColor(Bitmap bitmap) {
            FastBitmap fbm = new FastBitmap(bitmap);
            int width = bitmap.Width, height = bitmap.Height;
            int r = 0, g = 0, b = 0, total = width * height;
            fbm.LockImage();
            Color c = fbm.GetPixel(0, 0);
            r += c.R;
            g += c.G;
            b += c.B;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    c = fbm.GetPixel(x, y);
                    r += c.R;
                    g += c.G;
                    b += c.B;
                }
            }
            fbm.UnlockImage();
            r /= total;
            r = (r / 16) * 16;
            g /= total;
            g = (g / 16) * 16;
            b /= total;
            b = (b / 16) * 16;
            return Color.FromArgb(r, g, b);
        }
    }
}
