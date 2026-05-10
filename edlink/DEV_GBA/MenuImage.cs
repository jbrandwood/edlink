using System;


namespace Edlink.DEV_GBA {

    internal class LcdConfig {

        public int tmap_src = 0;
        public int chr_src = 0;
        public bool enabled = false;
        public int prior = 0;
        public int blend_cof = 0;
    }

    internal class MenuImage {

        const int plan_w = 32;//512/8
        const int screen_w = 30;//320/8
        const int screen_h = 20;//224/8

        public static void MakeImage(string path, byte[] vram, byte[] pal8, byte[] regs) {


            Image pic = new Image(screen_w * 8, screen_h * 8);
            LcdConfig[] cfg = null;

            UInt16[] pal16 = Data8To16(pal8);
            int[] pal32 = GetPal32(pal16);


            cfg = GetConfig(Data8To16(regs));


            FillBG(pic, pal32[0]);

            for (int i = 0; i < cfg.Length; i++) {
                DrawPlan(pic, vram, pal32, cfg[i]);
            }

            pic.Save(path);
        }

        static LcdConfig[] GetConfig(UInt16[] regs) {

            LcdConfig[] cfg = new LcdConfig[4];

            for (int i = 0; i < cfg.Length; i++) {

                int bg_cnt = regs[4 + i];
                bool blend_on = (((regs[0x50 / 2] & 15) >> i) & 1) != 0;

                cfg[i] = new LcdConfig();
                cfg[i].enabled = regs[0] >> (8 + i) != 0 ? true : false;
                cfg[i].prior = bg_cnt & 3;
                cfg[i].chr_src = ((bg_cnt >> 2) & 3) * 16384;
                cfg[i].tmap_src = ((bg_cnt >> 8) & 31) * 2048;

                cfg[i].blend_cof = blend_on ? (regs[0x52 / 2] >> 8) & 31 : 0;
            }



            for (int i = 0; i < cfg.Length - 1;) {

                if (cfg[i].prior >= cfg[i + 1].prior) {
                    i++;
                } else {
                    LcdConfig tmp = cfg[i];
                    cfg[i] = cfg[i + 1];
                    cfg[i + 1] = tmp;
                    i = 0;
                }
            }

            for (int i = 0; i < cfg.Length; i++) {
                Console.WriteLine(cfg[i].chr_src.ToString("X4") + ", " + cfg[i].enabled + ", " + cfg[i].blend_cof);
            }

            return cfg;
        }

        static void FillBG(Image pic, int color) {

            for (int i = 0; i < screen_w * screen_h * 8 * 8; i++) {

                int x = i % (screen_w * 8);
                int y = i / (screen_w * 8);

                pic.SetPixel(x, y, color);
            }
        }

        static void DrawPlan(Image pic, byte[] vram, int[] pal32, LcdConfig cfg) {


            if (!cfg.enabled) {
                return;
            }

            UInt16[] tilemap = GetTilemap(vram, cfg.tmap_src);

            for (int i = 0; i < screen_w * screen_h * 8 * 8; i++) {

                int x = i % (screen_w * 8);
                int y = i / (screen_w * 8);
                int tile_ptr = x / 8 + y / 8 * screen_w;
                int tile_pal = tilemap[tile_ptr] >> 12;
                int tile_idx = (tilemap[tile_ptr] & 0x1FF) + (cfg.chr_src / 32);
                int tile_pixel = GetPixel(vram, tile_idx, x, y);

                if (tile_pixel == 0) continue;

                int rgb = pal32[tile_pal * 16 + tile_pixel];

                int r = (rgb >> 16) & 0xff;
                int g = (rgb >> 8) & 0xff;
                int b = (rgb >> 0) & 0xff;

                if (cfg.blend_cof != 0) {

                    int old_pixel = pic.GetPixel(x, y);

                    int r_old = (old_pixel >> 16) & 0xff;
                    int g_old = (old_pixel >> 8) & 0xff;
                    int b_old = (old_pixel >> 0) & 0xff;

                    int eva = 0;
                    int evb = cfg.blend_cof;

                    r = Math.Min(0xF8, (r * eva + r_old * evb) / 16);
                    g = Math.Min(0xF8, (g * eva + g_old * evb) / 16);
                    b = Math.Min(0xF8, (b * eva + b_old * evb) / 16);
                    /*
                    int blend_cof = cfg.blend_cof - 1;
                    r = Math.Min(0xF8, r_old * blend_cof / 16);
                    g = Math.Min(0xF8, g_old * blend_cof / 16);
                    b = Math.Min(0xF8, b_old * blend_cof / 16);*/

                }

                pic.SetPixel(x, y, r, g, b);
            }
        }

        static int GetPixel(byte[] vram, int tile_idx, int x, int y) {

            int pixel = 0;
            x %= 8;
            y %= 8;

            int ptr = tile_idx * 32;
            ptr += y * 4;
            ptr += x / 2;

            pixel = x % 2 == 0 ? vram[ptr] & 15 : vram[ptr] >> 4;

            return pixel;
        }

        static UInt16[] GetTilemap(byte[] vram, int offset) {

            UInt16[] map = new UInt16[screen_w * screen_h];

            for (int y = 0; y < screen_h; y++) {

                for (int x = 0; x < screen_w; x++) {

                    int tile_idx = (x + y * plan_w) * 2 + offset;

                    map[x + y * screen_w] = (UInt16)((vram[tile_idx + 0] << 0) | (vram[tile_idx + 1] << 8));
                }
            }

            return map;
        }

        static UInt16[] Data8To16(byte[] data8) {

            UInt16[] data16 = new UInt16[data8.Length / 2];

            for (int i = 0; i < data16.Length; i++) {

                data16[i] = (UInt16)(data8[i * 2 + 0] | (data8[i * 2 + 1] << 8));
            }

            return data16;
        }

        static int[] GetPal32(UInt16[] pal16) {

            int[] pal32 = new int[pal16.Length];

            for (int i = 0; i < pal32.Length; i++) {

                int r = (pal16[i] >> 0) & 0x1F;
                int g = (pal16[i] >> 5) & 0x1F;
                int b = (pal16[i] >> 10) & 0x1F;

                r <<= 3;
                g <<= 3;
                b <<= 3;

                int alpha = 0xff0000;
                alpha <<= 8;


                pal32[i] = alpha | (r << 16) | (g << 8) | (b << 0);

            }



            return pal32;
        }

    }
}
