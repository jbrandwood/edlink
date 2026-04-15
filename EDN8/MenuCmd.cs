using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.EDN8 {
    internal class MenuCmd {

        const char cmd_test = 't';
        const char cmd_reboot = 'r';
        const char cmd_halt = 'h';
        const char cmd_sel_game = 'n';
        const char cmd_run_game = 's';
        const char cmd_vram_dump = 'v';

        DeviceIO dev;
        Link link;

        public MenuCmd(DeviceIO dev) {
            this.dev = dev;
            link = dev.Link;
        }

        internal void Test() {

            int resp;

            Cmd(cmd_test);

            try {
                resp = link.rx8();
            } catch (Exception) {
                throw new Exception("mcmd: no response from menu");
            }

            if (resp != 'k') {
                throw new Exception("mcmd: unexpected response: " + resp.ToString("X2"));
            }
        }

        public int AppInstall(string path) {

            int resp;

            Cmd(cmd_sel_game);
            dev.FifoTxString(path);

            resp = link.rx8();//game select status

            if (resp != 0) {
                throw new Exception("app install error 0x" + resp.ToString("X2"));
            }

            int map_idx = link.rx16();
            return map_idx;
        }

        public void AppStart() {
            Cmd(cmd_run_game);
        }

        public void Reset() {

            Cmd(cmd_reboot);
            link.rx8();//wait till it ready ready 
        }

        public void VramDump(byte[] vram, byte[] palette) {

            Cmd(cmd_vram_dump);
            link.rxData(vram, 0, 2048);
            link.rxData(palette, 0, 16);
        }

        void Cmd(char cmd) {

            byte[] buff = new byte[2];
            buff[0] = (byte)'*';
            buff[1] = (byte)cmd;
            dev.FifoWR(buff, 0, buff.Length);
        }

    }
}
