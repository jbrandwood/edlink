using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edlink.ED64 {

    internal class DeviceIO : edlink.IDeviceIO {

        const byte STATUS_KEY = 0x5A;
        public const int PROTOCOL_ID = 0x07;
        public const int DEV_ID_ED64_PRO = 0x27;


        const byte CMD_STATUS = 0x10;
        const byte CMD_GET_MODE = 0x11;
        const byte CMD_IO_RST = 0x12;
        const byte CMD_NRESP = 0x13;

        const byte CMD_EPO = 0x81;
        const byte EPO_SCMD_XFER = 0x10;
        const byte EPO_SCMD_WRE = 0x11;

        const byte CMD_SYS = 0x84;
        const byte SYS_SCMD_FPG_INIT = 0x12;
        const byte SYS_SCMD_BOOT_UPD = 0x14;

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


        public const int ADDR_FCI_SYS = 0x10000000;               //system registers
        public const int ADDR_FCI_FIFO = (ADDR_FCI_SYS + 0x10000);    //mcu fifo
        public const int ADDR_FCI_MREQ = (ADDR_FCI_SYS + 0x30000);    //mcu fifo

        Link link;
        MenuCmd mcmd;

        public DeviceIO(Link link) {

            if (link.ProtocolID != PROTOCOL_ID) {
                throw new NotSupportedException("Invalid protocol id: 0x" + link.ProtocolID.ToString("X02"));
            }

            this.link = link;
            link.SwapEndians = true;

            mcmd = new MenuCmd(this);
        }

        public string DeviceName {

            get {

                switch (link.DeviceID) {

                    case DeviceIO.DEV_ID_ED64_PRO:
                        return "EverDrive-64 PRO";
                    default:
                        return "Uknown EverDrive-64";
                }
            }
        }

        public void Stop() {
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

        public void Reset() {
            mcmd.Test();
            mcmd.RunPreloaded("", MenuCmd.MODE_MENU);
        }

        public void Run(string rom_path, string fpga_path) {

            mcmd.Test();
            mcmd.Run(rom_path);
        }

        public void FpgaInit(string path) {

            byte[] fpga = File.ReadAllBytes(path);

            fpgInit(fpga);
        }
        //************************************************************************************************ internal
        internal Link Link {
            get { return link; }
        }
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

        void fpgInit(byte[] data) {

            bool header = false;
            link.txCMD(CMD_SYS, SYS_SCMD_FPG_INIT);
            link.tx32(header ? 0 : data.Length);
            link.tx8(EPO_LINK_ACK);
            link.txDataACK(data, 0, data.Length);
            checkStatus();
        }
    }
}
