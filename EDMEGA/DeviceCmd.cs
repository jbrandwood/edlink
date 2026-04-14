using Edlink.Device;
using Edlink.ED64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

            mcmd.ResetToMenu(rst_mode);

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

                base.FileCopy(rom_path, app_dst);
            }

            if (fpga_path != null) {
                base.FileCopy(fpga_path, usb_home + "/" + Path.GetFileName(fpga_path));
            }

            mcmd.AppInstall(Link.GetDevPath(app_dst));
            mcmd.AppStart();
        }

        public override void McuApp(string path) {

            byte[] buff = File.ReadAllBytes(path);
            dev.McuAppLoad(buff);
        }
    }
}
