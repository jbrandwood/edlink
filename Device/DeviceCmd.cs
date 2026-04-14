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
                    dev.EnterServiceMode();
                    break;
                case Cli.ModeApp:
                    dev.ExitServiceMode();
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
            dev.FpgInit(buff);
        }

        public virtual void CopyFile(string src, string dst) {

            byte[] buff;

            if (Link.IsDevPath(src)) {
                dev.FileOpen(Link.GetPath(src), DeviceIO.FA_READ);
                buff = new byte[dev.FileAvailable()];
                dev.FileRead(buff, 0, buff.Length);
                dev.FileClose();
            } else {
                buff = File.ReadAllBytes(src);
            }


            if (Link.IsDevPath(dst)) {
                dev.FileOpen(Link.GetPath(dst), DeviceIO.FA_WRITE | DeviceIO.FA_CREATE_ALWAYS | DeviceIO.FS_MAKEPATH);
                dev.FileWrite(buff, 0, buff.Length);
                dev.FileClose();
            } else {
                File.WriteAllBytes(dst, buff);
            }
        }

        public virtual void RtcSet() {

            int sec = DateTime.Now.Second;
            while (DateTime.Now.Second == sec) ;//sync time
            dev.RtcSet(DateTime.Now);
        }

        public virtual string RtcCal(int cmd) {

            //cmd-0: set time and abort calibraion
            //cmd-1: start calibration
            //cmd-2: finish calibration
            //cmd-3: get current calibration value
            //cmd-4: get estimated calibration value
            //cmd-5: get time deviation in ms

            int resp = 0;
            long delta = NtpTime.GetDeltaTicks();
            int sec = NtpTime.GetDeltaTime(delta).Second;
            while (NtpTime.GetDeltaTime(delta).Second == sec) ;//sync time to the edge of second
            resp = dev.RtcCal(NtpTime.GetDeltaTime(delta), (byte)cmd);

            string sig = resp > 0 ? "+" : "";
            string msg = "";

            msg += "local time deviation: " + delta / TimeSpan.FromMilliseconds(1).Ticks + "ms\n";

            if (cmd == 5) {
                msg += "rtc deviation: " + sig + resp + "ms";
            } else {
                msg += "rtc calibration: " + sig + resp;
            }

            return msg;
        }

        public byte[] ConsoleRead() {

            byte[] buff = new byte[dev.Link.BytesToRead];
            dev.Link.rxData(buff, 0, buff.Length);
            return buff;
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

        public virtual string DevInf() {
            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

    }
}
