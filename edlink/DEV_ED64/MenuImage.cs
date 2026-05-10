using System;


namespace Edlink.DEV_ED64 {
    internal class MenuImage {

        const int screen_w = 640;
        const int screen_h = 240;

        public static void MakeImage(string path, byte[] vram) {

            Image img = new Image(screen_w, screen_h);

            for (int i = 0; i < screen_w * screen_h; i++) {

                int rgb16 = vram[i * 2 + 1] | (vram[i * 2 + 0] << 8);
                int b = ((rgb16 >> 1) & 0x1F) << 3;
                int g = ((rgb16 >> 6) & 0x1F) << 3;
                int r = ((rgb16 >> 11) & 0x1F) << 3;



                if (r == 16 && g == 16 && b == 16) {
                    r = 12;
                    g = 12;
                    b = 12;
                }


                int x = i % screen_w;
                int y = i / screen_w;

                img.SetPixel(x, y, r, g, b);

            }

          
            img = img.Scalse(screen_w, screen_h * 2);

            img.Save(path);
        }
    }
}
