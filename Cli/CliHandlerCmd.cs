using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Edlink {
    internal class CliHandlerCmd {

        protected DeviceCmd dcmd;
        const ConsoleColor inf_color = ConsoleColor.Green;

        protected void McuMode(CmdLine cmd) {

            string mode = cmd.getStr(Cli.ArgMode);
            CmdStart(cmd, "set mode: " + mode + "...");
            dcmd.McuMode(mode);
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

            bool print = cmd.HasArg(Cli.ArgPrint);
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

                for (int i = 0; i < buff.Length;) {

                    int block = Math.Min(len, 16);

                    string msg_hex = BitConverter.ToString(buff, i, block);

                    string msg_txt = Encoding.UTF8.GetString(buff, i, block);                    
                    msg_txt = Regex.Replace(msg_txt, @"\p{Cc}", "�");

                    Tools.PrintLine(msg_hex + "  " + msg_txt, inf_color);

                    i += block;
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

            string mode = "";

            if (cmd.HasArg(Cli.ArgMode)) {
                mode = cmd.getStr(Cli.ArgMode);
            }

            dcmd.Reset(mode);
            CmdEnd("ok");
        }

        protected void Run(CmdLine cmd) {

            CmdStart(cmd, "run application...");

            string rom_path = cmd.getStr(Cli.ArgFile);
            string fpga_path = null;

            if (cmd.HasArg(Cli.ArgFpga)) {
                fpga_path = cmd.getStr(Cli.ArgFpga);
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

            CmdStart(cmd, "rtc set...");
            dcmd.RtcSet();
            CmdEnd("ok");
        }

        protected void RtcCal(CmdLine cmd) {

            CmdStart(cmd, "rtc cal...");
            string msg;
            int arg = cmd.getInt(Cli.ArgScmd);

            msg = dcmd.RtcCal(arg);
            CmdEnd("ok");

            Tools.PrintLine(msg, inf_color);
        }

        protected void McuApp(CmdLine cmd) {

            CmdStart(cmd, "mcu app install...");

            string path = cmd.getStr(Cli.ArgFile);
            dcmd.McuApp(path);

            CmdEnd("ok");
        }

        protected void McuBoot(CmdLine cmd) {

            CmdStart(cmd, "mcu boot install...");

            string path = cmd.getStr(Cli.ArgFile);
            dcmd.McuBoot(path);

            CmdEnd("ok");
        }

        protected void UsbSpd(CmdLine cmd) {

            CmdStart(cmd, "usb speed test...\n");

            int addr = 0;
            int len = 0x100000;

            if (cmd.HasArg(Cli.ArgAddr)) {
                addr = cmd.getInt(Cli.ArgAddr);
            }

            if (cmd.HasArg(Cli.ArgLen)) {
                len = cmd.getInt(Cli.ArgLen);
            }


            byte[] buff = new byte[len];
            DateTime t;
            long t_ms;

            ConsoleColor old_color = Console.ForegroundColor;
            Console.ForegroundColor = inf_color;

            Console.Write("Read....");
            t = DateTime.Now;
            dcmd.MemRD(addr, buff, 0, buff.Length);
            t_ms = (DateTime.Now.Ticks - t.Ticks) / 10000;
            t_ms = Math.Max(t_ms, 1);
            Console.WriteLine((long)buff.Length * 1000 / t_ms / 1024 + " KB/s");

            Console.Write("Write...");
            t = DateTime.Now;
            dcmd.MemWR(addr, buff, 0, buff.Length);
            t_ms = (DateTime.Now.Ticks - t.Ticks) / 10000;
            t_ms = Math.Max(t_ms, 1);
            Console.WriteLine((long)buff.Length * 1000 / t_ms / 1024 + " KB/s");

            Console.ForegroundColor = old_color;
        }

        protected void DevInf(CmdLine cmd) {

            CmdStart(cmd, "\n");
            string msg = dcmd.DevInf();

            Tools.PrintLine(msg, inf_color);

            if (cmd.HasArg(Cli.ArgFile)) {
                File.WriteAllText(cmd.getStr(Cli.ArgFile), msg);
            }
        }

        protected void UsbPrint(CmdLine cmd) {

            CmdStart(cmd, "\n");

            Console.WriteLine("Press CTRL+X to exit");

            while (true) {

                if (Console.KeyAvailable) {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.X && key.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        break;
                    }
                }

                byte[] buff = dcmd.ConsoleRead();
                if (buff.Length == 0) {
                    Thread.Sleep(1);
                    continue;
                }

                string msg = Encoding.UTF8.GetString(buff);
                Tools.Print(msg, inf_color);
            }

        }

        protected void Screen(CmdLine cmd) {

            CmdStart(cmd, "taking screenshot...");

            string path;

            if (cmd.HasArg(Cli.ArgFile)) {
                path = cmd.getStr(Cli.ArgFile);
            } else {
                string date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                path = date.Replace(":", "").Replace(" ", "_").Replace(".", "-") + ".png";
            }

            dcmd.Screen(path);

            CmdEnd("ok");

            Console.WriteLine("saved: " + path);
        }
        //************************************************************************************************ 
        void CopyFile(CmdLine cmd, string src, string dst) {

            CmdStart(cmd, src + " --> " + dst);
            dcmd.FileCopy(src, dst);
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
