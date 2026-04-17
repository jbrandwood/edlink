using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edlink.DEV_TED {
    internal class MenuCmd {

        DeviceIO dev;
        Link link;

        public MenuCmd(DeviceIO dev) {
            this.dev = dev;
            link = dev.Link;
        }

        internal void ResetToMenu() {

            int resp;
            int tout = 2000;

            dev.hostReset(DeviceIO.HOST_RST_ON);//DeviceIO.HOST_RST_SOFT
            Thread.Sleep(10);
            dev.ConfigReset();
            dev.hostReset(DeviceIO.HOST_RST_OFF);

            var sw = Stopwatch.StartNew();

            while (link.BytesToRead < 1) {

                if (sw.ElapsedMilliseconds > tout) {
                    throw new Exception("reset timeout");
                }
            }

            resp = link.rx8();

            if (resp != 'r') {
                throw new Exception("unexpected usb status: 0x" + resp.ToString("X2"));
            }
        }


        public void AppInstall(string path) {

            int resp;
            dev.FifoWR("*i");
            dev.FifoTxString(path);
            resp = link.rx8();
            if (resp != 0) {
                throw new Exception("app instalation error: 0x" + resp.ToString("X2"));
            }
        }

        public void AppStart() {

            dev.FifoWR("*s");
        }

        public void VramDump(byte[] vram, byte[] palette) {

            int dump_addr;
            dev.FifoWR("*v");
            dump_addr = link.rx32();

            dev.MemRD(dump_addr, vram, 0, 0x10000);
            dev.MemRD(dump_addr + 0x10000, palette, 0, 1024);
        }
    }
}
