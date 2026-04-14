using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edlink.EDMEGA {
    internal class DeviceIO : IDeviceIO {

        const byte STATUS_KEY = 0x5A;
        public const int PROTOCOL_ID = 0x05;

        public const int DEV_ID_MEGA_PRO = 0x18;
        public const int DEV_ID_MEGA_CORE = 0x25;
        //************************************************************************************************ commands
        const byte CMD_STATUS = 0x10;
        const byte CMD_GET_MODE = 0x11;
        const byte CMD_IO_RST = 0x12;
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
        const byte CMD_REINIT = 0x25;
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

        //************************************************* FS flags
        public const byte FA_READ = 0x01;
        public const byte FA_WRITE = 0x02;
        public const byte FA_OPEN_EXISTING = 0x00;
        public const byte FA_CREATE_NEW = 0x04;
        public const byte FA_CREATE_ALWAYS = 0x08;
        public const byte FA_OPEN_ALWAYS = 0x10;
        public const byte FA_OPEN_APPEND = 0x30;
        public const byte FS_MAKEPATH = 0x80; //make path if not exists
        //************************************************* 

        Link link;
        //************************************************************************************************ public
        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = true;
        }

        public Link Link {
            get { return link; }
        }
        public void ExitServiceMode() {

            if (!IsServiceMode()) {
                return;
            }

            link.txCMD(CMD_RUN_APP);

            BootWait();
            if (IsServiceMode()) {
                throw new Exception("failed to exit service mode");
            }
        }

        public void EnterServiceMode() {

            if (IsServiceMode()) {
                return;
            }

            link.txCMD(CMD_IO_RST);
            link.tx8(0);

            BootWait();

            if (!IsServiceMode()) {
                throw new Exception("failed to enter service mode");
            }
        }

        public void MemWR(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }

            link.txCMD(CMD_MEM_WR);
            link.tx32(addr);
            link.tx32(len);
            link.tx8(0);//exec
            link.txData(buff, offset, len);
        }
        public void MemRD(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }

            link.txCMD(CMD_MEM_RD);
            link.tx32(addr);
            link.tx32(len);
            link.tx8(0);//exec
            link.rxData(buff, offset, len);
        }
        public void FlaWR(int addr, byte[] buff, int offset, int len) {

            link.txCMD(CMD_FLA_WR);
            link.tx32(addr);
            link.tx32(len);
            link.txDataACK(buff, offset, len);
            CheckStatus();
        }
        public void FlaRD(int addr, byte[] buff, int offset, int len) {

            link.txCMD(CMD_FLA_RD);
            link.tx32(addr);
            link.tx32(len);
            link.rxData(buff, offset, len);
        }
        public void FpgInit(byte[] data) {

            link.txCMD(CMD_FPG_USB);
            link.tx32(data.Length);
            link.txDataACK(data, 0, data.Length);
            CheckStatus();
        }
        public void FileOpen(string path, int mode) {

            MakePath(path, mode);

            link.txCMD(CMD_F_FOPN);
            link.tx8(mode & ~FS_MAKEPATH);
            link.txString(path);
            CheckStatus();
        }
        public void FileClose() {

            link.txCMD(CMD_F_FCLOSE);
            CheckStatus();
        }
        public UInt64 FileAvailable() {

            link.txCMD(CMD_F_AVB);

            UInt64 hi = (UInt64)link.rx32();
            UInt64 lo = (UInt64)link.rx32();

            return lo | (hi << 32);
        }
        public void FileRead(byte[] buff, int offset, int len) {

            link.txCMD(CMD_F_FRD);
            link.tx32(len);

            while (len > 0) {

                int block = 4096;
                if (block > len) block = len;
                int resp = link.rx8();
                if (resp != 0) {
                    throw new Exception("file read error: " + resp.ToString("X2"));
                }

                link.rxData(buff, offset, block);
                offset += block;
                len -= block;

            }
        }
        public void FileWrite(byte[] buff, int offset, int len) {

            link.txCMD(CMD_F_FWR);
            link.tx32(len);
            link.txDataACK(buff, offset, len);
            CheckStatus();
        }
        public void RtcSet(DateTime dt) {
        }
        public int RtcCal(DateTime dt, byte arg) {

            RtcTime rtc = new RtcTime(dt);
            byte[] vals = rtc.getVals();

            link.txCMD(CMD_RTC_CAL);
            link.txData(vals, 0, 6);
            link.tx8(arg);

            return link.rx32();
        }
        public void RtcCalSet(int ppm_val) {


        }

        //************************************************************************************************ internal
        //************************************************************************************************ private
        byte[] GetID() {

            link.txCMD(CMD_STATUS2);
            return link.rxData(4);
        }

        int GetStatus() {

            byte[] resp = GetID();

            if (resp[0] != STATUS_KEY || resp[1] != PROTOCOL_ID) {
                throw new Exception("unexpected status response (" + BitConverter.ToString(resp) + ")");
            }
            return resp[3];
        }

        void CheckStatus() {

            int resp = GetStatus();
            if (resp != 0) {
                throw new Exception("operation error: " + resp.ToString("X2"));
            }
        }

        bool IsServiceMode() {

            link.txCMD(CMD_GET_MODE);
            byte resp = link.rx8();

            if (resp == 0xA1) {
                return true;
            } else {
                return false;
            }
        }

        void BootWait() {
            BootWait(5);
        }

        void BootWait(int max_time_sec) {

            var sw = Stopwatch.StartNew();

            Thread.Sleep(100);

            while (true) {

                try {
                    link.Close();
                } catch (Exception) { }


                try {
                    Thread.Sleep(100);
                    link.Open();
                    return;
                } catch (Exception) { }


                if (sw.ElapsedMilliseconds > max_time_sec * 1000) {
                    throw new Exception("boot timeout");
                }
            }
        }


        void dirMake(string path) {

            link.txCMD(CMD_F_DIR_MK);
            link.txString(path);

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

                dirMake(path.Substring(0, sub_idx));
            }
        }
    }
}
