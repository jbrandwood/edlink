using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.EDTURBO {
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

                    case DeviceIO.DEV_ID_TURBO_PRO:
                        return "Turbo EverDrive PRO";
                    case DeviceIO.DEV_ID_TURBO_CORE:
                        return "Turbo EverDrive CORE";
                    default:
                        return "Uknown Turbo EverDrive";
                }
            }
        }

    }
}
