using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edlink {
    internal class DeviceIOFacotoy {

        public static IDeviceIO Create(Link link) {

            switch (link.ProtocolID) {

                case ED64.DeviceIO.PROTOCOL_ID:
                    return new ED64.DeviceIO(link);
                //case MEGA.CmdHandler.PROTOCOL_ID:
                // return new MEGA.CmdHandler(link);
                default:
                    throw new NotSupportedException("Unsupported protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }
        }
    }
}
