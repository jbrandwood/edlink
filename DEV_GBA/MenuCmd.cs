using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.DEV_GBA {
    internal class MenuCmd {

        const int BMOD_GBA_REBOOT = 0x4252;//'RB' reboot request
        const int BMOD_GBA_VDUMP = 0x4456;//'VD' vram dump

        public const int RST_MODE_SOFT = 0;
        public const int RST_MODE_HARD = 1;

        DeviceIO dev;
        Link link;

        public MenuCmd(DeviceIO dev) {
            this.dev = dev;
            link = dev.Link;
        }

        internal void ResetToMenu(int mode) {

            if (mode == RST_MODE_SOFT) {
                ResetToMenu_soft();
            } else {
                ResetToMenu_hard();
            }
        }

        public void VramDump(byte[] vram, byte[] palette, byte[] regs) {

            int dump_addr;
            dev.SetBootMode(BMOD_GBA_VDUMP);
            dump_addr = link.rx32();


            dev.MemRD(dump_addr, vram, 0, 96 * 1024);
            dev.MemRD(dump_addr + 96 * 1024, palette, 0, 1024);
            dev.MemRD(dump_addr + 97 * 1024, regs, 0, 1024);
        }

        void ResetToMenu_hard() {

            //reset mcu
            dev.BootSW(true);
            dev.EnterServiceMode();
            dev.ExitServiceMode();

            dev.ResetEfu(15);

            dev.BootSW(true);
            dev.SetBootMode(BMOD_GBA_REBOOT);
            dev.BootSW(false);
        }

        void ResetToMenu_soft() {

            dev.ExitServiceMode();
            dev.SetBootMode(BMOD_GBA_REBOOT);
            dev.BootSW(false);
        }
    }
}
