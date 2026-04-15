using Edlink.Device;
using Edlink.ED64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Configuration;

namespace Edlink.EDMEGA {
    internal class DeviceCmd : Device.DeviceCmd {

        new DeviceIO dev;
        MenuCmd mcmd;
        //byte default_rst_type = DeviceIO.HOST_RST_SOFT;

        byte rst_mode = DeviceIO.HOST_RST_SOFT;

        public DeviceCmd(Link link) {

            base.dev = dev = new DeviceIO(link);
            mcmd = new MenuCmd(dev);
        }

        public override string DeviceName {

            get {

                switch (dev.Link.DeviceID) {

                    case DeviceIO.DEV_ID_MEGA_PRO:
                        return "Mega EverDrive PRO";
                    case DeviceIO.DEV_ID_MEGA_CORE:
                        return "Mega EverDrive CORE";
                    default:
                        return "Uknown Mega EverDrive";
                }
            }
        }

        public override void Stop() {
            dev.Stop();
        }

        public override void Reset(string mode) {

            mode = mode.ToLower().Trim();

            if (mode.Equals("hard")) {
                rst_mode = DeviceIO.HOST_RST_HARD;
            } else
            if (mode.Equals("soft")) {
                rst_mode = DeviceIO.HOST_RST_SOFT;
            } else
            if (mode.Equals("off")) {
                dev.hostReset(DeviceIO.HOST_RST_OFF);
                return;
            }

            dev.hostReset(rst_mode);
        }

        public override void Run(string rom_path, string fpga_path) {

            string app_dst;

            mcmd.ResetToMenu(rst_mode);
            app_dst = base.AppDeploy(rom_path, fpga_path);
            mcmd.AppInstall(Link.GetDevPath(app_dst));
            mcmd.AppStart();
        }

        public override void McuApp(string path) {

            byte[] buff = File.ReadAllBytes(path);
            dev.McuAppLoad(buff);
        }

        public override void Screen(string path) {

            byte[] vram = new byte[0x10000];
            byte[] palette = new byte[128];

            mcmd.VramDump(vram, palette);
            MenuImage.makeImage(path, vram, palette);
        }

        public override string DevInf() {

            string msg = "";

            DeviceIO.SysInfo sys_inf = dev.getSysInf();
            DeviceIO.Vdc vdc = dev.GetVdc();

            string serial = "";
            serial += sys_inf.serial_g.ToString("X8");
            serial += ".";
            serial += sys_inf.serial_l.ToString("X8");

            msg += "device id : " + dev.Link.DeviceID.ToString("X2") + "\n";
            msg += "name      : " + DeviceName + "\n";
            msg += "serial    : " + serial + "\n";

            msg += "hw version: " + sys_inf.hw_ver.ToString("X4") + "\n";
            msg += "build date: " + Tools.TsToDate(sys_inf.asm_date) + "\n";
            msg += "bootloader: " + sys_inf.boot_ver.ToString("X4") + "\n";
            msg += "mcu core  : " + Tools.TsToVersion(sys_inf.sw_date) + "\n";
            msg += "flash size: " + Tools.SizeToStr(sys_inf.flash_size) + "\n";
            msg += "rtc calib : " + dev.RtcCal(DateTime.Now, (byte)DeviceIO.Rtcc.GET_CURCAL) + "\n";
            msg += "game ctr  : " + sys_inf.game_ctr + "\n";
            msg += "boot ctr  : " + sys_inf.boot_ctr + "\n";
            msg += "battery   : " + Tools.VdcToStr(vdc.bat) + "\n";
            msg += "vcc 5.0   : " + Tools.VdcToStr(vdc.v50) + "\n";
            msg += "vcc 2.5   : " + Tools.VdcToStr(vdc.v25) + "\n";
            msg += "vcc 1.2   : " + Tools.VdcToStr(vdc.v12) + "\n";

            return msg;
        }
    }
}
