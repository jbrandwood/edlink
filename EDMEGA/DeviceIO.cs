using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edlink.EDMEGA {

    internal class DeviceIO : DeviceIO_V1 {

        public const int PROTOCOL_ID = 0x05;
        public const int DEV_ID_MEGA_PRO = 0x18;
        public const int DEV_ID_MEGA_CORE = 0x25;

        protected override int ADDR_FCI_FIFO => 0x1810000;
        protected override int ADDR_FLA_ICOR => 0x80000;

        const byte CMD_HOST_RST = 0x29;

        const int ADDR_FCI_CFG = 0x1800000;

        public const byte HOST_RST_OFF = 0;
        public const byte HOST_RST_SOFT = 1;
        public const byte HOST_RST_HARD = 2;

        int rst_state;

        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = true;
        }

        internal void hostReset(byte rst) {

            if (rst_state == HOST_RST_OFF && rst != HOST_RST_OFF) {
                Thread.Sleep(50);
            }

            link.txCMD(CMD_HOST_RST);
            link.tx8(rst);

            rst_state = rst;
        }

        internal void configReset() {

            byte[] buff = new byte[256];
            MemWR(ADDR_FCI_CFG, buff, 0, buff.Length);
        }

        internal void Stop() {

            if (rst_state != HOST_RST_OFF) {
                hostReset(HOST_RST_OFF);
            }
        }
    }

    
}
