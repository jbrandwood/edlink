using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.EDN8 {
    internal class DeviceCmd : Device.DeviceCmd {

        new DeviceIO dev;
        //MenuCmd mcmd;

        public DeviceCmd(Link link) {

            base.dev = dev = new DeviceIO(link);
            //mcmd = new MenuCmd(dev);
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

    }
}
