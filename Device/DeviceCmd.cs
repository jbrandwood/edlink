using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edlink.Device {
    internal class DeviceCmd {

        protected DeviceIO dev;

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

        public int UsbRD(byte[] buff, int offset, int len) {

            len = Math.Min(len, dev.Link.BytesToRead);
            dev.Link.rxData(buff, offset, len);
            return len;
        }

        public virtual void FpgaInit(string path) {

            if (Link.IsDevPath(path)) {
                dev.FpgInit(Link.GetDevPath(path));
            } else {
                dev.FpgInit(File.ReadAllBytes(path));
            }
        }

        public virtual void FileCopy(string src, string dst) {


            byte[] buff;

            if (Link.IsDevPath(src)) {
                dev.FileOpen(Link.GetDevPath(src), DeviceIO.FA_READ);
                buff = new byte[dev.FileAvailable()];
                dev.FileRead(buff, 0, buff.Length);
                dev.FileClose();
            } else {
                buff = File.ReadAllBytes(src);
            }

            if (Link.IsDevPath(dst)) {
                dev.FileOpen(Link.GetDevPath(dst), DeviceIO.FA_WRITE | DeviceIO.FA_CREATE_ALWAYS | DeviceIO.FS_MAKEPATH);
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

            //Cmd-0: set time and abort calibraion
            //Cmd-1: start calibration
            //Cmd-2: finish calibration
            //Cmd-3: get current calibration value
            //Cmd-4: get estimated calibration value
            //Cmd-5: get time deviation in ms

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




        public virtual void Reset(string mode) {
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

        public virtual void Diag() {
            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

        public virtual void Dscmd(CmdLine cmd) {
            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

        protected string AppDeploy(string rom_path, string fpga_path) {

            string usb_home;
            string app_dst;

            if (Link.IsDevPath(rom_path)) {

                usb_home = Path.GetDirectoryName(rom_path);
                app_dst = rom_path;

            } else {

                usb_home = Link.MakeDevPath("usb-games");

                if (fpga_path != null) {
                    usb_home += "/" + Path.GetFileName(rom_path) + ".fpgrom";
                }
                app_dst = usb_home + "/" + Path.GetFileName(rom_path);

                FileCopy(rom_path, app_dst);
            }

            if (fpga_path != null) {
                FileCopy(fpga_path, usb_home + "/mapper" + Path.GetExtension(fpga_path));
            }

            return app_dst;
        }

    }
}
