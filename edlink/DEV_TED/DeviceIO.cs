using Edlink.Device;
using System;
using System.Threading;


namespace Edlink.DEV_TED {
    internal class DeviceIO : DeviceIO_V1 {

        public const int PROTOCOL_ID = 0x02;
        public const int DEV_ID_TURBO_PRO = 0x20;
        public const int DEV_ID_TURBO_CORE = 0x26;

        protected override int ADDR_FCI_FIFO => 0x1810000;
        protected override int ADDR_FLA_ICOR => 0x00000;

        public const int ADDR_FCI_RAM1 = 0x0000000;
        public const int ADDR_FCI_RAM2 = 0x0800000;

        public readonly int SIZE_RAM0 = 0x800000;
        public readonly int SIZE_RAM1;

        const byte CMD_HOST_RST = 0x29;

        const int ADDR_FCI_CFG = 0x1800000;

        public const byte HOST_RST_OFF = 0;
        public const byte HOST_RST_ON = 1;


        int rst_state;

        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = false;
            rst_state = HOST_RST_OFF;

            switch (link.DeviceID) {
                case DEV_ID_TURBO_PRO:
                    SIZE_RAM1 = 0x800000;
                    break;
                case DEV_ID_TURBO_CORE:
                    SIZE_RAM1 = 0;
                    break;
                default:
                    SIZE_RAM1 = 0;
                    break;
            }
        }


        internal void HostReset(byte mode) {

            link.TxCMD(CMD_HOST_RST);
            link.Tx8(mode);

            if (rst_state == HOST_RST_OFF && mode != HOST_RST_OFF) {
                Thread.Sleep(50);
            }

            rst_state = mode;
        }


        internal void ConfigReset() {

            byte[] buff = new byte[256];
            MemWR(ADDR_FCI_CFG, buff, 0, buff.Length);
        }

        internal void Stop() {

            if (rst_state != HOST_RST_OFF) {
                HostReset(HOST_RST_OFF);
            }
        }

        internal SysInfo getSysInf() {

            SysInfo inf;

            byte[] buff = base.GetSysInf();

            int ptr = 20;

            inf.serial_g = (UInt32)Link.Num32(buff, ptr);
            ptr += 4;
            inf.serial_l = (UInt32)Link.Num32(buff, ptr);
            ptr += 4;
            inf.boot_ctr = (UInt32)Link.Num32(buff, ptr);
            ptr += 4;
            inf.game_ctr = (UInt32)Link.Num32(buff, ptr);
            ptr += 4;

            inf.asm_date = Link.Num16(buff, ptr);
            ptr += 2;
            inf.asm_time = Link.Num16(buff, ptr);
            ptr += 2;
            inf.sw_date = Link.Num16(buff, ptr);
            ptr += 2;
            inf.sw_time = Link.Num16(buff, ptr);
            ptr += 2;
            inf.sw_ver = Link.Num16(buff, ptr);
            ptr += 2;
            inf.hw_ver = Link.Num16(buff, ptr);
            ptr += 2;
            inf.boot_ver = Link.Num16(buff, ptr);
            ptr += 2;

            inf.device_id = buff[ptr++];

            inf.flash_size = 1 << buff[64 - 6];

            return inf;
        }

        public override void RtcSet(DateTime dt) {
            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

        public override int RtcCal(DateTime dt, byte arg) {
            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }
    }
}
