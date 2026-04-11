using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Edlink.Device;

namespace Edlink {
    internal class CliHandlerCmd {

        protected DeviceCmd dcmd;


        protected void SetMode(CmdLine cmd) {

            string mode = cmd.getStr(Cli.ArgMode);
            CmdStart(cmd, "set mode: " + mode + "...");
            dcmd.SetMode(mode);
            CmdEnd("ok");
        }

        protected void LinkConfig(CmdLine cmd, Link link) {//move to Cmd base


            if (cmd.HasArg(Cli.ArgPort)) {
                link.PortName = cmd.getStr(Cli.ArgPort);
            }

            if (cmd.HasArg(Cli.ArgDevId)) {
                link.DeviceID = (byte)cmd.getInt(Cli.ArgDevId);
            }

            if (cmd.HasArg(Cli.ArgProtId)) {
                link.ProtocolID = (byte)cmd.getInt(Cli.ArgProtId);
            }
        }



        protected void MemRD(CmdLine cmd) {

            CmdStart(cmd, "memory read...");

            bool print = cmd.HasArg(Cli.ArgOut);
            int addr = cmd.getInt(Cli.ArgAddr);
            int len = cmd.getInt(Cli.ArgLen);
            string path;

            if (cmd.HasArg(Cli.ArgFile) || !print) {
                path = cmd.getStr(Cli.ArgFile);
            } else {
                path = null;
            }

            byte[] buff = new byte[len];
            dcmd.MemRD(addr, buff, 0, len);

            if (path != null) {
                File.WriteAllBytes(path, buff);
            }

            CmdEnd("ok");

            if (print) {
                for (int i = 0; i < buff.Length; i += 16) {
                    Console.WriteLine(BitConverter.ToString(buff, i, Math.Min(len, 16)));
                }
            }
        }

        protected void MemWR(CmdLine cmd) {

            CmdStart(cmd, "memory write...");

            string path = cmd.getStr(Cli.ArgFile);
            int addr = cmd.getInt(Cli.ArgAddr);
            int len;
            int offset = 0;

            byte[] buff = File.ReadAllBytes(path);

            if (cmd.HasArg(Cli.ArgLen)) {
                len = cmd.getInt(Cli.ArgLen);
            } else {
                len = buff.Length;
            }

            if (cmd.HasArg(Cli.ArgOffset)) {
                offset = cmd.getInt(Cli.ArgOffset);
            }

            dcmd.MemWR(addr, buff, offset, len);


            CmdEnd("ok");
        }

        protected void FlaRD(CmdLine cmd) {

            CmdStart(cmd, "flash read read...");

            string path = cmd.getStr(Cli.ArgFile);
            int addr = cmd.getInt(Cli.ArgAddr);
            int len = cmd.getInt(Cli.ArgLen);

            byte[] buff = new byte[len];
            dcmd.FlaRD(addr, buff, 0, len);

            File.WriteAllBytes(path, buff);

            CmdEnd("ok");
        }

        protected void FlaWR(CmdLine cmd) {

            CmdStart(cmd, "flash write.");

            string path = cmd.getStr(Cli.ArgFile);
            int addr = cmd.getInt(Cli.ArgAddr);
            int len;
            int offset = 0;

            byte[] buff = File.ReadAllBytes(path);

            if (cmd.HasArg(Cli.ArgLen)) {
                len = cmd.getInt(Cli.ArgLen);
            } else {
                len = buff.Length;
            }

            if (cmd.HasArg(Cli.ArgOffset)) {
                offset = cmd.getInt(Cli.ArgOffset);
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

            string rom_path = cmd.getStr(Cli.ArgFile);
            string fpga_path = null;

            if (cmd.HasArg(Cli.ArgFpga)) {
                fpga_path = cmd.getStr(fpga_path);
            }

            dcmd.Run(rom_path, fpga_path);

            CmdEnd("ok");
        }

        protected void FpgaInit(CmdLine cmd) {

            CmdStart(cmd, "fpga init...");
            string path = cmd.getStr(Cli.ArgFile);
            dcmd.FpgaInit(path);
            CmdEnd("ok");
        }

        protected void Copy(CmdLine cmd) {

            string src = cmd.getStr(Cli.ArgSrc);
            string dst = cmd.getStr(Cli.ArgDst);

            if (!Link.IsDevPath(src) && File.GetAttributes(src).HasFlag(FileAttributes.Directory)) {
                CopyDir(cmd, src, dst);
            } else {
                CopyFile(cmd, src, dst);
            }
        }

        protected void RtcSet(CmdLine cmd) {
            CmdStart(cmd, "RTC set...");
            dcmd.RtcSet();
            CmdEnd("ok");
        }

        protected void McuUpd(CmdLine cmd) {

            bool arg_ok = false;

            string[] mode = {
                Cli.ArgBoot,
                Cli.ArgApp
            };

            for (int i = 0; i < mode.Length; i++) {

                if (!cmd.HasArg(mode[i])) {
                    continue;
                }
                arg_ok = true;

                CmdStart(cmd, "MCU upd " + mode[i].Replace(Cli.ArgPrefix, "") + "...");
                string path = cmd.getStr(mode[i]);
                dcmd.McuUpd(path, mode[i]);
                CmdEnd("ok");
            }


            if (!arg_ok) {
                //force exception message
                cmd.getStr(Cli.ArgBoot + " or " + Cli.ArgApp);
            }

        }
        //************************************************************************************************ 
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
