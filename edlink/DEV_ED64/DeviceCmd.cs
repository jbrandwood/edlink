using System;
using Edlink.Device;

namespace Edlink.DEV_ED64 {

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

                    case DeviceIO.DEV_ID_ED64_PRO:
                        return "EverDrive-64 PRO";
                    default:
                        return "Uknown EverDrive-64";
                }
            }
        }

        public override void Reset(string mode) {

            mcmd.Test();
            mcmd.ResetToMenu();
        }

        public override void Run(string rom_path, string fpga_path) {

            mcmd.Test();
            mcmd.Run(rom_path);
        }

        public override void McuApp(string path) {

            byte[] buff = Stdio.Read(path);
            //dev.EnterServiceMode();
            dev.McuAppLoad(buff);
        }

        public override void McuBoot(string path) {

            byte[] buff = Stdio.Read(path);
            dev.ExitServiceMode();
            dev.McuBootInstall(buff);
        }

#if WINDOWS
        public override void Screen(string path) {

            byte[] vram = new byte[640 * 240 * 2];
            mcmd.VramDump(vram);
            MenuImage.MakeImage(path, vram);

        }
#endif

        public override string DevInf() {

            string msg = "";

            string serial = "";
            serial += dev.SysGetInf(DeviceIO.SysInf.INFS_SERIAL_G).ToString("X8");
            serial += ".";
            serial += dev.SysGetInf(DeviceIO.SysInf.INFS_SERIAL_L).ToString("X8");

            msg += "device id : " + dev.Link.DeviceID.ToString("X2") + "\n";
            msg += "name      : " + DeviceName + "\n";
            msg += "serial    : " + serial + "\n";
            msg += "build date: " + Tools.TsToDate(dev.SysGetInf(DeviceIO.SysInf.INFS_TS_ASM)) + "\n";
            msg += "bootloader: " + Tools.TsToVersion(dev.SysGetInf(DeviceIO.SysInf.INFS_TS_BOOT)) + "\n";
            msg += "firmware  : " + Tools.TsToVersion(dev.SysGetInf(DeviceIO.SysInf.INFS_TS_FW)) + "\n";
            msg += "cic       : " + Tools.TsToVersion(dev.SysGetInf(DeviceIO.SysInf.INFS_TS_CIC)) + "\n";
            msg += "flash size: " + Tools.SizeToStr(dev.SysGetInf(DeviceIO.SysInf.INFS_FLA_SIZE)) + "\n";
            msg += "rtc calib : " + dev.RtcCal(DateTime.Now, (byte)DeviceIO.Rtcc.GET_CURCAL) + "\n";
            msg += "rom size  : " + Tools.SizeToStr(dev.SysGetInf(DeviceIO.SysInf.INFS_MAX_ROM_SIZE)) + "\n";
            msg += "game ctr  : " + dev.SysGetInf(DeviceIO.SysInf.INFD_GAME_CTR) + "\n";
            msg += "boot ctr  : " + dev.SysGetInf(DeviceIO.SysInf.INFD_BOOT_CTR) + "\n";
            msg += "battery   : " + Tools.VdcToStr(dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_BAT)) + "\n";
            msg += "vcc 3.3   : " + Tools.VdcToStr(dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_3V3)) + "\n";
            msg += "vcc 2.5   : " + Tools.VdcToStr(dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_2V5)) + "\n";
            msg += "vcc 1.2   : " + Tools.VdcToStr(dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_1V2)) + "\n";

            return msg;
        }

        public override void Dscmd(CmdLine cmd) {

            string scmd = cmd.GetStr(Cli.ArgCmd);
     
            switch (scmd) {
                case "cicupd":
                    dev.CicUpd(cmd.GetStr(Cli.ArgFile));
                    break;
                default:
                    throw new CmdException(CmdExceptionType.UnknownCmd);
            }

        }



    }
}
