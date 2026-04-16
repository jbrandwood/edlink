using Edlink.Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.EDN8 {
    internal class DeviceCmd : Device.DeviceCmd {

        new DeviceIO dev;
        MenuCmd mcmd;

        public DeviceCmd(Link link) {

            base.dev = dev = new DeviceIO(link);
            mcmd = new MenuCmd(dev);
        }

        public override string DeviceName {

            get {

                switch (dev.Link.DeviceID) {

                    case DeviceIO.DEV_ID_N8_PRO:
                        return "EverDrive-N8 PRO";
                    default:
                        return "Uknown EverDrive-N8";
                }
            }
        }


        public override void Reset(string mode) {

            mcmd.Test();
            mcmd.Reset();
        }

        public override void Run(string rom_path, string fpga_path) {


            string app_dst;

            mcmd.Test();
            app_dst = base.AppDeploy(rom_path, fpga_path);
            mcmd.AppInstall(Link.GetDevPath(app_dst));
            mcmd.AppStart();

        }

        public override void McuApp(string path) {

            byte[] buff = File.ReadAllBytes(path);
            dev.McuAppLoad(buff);
        }

        public override void Screen(string path) {

            byte[] vram = new byte[2048];
            byte[] palette = new byte[16];
            byte[] chr = new byte[8192];

            mcmd.VramDump(vram, palette);
            MemRD(DeviceIO.ADDR_FCI_MENU_CHR, chr, 0, chr.Length);

            MenuImage.makeImage(path, chr, vram, palette);
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
            msg += "formfactor: " + dev.getCartForm() + "\n";

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
