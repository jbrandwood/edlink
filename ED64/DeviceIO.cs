using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Edlink.Device;

namespace Edlink.ED64 {

    internal class DeviceIO : IDeviceIO {

        const byte STATUS_KEY = 0x5A;
        public const int PROTOCOL_ID = 0x07;
        public const int DEV_ID_ED64_PRO = 0x27;

        //*************************************************
        const byte CMD_STATUS = 0x10;
        const byte CMD_GET_MODE = 0x11;
        const byte CMD_IO_RST = 0x12;
        const byte CMD_NRESP = 0x13;

        const byte CMD_FS = 0x80;
        const byte FS_SCMD_INIT = 0x10;
        const byte FS_SCMD_DIR_OPN = 0x11;
        const byte FS_SCMD_DIR_RD = 0x12;
        const byte FS_SCMD_DIR_LD = 0x13;
        const byte FS_SCMD_DIR_SIZE = 0x14;
        const byte FS_SCMD_DIR_PATH = 0x15;
        const byte FS_SCMD_DIR_GET = 0x16;
        const byte FS_SCMD_FOPN = 0x17;
        const byte FS_SCMD_FCLOSE = 0x18;
        const byte FS_SCMD_FPTR = 0x19;
        const byte FS_SCMD_FINFO = 0x1A;
        const byte FS_SCMD_FCRC = 0x1B;
        const byte FS_SCMD_DIR_MK = 0x1C;
        const byte FS_SCMD_DEL = 0x1D;
        const byte FS_SCMD_SEEK_IDX = 0x1E;
        const byte FS_SCMD_AVB = 0x1F;
        const byte FS_SCMD_FCP = 0x20;
        const byte FS_SCMD_SEEK_PAT = 0x21; //seek data pattern
        const byte FS_SCMD_DTEST = 0x22; //check if dir exists
        const byte FS_SCMD_FTEST = 0x23; //check if file exists

        const byte CMD_EPO = 0x81;
        const byte EPO_SCMD_XFER = 0x10;
        const byte EPO_SCMD_WRE = 0x11;

        const byte CMD_RTC = 0x83;
        const byte RTC_SCMD_GET = 0x10;
        const byte RTC_SCMD_SET = 0x11;
        const byte RTC_SCMD_CAL = 0x12;
        const byte RTC_SCMD_CALSET = 0x13;

        const byte CMD_SYS = 0x84;
        const byte SYS_SCMD_FPG_INIT = 0x12;
        const byte SYS_SCMD_BOOT_UPD = 0x14;

        const byte CMD_BOOT = 0xF0;
        const byte BOOT_SCMD_APP_MODE = 0x10;
        const byte BOOT_SCMD_LOAD_APP = 0x11;

        //*************************************************
        const byte EPO_WER_SRC = 0x01;
        const byte EPO_WER_DST = 0x02;

        const byte EPO_LINK = 0x10;//link (usb or console)
        const byte EPO_LINK_ACK = 0x11;//link (usb or console)
        const byte EPO_LINK_FS = 0x12;//file system
        const byte EPO_FS = 0x12; //file
        const byte EPO_FCI = 0x13; //ed mem
        const byte EPO_FLA = 0x14; //mcu flash
        const byte EPO_EFU = 0x15; //efu in flash
        const byte EPO_USB = 0x18; //efu in flash

        public const byte FA_READ = 0x01;
        public const byte FA_WRITE = 0x02;
        public const byte FA_OPEN_EXISTING = 0x00;
        public const byte FA_CREATE_NEW = 0x04;
        public const byte FA_CREATE_ALWAYS = 0x08;
        public const byte FA_OPEN_ALWAYS = 0x10;
        public const byte FA_OPEN_APPEND = 0x30;
        public const byte FS_MAKEPATH = 0x80; //make path if not exists

        public const int BMOD_MCU_APP = 0xA0;//app mode
        public const int BMOD_MCU_SER = 0xA1;//service mode

        public const int ADDR_FCI_SYS = 0x10000000;               //system registers
        public const int ADDR_FCI_FIFO = (ADDR_FCI_SYS + 0x10000);    //mcu fifo
        public const int ADDR_FCI_MREQ = (ADDR_FCI_SYS + 0x30000);    //mcu fifo

        Link link;

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

        public void exitServiceMode() {

            if (!isServiceMode()) {
                return;
            }

            link.txCMD(CMD_BOOT, BOOT_SCMD_APP_MODE);

            bootWait();
            if (isServiceMode()) {
                throw new Exception("failed to exit service mode");
            }
        }

        public void enterServiceMode() {

            if (isServiceMode()) {
                return;
            }

            link.txCMD(CMD_IO_RST);
            link.tx8(BMOD_MCU_SER);

            bootWait();

            if (!isServiceMode()) {
                throw new Exception("failed to enter service mode");
            }
        }

        public void MemWR(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }
            epoWR(EPO_FCI, addr, buff, offset, len);
        }

        public void MemRD(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;

            }
            epoRD(EPO_FCI, addr, buff, offset, len);
        }

        public void FlaRD(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }
            epoRD(EPO_FLA, addr, buff, offset, len);
        }

        public void FlaWR(int addr, byte[] buff, int offset, int len) {

            if (len == 0) {
                return;
            }
            epoWre();
            epoWR(EPO_FLA, addr, buff, offset, len);
        }

        public void fpgInit(byte[] data) {

            bool header = false;
            link.txCMD(CMD_SYS, SYS_SCMD_FPG_INIT);
            link.tx32(header ? 0 : data.Length);
            link.tx8(EPO_LINK_ACK);
            link.txDataACK(data, 0, data.Length);
            checkStatus();
        }

        public void fileOpen(string path, int mode) {

            link.txCMD(CMD_FS, FS_SCMD_FOPN);
            link.tx8(mode);
            link.txString(path);
            checkStatus();
        }
        public void fileClose() {

            link.txCMD(CMD_FS, FS_SCMD_FCLOSE);
            checkStatus();
        }

        public UInt64 fileAvailable() {

            link.txCMD(CMD_FS, FS_SCMD_AVB);

            UInt64 lo = (UInt64)link.rx32();
            UInt64 hi = (UInt64)link.rx32();

            return lo | (hi << 32);
        }

        public void fileRead(byte[] buff, int offset, int len) {

            epoRD(EPO_FS, 0, buff, offset, len);
        }

        public void fileWrite(byte[] buff, int offset, int len) {

            epoWR(EPO_FS, 0, buff, offset, len);
        }

        public void rtcSet(DateTime dt) {

            RtcTime rtc = new RtcTime(dt);
            byte[] vals = rtc.getVals();
            link.txCMD(CMD_RTC, RTC_SCMD_SET);
            link.txData(vals, 0, 8);
        }

        public int RtcCal(DateTime dt, byte arg) {

            RtcTime rtc = new RtcTime(dt);
            byte[] vals = rtc.getVals();

            link.txCMD(CMD_RTC, RTC_SCMD_CAL);
            link.txData(vals);
            link.tx8(arg);

            return link.rx32();
        }

        public void RtcCalSet(int ppm_val) {

            link.txCMD(CMD_RTC, RTC_SCMD_CALSET);
            link.tx32(ppm_val);
            checkStatus();
        }

        public void McuAppLoad(byte[] data) {

            link.txCMD(CMD_BOOT, BOOT_SCMD_LOAD_APP);
            txApp(data);

            bootWait(2);
            checkStatus();
        }

        public void McuBootInstall(byte[] data) {

            link.txCMD(CMD_SYS, SYS_SCMD_BOOT_UPD);
            txApp(data);

            bootWait(2);
            checkStatus();
        }
        //************************************************************************************************ internal
        internal void fifoWR(string str) {

            byte[] bytes = Encoding.ASCII.GetBytes(str);
            fifoWR(bytes, 0, bytes.Length);
        }

        internal void fifoWR(byte[] data, int offset, int len) {

            MemWR(ADDR_FCI_FIFO, data, offset, len);
        }

        internal void fifoTxString(string str) {

            byte[] bytes = Encoding.ASCII.GetBytes(str);
            byte[] len = link.num16(bytes.Length);
            fifoWR(len, 0, 2);
            fifoWR(bytes, 0, bytes.Length);
        }
        //************************************************************************************************ private
        byte[] getID() {

            link.txCMD(CMD_STATUS);
            return link.rxData(4);
        }

        int getNresp(int resp) {
            link.txCMD(CMD_NRESP);
            link.tx8(resp);
            return link.rx8();
        }
        int getStatus() {

            byte[] resp = getID();

            if (resp[0] != STATUS_KEY || resp[1] != PROTOCOL_ID) {
                throw new Exception("unexpected status response (" + BitConverter.ToString(resp) + ")");
            }
            return resp[3];
        }

        void checkStatus() {

            int resp = getStatus();
            if (resp != 0) {
                int nresp = getNresp(resp);
                throw new Exception("operation error: " + resp.ToString("X2") + "." + nresp.ToString("X2"));
            }
        }

        bool isServiceMode() {

            link.txCMD(CMD_GET_MODE);
            byte resp = link.rx8();

            if (resp == BMOD_MCU_SER) {
                return true;
            } else {
                return false;
            }
        }


        void epoWre() {

            link.txCMD(CMD_EPO, EPO_SCMD_WRE);
            link.tx32(0x27101983);//flash wr protection magic number
        }

        void epoCmd(byte ep_src, byte ep_dst, int addr_src, int addr_dst, int len) {

            link.txCMD(CMD_EPO, EPO_SCMD_XFER);
            link.tx32(addr_src);
            link.tx32(addr_dst);
            link.tx32(len);
            link.tx8(ep_src);
            link.tx8(ep_dst);
            link.tx16(0);//reserved(for aligment)

            link.tx8(0);//ack
        }

        void epoRD(byte epo_src, int addr, byte[] buff, int offset, int len) {

            epoCmd(epo_src, EPO_LINK, addr, 0, len);
            link.rxData(buff, offset, len);
            checkStatus();
        }

        void epoWR(byte epo_dst, int addr, byte[] buff, int offset, int len) {

            byte epo_src = EPO_LINK; ;

            if (epo_dst == EPO_FLA || epo_dst == EPO_FS) {
                epo_src = EPO_LINK_ACK;
            }

            epoCmd(epo_src, epo_dst, 0, addr, len);

            if (epo_src == EPO_LINK_ACK) {
                link.txDataACK(buff, offset, len);
            } else {
                link.txData(buff, offset, len);
            }

            checkStatus();
        }

        void bootWait() {
            bootWait(5);
        }

        void bootWait(int max_time_sec) {

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

        void txApp(byte[] data) {

            link.tx32(data.Length);
            link.txDataACK(data, 0, 512);
            link.txDataACK(data, 512, data.Length - 512);
        }

    }
}
