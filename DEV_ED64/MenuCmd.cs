using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edlink.Device;

namespace Edlink.DEV_ED64 {
    internal class MenuCmd {

        internal const int MODE_MENU = 0;
        internal const int MODE_GPAK = 1;
        internal const int MODE_IPL4 = 2;

        DeviceIO dev;
        Link link;

        public MenuCmd(DeviceIO dev) {
            this.dev = dev;
            link = dev.Link;
        }

        internal void Test() {

            int resp;

            dev.FifoWR("*t");

            try {
                resp = link.rx8();
            } catch (Exception) {
                throw new Exception("mcmd: no response from menu");
            }

            if (resp != 'k') {
                throw new Exception("mcmd: unexpected response: " + resp.ToString("X2"));
            }
        }

        internal void ResetToMenu() {

            RunPreloaded("", MenuCmd.MODE_MENU);
        }

        internal void Run(string path) {

            if (Link.IsDevPath(path)) {
                RunSDC(Link.GetDevPath(path));
            } else {
                RunUSB(path);
            }
        }

        void RunUSB(string path) {

            int mode = MenuCmd.MODE_GPAK;
            byte[] rom = getRom(path);
            int entry = GetEntry(mode);

            dev.MemWR(entry, rom, 0, rom.Length);

            SetGpakSize(rom.Length);

            RunPreloaded(Path.GetFileName(path), mode);
        }

        void RunSDC(string path) {

            //run file from sd card
            dev.FifoWR("*f");
            dev.FifoTxString(path);


            int resp = link.waitResp(7000);
            if (resp != 0) {
                throw new Exception("mcmd: cmd error: " + resp.ToString("X2"));
            }
        }

        void RunPreloaded(string fname, int mode) {

            //run preloaded in memory rom
            dev.FifoWR("*r");
            dev.FifoWR(new byte[] { (byte)mode }, 0, 1);
            dev.FifoTxString(fname);

            int resp = link.rx8();
            if (resp != 0) {
                throw new Exception("mcmd: cmd error: " + resp.ToString("X2"));
            }
        }

        void SetGpakSize(int size) {

            dev.FifoWR("*g");
            dev.FifoWR(link.num32(size), 0, 4);
        }

        int GetEntry(int mode) {

            dev.FifoWR("*a");
            dev.FifoWR(new byte[] { (byte)mode }, 0, 1);
            return link.rx32();
        }

        byte[] getRom(string path) {

            byte[] rom = File.ReadAllBytes(path);

            if (rom[1] == 0x80) {
                for (int i = 0; i < rom.Length; i += 2) {
                    byte tmp = rom[i + 0];
                    rom[i + 0] = rom[i + 1];
                    rom[i + 1] = tmp;
                }
            }

            return rom;
        }

        internal void VramDump(byte[] vram) {

            int dump_addr;
            dev.FifoWR("*v");
            dump_addr = link.rx32();
            dev.MemRD(dump_addr, vram, 0, vram.Length);
        }

    }
}
