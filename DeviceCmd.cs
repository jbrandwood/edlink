using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace edlink {
    internal class DeviceCmd {

        const string arg_file = "--file";
        const string arg_addr = "--addr";
        const string arg_len = "--len";
        const string arg_offset = "--offset";
        const string arg_fpga = "--fpga";

        IDeviceIO dev;

        public DeviceCmd(IDeviceIO dev) {

            this.dev = dev;
        }

        public void MemPrint(Cmd cmd) {

            int addr = cmd.getInt(arg_addr);
            int len = 256;

            if (cmd.HasArg(arg_len)) {
                len = cmd.getInt(arg_len);
            }

            byte[] buff = new byte[len];
            dev.MemRD(addr, buff, 0, len);

            for (int i = 0; i < buff.Length; i += 16) {
                Console.WriteLine(BitConverter.ToString(buff, i, Math.Min(len, 16)));
            }
        }

        public void MemRD(Cmd cmd) {

            string path = cmd.getStr(arg_file);
            int addr = cmd.getInt(arg_addr);
            int len = cmd.getInt(arg_len);

            byte[] buff = new byte[len];
            dev.MemRD(addr, buff, 0, len);

            File.WriteAllBytes(path, buff);
        }

        public void MemWR(Cmd cmd) {

            string path = cmd.getStr(arg_file);
            int addr = cmd.getInt(arg_addr);
            int len;
            int offset = 0;

            byte[] buff = File.ReadAllBytes(path);

            if (cmd.HasArg(arg_len)) {
                len = cmd.getInt(arg_len);
            } else {
                len = buff.Length;
            }

            if (cmd.HasArg(arg_offset)) {
                offset = cmd.getInt(arg_offset);
            }

            dev.MemWR(addr, buff, offset, len);

            File.WriteAllBytes(path, buff);
        }

        public void Reset(Cmd cmd) {

            dev.Reset();
        }

        public void Run(Cmd cmd) {

            string rom_path = cmd.getStr(arg_file);
            string fpga_path = null;

            if (cmd.HasArg(arg_fpga)) {
                fpga_path = cmd.getStr(fpga_path);
            }

            dev.Run(rom_path, fpga_path);
        }

        public void FpgaInit(Cmd cmd) {

            string path = cmd.getStr(arg_file);
            dev.FpgaInit(path);
        }
    }
}
