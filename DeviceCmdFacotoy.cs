using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edlink {
    internal class DeviceCmdFacotoy {

        public static IDeviceCmd Create(Link link) {

            switch (link.ProtocolID) {

                case ED64.DeviceIO.PROTOCOL_ID:
                    return new ED64.DeviceCmd(link);
                //case MEGA.CliHandler.PROTOCOL_ID:
                // return new MEGA.CliHandler(link);
                default:
                    throw new NotSupportedException("unsupported protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }
        }
    }
}
