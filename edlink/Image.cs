using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Edlink {

    internal class Image {


        Bitmap img;

        public Image(int w, int h) {

            img = new Bitmap(w, h);
        }

        Image(Bitmap img) {

            this.img = img;
        }

        public void SetPixel(int x, int y, int rgb) {

            img.SetPixel(x, y, Color.FromArgb(rgb));
        }

        public void SetPixel(int x, int y, int r, int g, int b) {

            img.SetPixel(x, y, Color.FromArgb(r, g, b));
        }

        public void Save(string path) {

            img.Save(path);
        }

        public int GetPixel(int x, int y) {

            return img.GetPixel(x, y).ToArgb();
        }

        public Image Scalse(int w, int h) {

            Bitmap img_scal = new Bitmap(w, h);

            using (Graphics g = Graphics.FromImage(img_scal)) {
                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(img, 0, 0, w, h);
            }

            return new Image(img_scal);
        }

    }
}
