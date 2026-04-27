using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.DEV_GBA {
    internal class DeviceIO : DeviceIO_V1 {


        public const int PROTOCOL_ID = 0x04;
        public const int DEV_ID_GBA_PRO = 0x24;

        protected override int ADDR_FCI_FIFO => (ADDR_FCI_SYS + 0x10000);
        protected override int ADDR_FLA_ICOR => 0x00000;

        public const int ADDR_FCI_RAM0 = 0x0000000;
        public const int ADDR_FCI_RAM1 = 0x4000000;

        const int ADDR_FCI_SYS = 0x8800000;               //system registers
        const int ADDR_FCI_CFG = (ADDR_FCI_SYS + 0x00000); //system config
        const int ADDR_FCI_CTRL = (ADDR_FCI_CFG + 0xff);  //cfg->ctrl byte

        const int ADDR_FCI_ROM = (ADDR_FCI_RAM0 + 0x0000000);
        const int ADDR_FCI_MENU = (ADDR_FCI_RAM1 + 0x0000000);
        const int ADDR_FCI_BMOD = (ADDR_FCI_MENU + 0x00000BE);

        const int CFG_CTRL_BOOT_ON = 0x01;

        public readonly int SIZE_RAM0 = 0x2000000;
        public readonly int SIZE_RAM1 = 0x0800000;

        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = false;

        }

        internal void BootSW(bool boot_on) {

            byte[] cmd = new byte[1];
            cmd[0] = (byte)(boot_on ? CFG_CTRL_BOOT_ON : 0);
            MemWR(ADDR_FCI_CTRL, cmd, 0, cmd.Length);
        }

        internal void SetBootMode(int mode) {

            byte[] cmd = new byte[2];
            cmd[0] = (byte)(mode >> 0);
            cmd[1] = (byte)(mode >> 8);
            MemWR(ADDR_FCI_BMOD, cmd, 0, cmd.Length);
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
            inf.boot_ver = Link.num16(buff, ptr);
            ptr += 2;
            ptr += 2;
            inf.hw_ver = Link.num16(buff, ptr);
            ptr += 2;
            inf.device_id = buff[ptr++];

            inf.flash_size = 1 << buff[64 - 6];

            inf.sw_ver = Link.num16(buff, 64 - 4);
           
            return inf;
        }

    }
}
