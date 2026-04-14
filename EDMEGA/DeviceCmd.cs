using Edlink.Device;
using Edlink.ED64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.EDMEGA {
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

                    case DeviceIO.DEV_ID_MEGA_PRO:
                        return "Mega EverDrive PRO";
                    case DeviceIO.DEV_ID_MEGA_CORE:
                        return "Mega EverDrive CORE";
                    default:
                        return "Uknown Mega EverDrive";
                }
            }
        }
    }
}
