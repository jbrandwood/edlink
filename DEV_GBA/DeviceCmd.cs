using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.DEV_GBA {
    internal class DeviceCmd : Device.DeviceCmd {

        new DeviceIO dev;
        MenuCmd mcmd;

        public DeviceCmd(Link link) {

            base.dev = dev = new DeviceIO(link);
            mcmd = new MenuCmd(dev);

        }


        public override string DeviceName {

            get {

                switch (dev.Link.DeviceID) {

                    case DeviceIO.DEV_ID_GBA_PRO:
                        return "EverDrive-GBA PRO";
                    default:
                        return "Uknown EverDrive-GBA";
                }
            }
        }

        public override void Reset(string mode) {

            mode = mode.ToLower().Trim();

            if (mode.Equals("hard")) {
                mcmd.ResetToMenu(MenuCmd.RST_MODE_HARD);
            } else {
                mcmd.ResetToMenu(MenuCmd.RST_MODE_SOFT);
            }
        }

        public override void Screen(string path) {

            byte[] vram = new byte[96 * 1024];
            byte[] palette = new byte[1024];
            byte[] regs = new byte[1024];

            mcmd.VramDump(vram, palette, regs);
            MenuImage.makeImage(path, vram, palette, regs);
        }

        public override void Diag() {
            Diagnostics diag = new Diagnostics(dev);
            diag.Start();
        }
    }
}
