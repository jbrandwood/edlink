using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Edlink.Device;

namespace Edlink.ED64 {

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

        public override void Reset() {

            mcmd.Test();
            mcmd.RunPreloaded("", MenuCmd.MODE_MENU);
        }

        public override void Run(string rom_path, string fpga_path) {

            mcmd.Test();
            mcmd.Run(rom_path);
        }

        public override void McuApp(string path) {

            byte[] buff = File.ReadAllBytes(path);
            dev.enterServiceMode();
            dev.McuAppLoad(buff);
        }

        public override void McuBoot(string path) {

            byte[] buff = File.ReadAllBytes(path);
            dev.exitServiceMode();
            dev.McuBootInstall(buff);
        }

      
    }
}
