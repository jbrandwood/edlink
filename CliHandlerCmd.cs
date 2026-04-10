using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace edlink {
    internal class CliHandlerCmd {

        public const string ARG_PREFIX = "--";
        public const string ARG_NEWCMD = "--";
        public const string ARG_FILE = "--file";
        const string ARG_ADDR = "--addr";
        const string ARG_LEN = "--len";
        const string ARG_OFFSET = "--offset";
        const string ARG_FPGA = "--fpga";
        const string ARG_SRC = "--src";
        const string ARG_DST = "--dst";
        const string ARG_MODE = "--mode";
        

        protected IDeviceCmd dcmd;


        protected void SetMode(CmdLine cmd) {

            string mode = cmd.getStr(ARG_MODE);
            CmdStart(cmd, "set mode: " + mode + "...");
            dcmd.SetMode(mode);
            CmdEnd("ok");
        }

        protected void MemPrint(CmdLine cmd) {

            int addr = cmd.getInt(ARG_ADDR);
            int len = 256;

            if (cmd.HasArg(ARG_LEN)) {
                len = cmd.getInt(ARG_LEN);
            }

            byte[] buff = new byte[len];
            dcmd.MemRD(addr, buff, 0, len);

            for (int i = 0; i < buff.Length; i += 16) {
                Console.WriteLine(BitConverter.ToString(buff, i, Math.Min(len, 16)));
            }
        }

        protected void MemRD(CmdLine cmd) {

            CmdStart(cmd, "memory read...");

            string path = cmd.getStr(ARG_FILE);
            int addr = cmd.getInt(ARG_ADDR);
            int len = cmd.getInt(ARG_LEN);

            byte[] buff = new byte[len];
            dcmd.MemRD(addr, buff, 0, len);

            File.WriteAllBytes(path, buff);

            CmdEnd("ok");
        }

        protected void MemWR(CmdLine cmd) {

            CmdStart(cmd, "memory write...");

            string path = cmd.getStr(ARG_FILE);
            int addr = cmd.getInt(ARG_ADDR);
            int len;
            int offset = 0;

            byte[] buff = File.ReadAllBytes(path);

            if (cmd.HasArg(ARG_LEN)) {
                len = cmd.getInt(ARG_LEN);
            } else {
                len = buff.Length;
            }

            if (cmd.HasArg(ARG_OFFSET)) {
                offset = cmd.getInt(ARG_OFFSET);
            }

            dcmd.MemWR(addr, buff, offset, len);


            CmdEnd("ok");
        }

        protected void FlaRD(CmdLine cmd) {

            CmdStart(cmd, "flash read read...");

            string path = cmd.getStr(ARG_FILE);
            int addr = cmd.getInt(ARG_ADDR);
            int len = cmd.getInt(ARG_LEN);

            byte[] buff = new byte[len];
            dcmd.FlaRD(addr, buff, 0, len);

            File.WriteAllBytes(path, buff);

            CmdEnd("ok");
        }

        protected void FlaWR(CmdLine cmd) {

            CmdStart(cmd, "flash write.");

            string path = cmd.getStr(ARG_FILE);
            int addr = cmd.getInt(ARG_ADDR);
            int len;
            int offset = 0;

            byte[] buff = File.ReadAllBytes(path);

            if (cmd.HasArg(ARG_LEN)) {
                len = cmd.getInt(ARG_LEN);
            } else {
                len = buff.Length;
            }

            if (cmd.HasArg(ARG_OFFSET)) {
                offset = cmd.getInt(ARG_OFFSET);
            }

            while (len > 0) {

                int block = Math.Min(len, 0x10000);
                block = Math.Min(block, 0x10000 - addr % 0x10000);

                dcmd.FlaWR(addr, buff, offset, block);
                cmdProgress();
                len -= block;
                addr += block;
                offset += block;
            }

            CmdEnd("ok");
        }

        protected void Reset(CmdLine cmd) {
            CmdStart(cmd, "device reset...");
            dcmd.Reset();
            CmdEnd("ok");
        }

        protected void Run(CmdLine cmd) {

            CmdStart(cmd, "run application...");

            string rom_path = cmd.getStr(ARG_FILE);
            string fpga_path = null;

            if (cmd.HasArg(ARG_FPGA)) {
                fpga_path = cmd.getStr(fpga_path);
            }

            dcmd.Run(rom_path, fpga_path);

            CmdEnd("ok");
        }

        protected void FpgaInit(CmdLine cmd) {

            CmdStart(cmd, "fpga init...");
            string path = cmd.getStr(ARG_FILE);
            dcmd.FpgaInit(path);
            CmdEnd("ok");
        }

        protected void Copy(CmdLine cmd) {

            string src = cmd.getStr(ARG_SRC);
            string dst = cmd.getStr(ARG_DST);

            if (!Link.IsDevPath(src) && File.GetAttributes(src).HasFlag(FileAttributes.Directory)) {
                CopyDir(cmd, src, dst);
            } else {
                CopyFile(cmd, src, dst);
            }
        }

         void CopyFile(CmdLine cmd, string src, string dst) {

            CmdStart(cmd, src + " --> " + dst);
            dcmd.CopyFile(src, dst);
            CmdEnd("");
        }

        void CopyDir(CmdLine cmd, string src, string dst) {

            if (!src.EndsWith("/")) src += "/";
            if (!dst.EndsWith("/")) dst += "/";

            string[] dirs = Directory.GetDirectories(src);

            for (int i = 0; i < dirs.Length; i++) {
                CopyDir(cmd, dirs[i], dst + Path.GetFileName(dirs[i]));
            }

            string[] files = Directory.GetFiles(src);

            for (int i = 0; i < files.Length; i++) {
                CopyFile(cmd, files[i], dst + Path.GetFileName(files[i]));
            }
        }

        void CmdStart(CmdLine cmd, string msg) {
            Console.Write("[" + cmd.Name + "] " + msg);
        }
        void CmdEnd(string msg) {
            Console.WriteLine(msg);
        }

        void cmdProgress() {
            Console.Write(".");
        }
    }
}
