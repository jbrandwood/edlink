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

            string mode = cmd.GetStr(Cli.ArgMode);
            CmdStart(cmd, "set mode: " + mode + "...");
            dcmd.McuMode(mode);
            CmdEnd("ok");
        }

        protected void LinkConfig(CmdLine cmd, Link link) {//move to Cmd base


            if (cmd.HasArg(Cli.ArgPort)) {
                link.PortName = cmd.GetStr(Cli.ArgPort);
            }

            if (cmd.HasArg(Cli.ArgDevId)) {
                link.DeviceID = (byte)cmd.GetInt(Cli.ArgDevId);
            }

            if (cmd.HasArg(Cli.ArgProtId)) {
                link.ProtocolID = (byte)cmd.GetInt(Cli.ArgProtId);
            }
        }



        protected void MemRD(CmdLine cmd) {

            CmdStart(cmd, "memory read...");

            bool print = cmd.HasArg(Cli.ArgPrint);
            int addr = cmd.GetInt(Cli.ArgAddr);
            int len = cmd.GetInt(Cli.ArgLen);
            string path;

            if (cmd.HasArg(Cli.ArgFile) || !print) {
                path = cmd.GetStr(Cli.ArgFile);
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

                    string msg_txt = "";

                    for (int u = 0; u < block; u++) {
                        byte val = buff[i + u];
                        msg_txt += val < 32 || val > 126 ? '.' : (char)val;
                    }

                    Tools.PrintLine(msg_hex + "  " + msg_txt, inf_color);

                    i += block;
                }
            }
        }

        protected void MemWR(CmdLine cmd) {

            CmdStart(cmd, "memory write...");

            string path = cmd.GetStr(Cli.ArgFile);
            int addr = cmd.GetInt(Cli.ArgAddr);
            int len;
            int offset = 0;

            byte[] buff = File.ReadAllBytes(path);

            if (cmd.HasArg(Cli.ArgLen)) {
                len = cmd.GetInt(Cli.ArgLen);
            } else {
                len = buff.Length;
            }

            if (cmd.HasArg(Cli.ArgOffset)) {
                offset = cmd.GetInt(Cli.ArgOffset);
            }

            dcmd.MemWR(addr, buff, offset, len);

            CmdEnd("ok");
        }

        protected void FlaRD(CmdLine cmd) {

            CmdStart(cmd, "flash read read...");

            string path = cmd.GetStr(Cli.ArgFile);
            int addr = cmd.GetInt(Cli.ArgAddr);
            int len = cmd.GetInt(Cli.ArgLen);

            byte[] buff = new byte[len];
            dcmd.FlaRD(addr, buff, 0, len);

            File.WriteAllBytes(path, buff);

            CmdEnd("ok");
        }

        protected void FlaWR(CmdLine cmd) {

            CmdStart(cmd, "flash write.");

            string path = cmd.GetStr(Cli.ArgFile);
            int addr = cmd.GetInt(Cli.ArgAddr);
            int len;
            int offset = 0;

            byte[] buff = File.ReadAllBytes(path);

            if (cmd.HasArg(Cli.ArgLen)) {
                len = cmd.GetInt(Cli.ArgLen);
            } else {
                len = buff.Length;
            }

            if (cmd.HasArg(Cli.ArgOffset)) {
                offset = cmd.GetInt(Cli.ArgOffset);
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

        protected void UsbRD(CmdLine cmd) {

            CmdStart(cmd, "\n");

            FileStream fs = null;
            bool print;
            int len = 0;
            int byte_ctr = 0;
            int bytes_ctr_old = -1;

            print = cmd.HasArg(Cli.ArgPrint);

            if (cmd.HasArg(Cli.ArgLen)) {
                len = cmd.GetInt(Cli.ArgLen);
            }

            if (cmd.HasArg(Cli.ArgFile) || !print) {
                fs = new FileStream(cmd.GetStr(Cli.ArgFile), FileMode.Create, FileAccess.Write);
            }

            if (print) {
                Console.WriteLine("Press CTRL+Q to exit");
                Console.WriteLine("Press CTRL+X to clear console");
            }

            byte[] buff = new byte[1024];
            int console_base = Console.CursorTop;

            while (true) {

                if (print && Console.KeyAvailable) {

                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q && key.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        break;
                    }

                    if (key.Key == ConsoleKey.X && key.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        Console.Clear();
                    }
                }

                int block = buff.Length;
                if (len != 0) {
                    block = Math.Min(block, len);
                }
                block = dcmd.UsbRD(buff, 0, block);
                byte_ctr += block;

                if (!print && byte_ctr != bytes_ctr_old) {
                    Console.CursorLeft = 0;
                    Console.CursorTop = console_base;
                    Tools.Print("bytes recived: " + byte_ctr, inf_color);
                    bytes_ctr_old = byte_ctr;
                }

                if (block == 0) {
                    Thread.Sleep(1);
                    continue;
                }

                if (print) {
                    string msg = Encoding.UTF8.GetString(buff, 0, block);
                    Tools.Print(msg, inf_color);
                }

                if (fs != null) {
                    fs.Write(buff, 0, block);
                    fs.Flush();
                }

                if (len != 0) {
                    len -= block;
                    if (len <= 0) {
                        break;
                    }
                }

            }

            if (fs != null) {
                fs.Close();
            }

            Console.WriteLine();
        }

        protected void Reset(CmdLine cmd) {

            CmdStart(cmd, "device reset...");

            string mode = "";

            if (cmd.HasArg(Cli.ArgMode)) {
                mode = cmd.GetStr(Cli.ArgMode);
            }

            dcmd.Reset(mode);
            CmdEnd("ok");
        }

        protected void Run(CmdLine cmd) {

            CmdStart(cmd, "run application...");

            string rom_path = cmd.GetStr(Cli.ArgFile);
            string fpga_path = null;

            if (cmd.HasArg(Cli.ArgFpga)) {
                fpga_path = cmd.GetStr(Cli.ArgFpga);
            }

            dcmd.Run(rom_path, fpga_path);

            CmdEnd("ok");
        }

        protected void FpgaInit(CmdLine cmd) {

            CmdStart(cmd, "fpga init...");
            string path = cmd.GetStr(Cli.ArgFile);
            dcmd.FpgaInit(path);
            CmdEnd("ok");
        }

        protected void Copy(CmdLine cmd) {

            string src = cmd.GetStr(Cli.ArgSrc);
            string dst = cmd.GetStr(Cli.ArgDst);

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

            if (cmd.HasArg(Cli.ArgVal)) {
                int arg = cmd.GetInt(Cli.ArgVal);
                msg = dcmd.RtcCalSet(arg);
            } else {
                int arg = cmd.GetInt(Cli.ArgCmd);
                msg = dcmd.RtcCal(arg);
            }

            CmdEnd("ok");

            Tools.PrintLine(msg, inf_color);
        }

        protected void McuApp(CmdLine cmd) {

            CmdStart(cmd, "mcu app install...");

            string path = cmd.GetStr(Cli.ArgFile);
            dcmd.McuApp(path);

            CmdEnd("ok");
        }

        protected void McuBoot(CmdLine cmd) {

            CmdStart(cmd, "mcu boot install...");

            string path = cmd.GetStr(Cli.ArgFile);
            dcmd.McuBoot(path);

            CmdEnd("ok");
        }

        protected void UsbSpd(CmdLine cmd) {

            CmdStart(cmd, "usb speed test...\n");

            int addr = 0;
            int len = 0x100000;

            if (cmd.HasArg(Cli.ArgAddr)) {
                addr = cmd.GetInt(Cli.ArgAddr);
            }

            if (cmd.HasArg(Cli.ArgLen)) {
                len = cmd.GetInt(Cli.ArgLen);
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
                File.WriteAllText(cmd.GetStr(Cli.ArgFile), msg);
            }
        }

        protected void Screen(CmdLine cmd) {

            CmdStart(cmd, "taking screenshot...");

            string path;

            if (cmd.HasArg(Cli.ArgFile)) {
                path = cmd.GetStr(Cli.ArgFile);
            } else {
                string date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                path = date.Replace(":", "").Replace(" ", "_").Replace(".", "-") + ".png";
            }

            dcmd.Screen(path);

            CmdEnd("ok");

            Console.WriteLine("saved: " + path);
        }

        protected void Diag(CmdLine cmd) {

            CmdStart(cmd, "\n");
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            dcmd.Diag();
            Console.ForegroundColor = old;
        }

        protected void Dscmd(CmdLine cmd) {

            CmdStart(cmd, cmd.GetStr(Cli.ArgCmd) + "...");
            dcmd.Dscmd(cmd);
            CmdEnd("ok");
        }

        protected void NetGate(CmdLine cmd) {

            CmdStart(cmd, "\n");
            dcmd.NetGate(cmd);
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
