using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Edlink.Device.DeviceIO_V1;

namespace Edlink.EDTURBO {
    internal class DeviceIO : DeviceIO_V1 {

        public const int PROTOCOL_ID = 0x02;
        public const int DEV_ID_TURBO_PRO = 0x20;
        public const int DEV_ID_TURBO_CORE = 0x26;

        protected override int ADDR_FCI_FIFO => 0x1810000;
        protected override int ADDR_FLA_ICOR => 0x00000;

        const byte CMD_HOST_RST = 0x29;

        const int ADDR_FCI_CFG = 0x1800000;



        int rst_state;

        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = false;
        }

        /*
        internal void hostReset(byte rst) {

            if (rst_state == HOST_RST_OFF && rst != HOST_RST_OFF) {
                Thread.Sleep(50);
            }

            link.txCMD(CMD_HOST_RST);
            link.tx8(rst);

            rst_state = rst;
        }

        internal void ConfigReset() {

            byte[] buff = new byte[256];
            MemWR(ADDR_FCI_CFG, buff, 0, buff.Length);
        }

        internal void Stop() {

            if (rst_state != HOST_RST_OFF) {
                hostReset(HOST_RST_OFF);
            }
        }

        internal SysInfo getSysInf() {

            SysInfo inf;

            byte[] buff = base.GetSysInf();

            int ptr = 20;

            inf.serial_g = (UInt32)Link.num32(buff, ptr);
            ptr += 4;
            inf.serial_l = (UInt32)Link.num32(buff, ptr);
            ptr += 4;
            inf.boot_ctr = (UInt32)Link.num32(buff, ptr);
            ptr += 4;
            inf.game_ctr = (UInt32)Link.num32(buff, ptr);
            ptr += 4;

            inf.asm_date = Link.num16(buff, ptr);
            ptr += 2;
            inf.asm_time = Link.num16(buff, ptr);
            ptr += 2;
            inf.sw_date = Link.num16(buff, ptr);
            ptr += 2;
            inf.sw_time = Link.num16(buff, ptr);
            ptr += 2;
            inf.sw_ver = Link.num16(buff, ptr);
            ptr += 2;
            inf.hw_ver = Link.num16(buff, ptr);
            ptr += 2;
            inf.boot_ver = Link.num16(buff, ptr);
            ptr += 2;

            inf.device_id = buff[ptr++];

            inf.flash_size = 1 << buff[64 - 6];

            return inf;
        }*/
    }
}
