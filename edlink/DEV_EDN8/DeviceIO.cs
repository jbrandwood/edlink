using Edlink.Device;
using System;


namespace Edlink.DEV_EDN8 {
    internal class DeviceIO : DeviceIO_V1 {

        public const int PROTOCOL_ID = 0x06;
        public const int DEV_ID_N8_PRO = 0x17;

        protected override int ADDR_FCI_FIFO => 0x1810000;
        protected override int ADDR_FLA_ICOR => 0x80000;

        const int ADDR_FCI_CFG = 0x1800000;

        public const int ADDR_FCI_PRG = 0x0000000;
        public const int ADDR_FCI_CHR = 0x0800000;
        public const int ADDR_FCI_SRM = 0x1000000;

        public const int ADDR_FCI_MENU_PRG = (ADDR_FCI_PRG + 0x7E0000);
        public const int ADDR_FCI_MENU_CHR = (ADDR_FCI_CHR + 0x7E0000);

        public readonly int SIZE_PRG = 0x800000;
        public readonly int SIZE_CHR = 0x800000;
        public readonly int SIZE_SRM = 0x40000;

        public enum CartForm {
            NES,
            FAMICOM,
        };

        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = false;
        }

        public override UInt64 FileAvailable() {

            UInt64 size = base.FileAvailable();
            UInt64 hi = size & 0xffffffff;
            UInt64 lo = size >> 32;
            return lo | (hi << 32);
        }

        public override void FpgInit(byte[] data) {
            base.FpgInit(data);
            ConfigReset();
        }

        public override void FpgInit(string path) {
            base.FpgInit(path);
            ConfigReset();
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
            inf.sw_ver = Link.Num16(buff, ptr);
            ptr += 2;
            inf.hw_ver = Link.Num16(buff, ptr);
            ptr += 2;
            inf.boot_ver = Link.Num16(buff, ptr);
            ptr += 2;

            inf.device_id = buff[ptr++];

            ptr = 64 - 9;
            inf.flash_size = 1 << buff[ptr++];

            inf.sw_date = Link.Num16(buff, ptr);
            ptr += 2;
            inf.sw_time = Link.Num16(buff, ptr);
            ptr += 2;


            return inf;
        }

        internal CartForm getCartForm() {

            byte[] buff = base.GetSysInf();

            return (CartForm)buff[64 - 12];
        }

        void ConfigReset() {

            int cfg_base = 32;
            byte[] cfg = new byte[cfg_base + 16];
            cfg[cfg_base + 7] = 0x80;//ctrl
            cfg[cfg_base + 0] = 0xff;//mapper

            MemWR(ADDR_FCI_CFG, cfg, 0, cfg.Length);
        }
    }
}
