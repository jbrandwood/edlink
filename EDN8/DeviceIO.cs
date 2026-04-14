using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.EDN8 {
    internal class DeviceIO : DeviceIO_V1 {

        public const int PROTOCOL_ID = 0x06;
        public const int DEV_ID_N8_PRO = 0x17;

        protected override int ADDR_FCI_FIFO => 0x1810000;
        protected override int ADDR_FLA_ICOR => 0x80000;


       
        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = false;
        }
    }
}
