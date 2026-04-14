using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edlink.EDMEGA {
    internal class MenuCmd {

        DeviceIO dev;
        Link link;

        public MenuCmd(DeviceIO dev) {
            this.dev = dev;
            link = dev.Link;
        }

        internal void Test() {

            int resp;

            dev.FifoWR("*t");
            resp = link.rx8();

            if (resp != 'k') {
                throw new Exception("unexpected response: " + resp);
            }
        }

        internal void ResetToMenu(byte rst_type) {

            int resp;
            int tout = rst_type == DeviceIO.HOST_RST_HARD ? 6000 : 2000;

            dev.hostReset(rst_type);//DeviceIO.HOST_RST_SOFT
            Thread.Sleep(10);
            dev.configReset();
            dev.hostReset(DeviceIO.HOST_RST_OFF);

            var sw = Stopwatch.StartNew();

            while (link.BytesToRead < 1) {

                if (sw.ElapsedMilliseconds > tout) {
                    throw new Exception("reset timeout");
                }
            }

            resp = link.rx8();

            if (resp != 'r') {
                throw new Exception("unexpected usb status: " + resp.ToString("X2"));
            }
        }

        internal void AppInstall(string path) {

            int resp;

            dev.FifoWR("*i");
            dev.FifoTxString(path);
            resp = link.rx8();
            if (resp != 0) {
                throw new Exception("app instalation error: " + resp.ToString("X2"));
            }
        }

        internal void AppStart() {
            dev.FifoWR("*s");
        }

        internal void VramDump(byte[] vram, byte[] palette) {

            int dump_addr;
            dev.FifoWR("*v");
            dump_addr = link.rx32();

            dev.MemRD(dump_addr, vram, 0, 0x10000);
            dev.MemRD(dump_addr + 0x10000, palette, 0, 128);
        }

    }
}
