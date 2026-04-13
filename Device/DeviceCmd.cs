using Edlink.ED64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Edlink.Device {
    internal class DeviceCmd {

        protected IDeviceIO dev;

        public virtual string DeviceName {

            get { return "Uknown device"; }
        }

        public virtual void Stop() {

        }

        public virtual void McuMode(string mode) {

            switch (mode) {
                case Cli.ModeService:
                    dev.enterServiceMode();
                    break;
                case Cli.ModeApp:
                    dev.exitServiceMode();
                    break;
                default:
                    throw new CmdException(CmdExceptionType.Mode, mode);
            }
        }

        public virtual void MemWR(int addr, byte[] buff, int offset, int len) {

            dev.MemWR(addr, buff, offset, len);
        }

        public virtual void MemRD(int addr, byte[] buff, int offset, int len) {

            dev.MemRD(addr, buff, offset, len);
        }

        public virtual void FlaWR(int addr, byte[] buff, int offset, int len) {

            dev.FlaWR(addr, buff, offset, len);
        }

        public virtual void FlaRD(int addr, byte[] buff, int offset, int len) {

            dev.FlaRD(addr, buff, offset, len);
        }

        public virtual void FpgaInit(string path) {
            byte[] buff = File.ReadAllBytes(path);
            dev.fpgInit(buff);
        }

        public virtual void CopyFile(string src, string dst) {

            byte[] buff;

            if (Link.IsDevPath(src)) {
                dev.fileOpen(Link.GetPath(src), DeviceIO.FA_READ);
                buff = new byte[dev.fileAvailable()];
                dev.fileRead(buff, 0, buff.Length);
                dev.fileClose();
            } else {
                buff = File.ReadAllBytes(src);
            }


            if (Link.IsDevPath(dst)) {
                dev.fileOpen(Link.GetPath(dst), DeviceIO.FA_WRITE | DeviceIO.FA_CREATE_ALWAYS | DeviceIO.FS_MAKEPATH);
                dev.fileWrite(buff, 0, buff.Length);
                dev.fileClose();
            } else {
                File.WriteAllBytes(dst, buff);
            }
        }

        public virtual void RtcSet() {

            int sec = DateTime.Now.Second;
            while (DateTime.Now.Second == sec) ;//sync time
            dev.rtcSet(DateTime.Now);
        }

        public virtual string RtcCal(int cmd) {

            //cmd-0: set time and abort calibraion
            //cmd-1: start calibration
            //cmd-2: finish calibration
            //cmd-3: get current calibration value
            //cmd-4: get estimated calibration value
            //cmd-5: get time deviation in ms

            int resp;

            int sec = DateTime.Now.Second;
            while (DateTime.Now.Second == sec) ;//sync time

            resp = dev.RtcCal(DateTime.Now, (byte)cmd);

            string sig = resp > 0 ? "+" : "";


            if (cmd == 5) {
                return "rtc deviation: " + sig + resp + "ms";
            } else {
                return "rtc calibration: " + sig + resp;
            }
        }

        public virtual void Reset() {
            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

        public virtual void Run(string rom_path, string fpga_path) {
            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

        public virtual void McuUpd(string path, string mode) {

            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

        public virtual void McuApp(string path) {

            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

        public virtual void McuBoot(string path) {

            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

        public virtual void Screen(string path) {

            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }



        public void UsbSpd(int addr, int len) {

            byte[] buff = new byte[len];
            DateTime t;
            long t_ms;

            Console.Write("Read....");
            t = DateTime.Now;
            MemRD(addr, buff, 0, buff.Length);
            t_ms = (DateTime.Now.Ticks - t.Ticks) / 10000;
            Console.WriteLine((long)buff.Length * 1000 / t_ms / 1024 + " KB/s");

            Console.Write("Write...");
            t = DateTime.Now;
            MemWR(addr, buff, 0, buff.Length);
            t_ms = (DateTime.Now.Ticks - t.Ticks) / 10000;
            Console.WriteLine((long)buff.Length * 1000 / t_ms / 1024 + " KB/s");
        }

    }
}
