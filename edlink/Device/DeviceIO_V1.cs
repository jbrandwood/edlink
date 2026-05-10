using System;


namespace Edlink.Device {

    abstract class DeviceIO_V1 : DeviceIO {


        protected abstract int ADDR_FCI_FIFO { get; } //host fifo
        protected abstract int ADDR_FLA_ICOR { get; } //mcu firmware update

        //************************************************* commands

        const byte CMD_STATUS = 0x10;
        const byte CMD_GET_MODE = 0x11;
        const byte CMD_RST_MCU = 0x12;
        const byte CMD_GET_VDC = 0x13;
        const byte CMD_RTC_GET = 0x14;
        const byte CMD_RTC_SET = 0x15;
        const byte CMD_FLA_RD = 0x16;
        const byte CMD_FLA_WR = 0x17;
        const byte CMD_FLA_WR_SDC = 0x18;
        const byte CMD_MEM_RD = 0x19;
        const byte CMD_MEM_WR = 0x1A;
        const byte CMD_MEM_SET = 0x1B;
        const byte CMD_MEM_TST = 0x1C;
        const byte CMD_MEM_CRC = 0x1D;
        const byte CMD_FPG_USB = 0x1E;
        const byte CMD_FPG_SDC = 0x1F;
        const byte CMD_FPG_FLA = 0x20;
        const byte CMD_RTC_CAL = 0x21;
        const byte CMD_USB_WR = 0x22;
        const byte CMD_FIFO_WR = 0x23;
        const byte CMD_UART_WR = 0x24;
        const byte CMD_RST_EFU = 0x25;
        const byte CMD_SYS_INF = 0x26;
        const byte CMD_GAME_CTR = 0x27;
        const byte CMD_UPD_EXEC = 0x28;
        const byte CMD_HOST_RST = 0x29;

        const byte CMD_STATUS2 = 0x40;

        const byte CMD_DISK_INIT = 0xC0;
        const byte CMD_DISK_RD = 0xC1;
        const byte CMD_DISK_WR = 0xC2;
        const byte CMD_F_DIR_OPN = 0xC3;
        const byte CMD_F_DIR_RD = 0xC4;
        const byte CMD_F_DIR_LD = 0xC5;
        const byte CMD_F_DIR_SIZE = 0xC6;
        const byte CMD_F_DIR_PATH = 0xC7;
        const byte CMD_F_DIR_GET = 0xC8;
        const byte CMD_F_FOPN = 0xC9;
        const byte CMD_F_FRD = 0xCA;
        const byte CMD_F_FRD_MEM = 0xCB;
        const byte CMD_F_FWR = 0xCC;
        const byte CMD_F_FWR_MEM = 0xCD;
        const byte CMD_F_FCLOSE = 0xCE;
        const byte CMD_F_FPTR = 0xCF;
        const byte CMD_F_FINFO = 0xD0;
        const byte CMD_F_FCRC = 0xD1;
        const byte CMD_F_DIR_MK = 0xD2;
        const byte CMD_F_DEL = 0xD3;
        const byte CMD_F_AVB = 0xD5;

        const byte CMD_USB_RECOV = 0xF0;
        const byte CMD_RUN_APP = 0xF1;

        const int BMOD_MCU_APP = 0xA0;
        const int BMOD_MCU_SER = 0xA1;
        const int BMOD_MCU_UPD = 0xA3;

        const int RTC_SIZE = 6;
        public struct SysInfo {

            public UInt32 serial_g;
            public UInt32 serial_l;
            public UInt32 boot_ctr;
            public UInt32 game_ctr;

            public UInt16 asm_date;
            public UInt16 asm_time;
            public UInt16 sw_date;
            public UInt16 sw_time;
            public UInt16 sw_ver;
            public UInt16 hw_ver;
            public UInt16 boot_ver;
            public byte device_id;
            public int flash_size;
        }

        public struct Vdc {

            public UInt16 v50;
            public UInt16 v25;
            public UInt16 v12;
            public UInt16 bat;
        }

        public override Link Link {
            get { return link; }
        }
        public override void ExitServiceMode() {

            if (!IsServiceMode()) {
                return;
            }

            link.TxCMD(CMD_RUN_APP);

            BootWait();
            if (IsServiceMode()) {
                throw new Exception("failed to exit service mode");
            }
        }

        public override void EnterServiceMode() {

            if (IsServiceMode()) {
                return;
            }

            link.TxCMD(CMD_RST_MCU);
            link.Tx8(BMOD_MCU_SER);//only gba require ser mode, older carts accept any val

            BootWait();

            if (!IsServiceMode()) {
                throw new Exception("failed to enter service mode");
            }
        }


        public override void MemWR(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }

            link.TxCMD(CMD_MEM_WR);
            link.Tx32(addr);
            link.Tx32(len);
            link.Tx8(0);//exec
            link.TxData(buff, offset, len);
        }
        public override void MemRD(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }

            link.TxCMD(CMD_MEM_RD);
            link.Tx32(addr);
            link.Tx32(len);
            link.Tx8(0);//exec
            link.RxData(buff, offset, len);
        }
        public override void FlaWR(int addr, byte[] buff, int offset, int len) {

            link.TxCMD(CMD_FLA_WR);
            link.Tx32(addr);
            link.Tx32(len);
            link.TxDataACK(buff, offset, len);
            CheckStatus();
        }
        public override void FlaRD(int addr, byte[] buff, int offset, int len) {

            link.TxCMD(CMD_FLA_RD);
            link.Tx32(addr);
            link.Tx32(len);
            link.RxData(buff, offset, len);
        }

        public override void FifoWR(byte[] data, int offset, int len) {

            MemWR(ADDR_FCI_FIFO, data, offset, len);
        }

        public override void FpgInit(byte[] data) {

            link.TxCMD(CMD_FPG_USB);
            link.Tx32(data.Length);
            link.TxDataACK(data, 0, data.Length);
            CheckStatus();
        }

        public override void FpgInit(string path) {

            FileOpen(path, FA_READ);
            int size = (int)FileAvailable();
            link.TxCMD(CMD_FPG_SDC);
            link.Tx32(size);
            link.Tx8(0);
            CheckStatus();
        }
        public override void FileOpen(string path, int mode) {

            MakePath(path, mode);

            link.TxCMD(CMD_F_FOPN);
            link.Tx8(mode & ~FS_MAKEPATH);
            link.TxString(path);
            CheckStatus();
        }
        public override void FileClose() {

            link.TxCMD(CMD_F_FCLOSE);
            CheckStatus();
        }
        public override UInt64 FileAvailable() {

            link.TxCMD(CMD_F_AVB);

            UInt64 hi = (UInt64)link.Rx32();
            UInt64 lo = (UInt64)link.Rx32();

            return lo | (hi << 32);
        }
        public override void FileRead(byte[] buff, int offset, int len) {

            link.TxCMD(CMD_F_FRD);
            link.Tx32(len);

            while (len > 0) {

                int block = 4096;
                if (block > len) block = len;
                int resp = link.Rx8();
                if (resp != 0) {
                    throw new Exception("file read error: " + resp.ToString("X2"));
                }

                link.RxData(buff, offset, block);
                offset += block;
                len -= block;
            }
        }
        public override void FileWrite(byte[] buff, int offset, int len) {

            link.TxCMD(CMD_F_FWR);
            link.Tx32(len);
            link.TxDataACK(buff, offset, len);
            CheckStatus();
        }

        public override RtcTime RtcGet() {

            link.TxCMD(CMD_RTC_GET);
            byte[] buff = link.RxData(RTC_SIZE);
            return new RtcTime(buff);
        }

        public override void RtcSet(DateTime dt) {

            RtcTime rtc = new RtcTime(dt);
            byte[] vals = rtc.GetVals();
            link.TxCMD(CMD_RTC_SET);
            link.TxData(vals, 0, RTC_SIZE);
        }

        public override int RtcCal(DateTime dt, byte arg) {

            RtcTime rtc = new RtcTime(dt);
            byte[] vals = rtc.GetVals();

            link.TxCMD(CMD_RTC_CAL);
            link.TxData(vals, 0, 6);
            link.Tx8(arg);

            return link.Rx32();
        }

        public override void RtcCalSet(int ppm_val) {

            throw new CmdException(CmdExceptionType.UnsupportedCmd);
        }

        public void McuAppLoad(byte[] data) {

            if (data.Length > 0x40000) {
                throw new Exception("mcu app file size exceeds limit");
            }

            FlaWR(ADDR_FLA_ICOR, data, 0, data.Length);
            int crc = (data[4] << 0) | (data[5] << 8) | (data[6] << 16) | (data[7] << 24);
            UpdExec(ADDR_FLA_ICOR, crc);
        }

        public Vdc GetVdc() {

            link.TxCMD(CMD_GET_VDC);

            Vdc vdc;
            vdc.v50 = link.Tx16();
            vdc.v25 = link.Tx16();
            vdc.v12 = link.Tx16();
            vdc.bat = link.Tx16();
            return vdc;
        }

        public void ResetEfu(int tout_sec) {

            link.TxCMD(CMD_RST_EFU);
            link.Tx8(0);//ack
            BootWait(tout_sec);
        }


        //************************************************************************************************ protected
        protected byte[] GetSysInf() {

            link.TxCMD(CMD_SYS_INF);
            return link.RxData(64);
        }
        //************************************************************************************************ private

        void CheckStatus() {

            int resp = GetStatus();
            if (resp != 0) {
                throw new Exception("operation error: " + resp.ToString("X2"));
            }
        }

        bool IsServiceMode() {

            link.TxCMD(CMD_GET_MODE);
            byte resp = link.Rx8();

            if (resp == BMOD_MCU_SER) {
                return true;
            } else {
                return false;
            }
        }


        void DirMake(string path) {

            link.TxCMD(CMD_F_DIR_MK);
            link.TxString(path);

            int resp = GetStatus();
            if (resp != 0 && resp != 8)//ignore error 8 (already exist)
            {
                CheckStatus();
            }
        }

        void MakePath(string path, int mode) {

            int target_mode = (FA_WRITE | FS_MAKEPATH);

            if ((mode & target_mode) != target_mode) {
                return;
            }


            int sub_idx = 0;

            while (true) {

                sub_idx = path.IndexOf("/", sub_idx + 1);
                if (sub_idx < 0) {
                    return;
                }

                DirMake(path.Substring(0, sub_idx));
            }
        }

        void UpdExec_boot(int addr, int crc) {

            link.TxCMD(CMD_USB_RECOV);
            link.Tx32(ADDR_FLA_ICOR);
            link.Tx32(crc);

            int status = GetStatus(8000);

            if (status == 0x88) {
                throw new Exception("current core matches recovery copy");
            } else if (status != 0) {
                throw new Exception("recovery error: " + status.ToString("X2"));
            }
        }

        void UpdExec_app(int addr, int crc) {

            link.TxCMD(CMD_UPD_EXEC);
            link.Tx32(addr);
            link.Tx32(crc);
            link.Tx8(0);//exec

            BootWait(8);
        }

        void UpdExec(int addr, int crc) {

            if (IsServiceMode()) {
                UpdExec_boot(addr, crc);
            } else {
                UpdExec_app(addr, crc);
            }
        }
    }
}
