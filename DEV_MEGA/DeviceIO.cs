using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Edlink.DEV_MEGA {

    internal class DeviceIO : DeviceIO_V1 {

        public const int PROTOCOL_ID = 0x05;
        public const int DEV_ID_MEGA_PRO = 0x18;
        public const int DEV_ID_MEGA_CORE = 0x25;

        protected override int ADDR_FCI_FIFO => 0x1810000;
        protected override int ADDR_FLA_ICOR => 0x80000;

        const byte CMD_HOST_RST = 0x29;

        public const int ADDR_PRG1 = 0x0000000;
        public const int ADDR_PRG2 = 0x0800000;
        public const int ADDR_SRAM = 0x1000000;
        public const int ADDR_BRAM = 0x1080000;

        const int ADDR_FCI_CFG = 0x1800000;

        public const byte HOST_RST_OFF = 0;
        public const byte HOST_RST_SOFT = 1;
        public const byte HOST_RST_HARD = 2;

        public readonly int SIZE_PRG1 = 0x800000;
        public readonly int SIZE_PRG2;
        public readonly int SIZE_SRAM;
        public readonly int SIZE_BRAM;

            int rst_state;

        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = true;
            rst_state = HOST_RST_OFF;


            switch (link.DeviceID) {
                case DEV_ID_MEGA_PRO:
                    SIZE_PRG2 = 0x800000;
                    SIZE_SRAM = 0x80000;
                    SIZE_BRAM = 0x80000;
                    break;
                case DEV_ID_MEGA_CORE:
                    SIZE_PRG2 = 0;
                    SIZE_SRAM = 0;
                    SIZE_BRAM = 0x20000;
                    break;
                default:
                    SIZE_PRG2 = 0;
                    SIZE_SRAM = 0;
                    SIZE_BRAM = 0;
                    break;
            }
        }

        internal void hostReset(byte rst) {
            
            link.txCMD(CMD_HOST_RST);
            link.tx8(rst);

            if (rst_state == HOST_RST_OFF && rst != HOST_RST_OFF) {
                Thread.Sleep(50);
            }

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
        }

    }
}
