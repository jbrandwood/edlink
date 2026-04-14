using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.Device {
    internal class DeviceCmdFacotoy {

        public static DeviceCmd Create(Link link) {

            switch (link.ProtocolID) {

                case ED64.DeviceIO.PROTOCOL_ID:
                    return new ED64.DeviceCmd(link);
                case EDMEGA.DeviceIO.PROTOCOL_ID:
                    return new EDMEGA.DeviceCmd(link);
                case EDN8.DeviceIO.PROTOCOL_ID:
                    return new EDN8.DeviceCmd(link);
                default:
                    throw new NotSupportedException("unsupported protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }
        }
    }
}
