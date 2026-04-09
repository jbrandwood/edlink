using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace edlink {
    internal class Link {

        const byte STATUS_KEY = 0x5A;

        const byte CMD_STATUS = 0x10;
        const byte CMD_STATUS2 = 0x40;//only for mega and n8        

        SerialPort port;
        byte device_id = 0;
        byte protocol_id = 0;
        bool swap_endians = true;
        int ack_block_size = 1024;
        bool cfg_locked = false;
        string target_port = null;

        string[] port_blk = new string[0];

        public Link() {
        }

        public string PortName {
            get { return port.PortName; }
            set {
                if (cfg_locked) {
                    throw new Exception("port name cannot be changed");
                }
                target_port = value;
            }
        }
        public byte DeviceID {
            get { return device_id; }
            set {
                if (cfg_locked) {
                    throw new Exception("device id cannot be changed");
                }
                device_id = value;
            }
        }
        public byte ProtocolID {
            get { return protocol_id; }
            set {
                if (cfg_locked) {
                    throw new Exception("protocol id cannot be changed");
                }
                protocol_id = value;
            }
        }

        public bool SwapEndians {
            get { return swap_endians; }
            set { swap_endians = value; }
        }
        //************************************************************************************************
        public void Open() {

            try {
                TryOpen();
            } catch (Exception) {
                Thread.Sleep(200);
                TryOpen();
            }
        }
        public void TryOpen() {

            cfg_locked = true;

            string[] ports;// = SerialPort.GetPortNames();

            if (target_port != null) {
                ports = new string[] { target_port };
            } else {
                ports = SerialPort.GetPortNames();
            }

            for (int i = 0; i < ports.Length; i++) {

                try {
                    PortBlkCheck(ports[i]);
                    OpenConnection(ports[i]);
                    PortBlkInit(ports[i]);
                    return;
                } catch (Exception) { }
            }

            throw new Exception("EverDrive not found");
        }

        public void Close() {

            try {
                port.Close();
            } catch (Exception) { }
        }

        public void FlushPort() {
            port.ReadExisting();
        }

        public void txData(byte[] buff) {
            txData(buff, 0, buff.Length);
        }

        public void txData(byte[] buff, int offset, int len) {

            while (len > 0) {

                int block = Math.Min(len, 4096);

                if (block == 512) {
                    //512 does not work well by some reasons (mcu app load)
                    block = 256;
                }

                port.Write(buff, offset, block);
                len -= block;
                offset += block;
            }

            port.Write(new byte[0], 0, 0);
        }

        public void txData(string str) {

            port.Write(str);
            port.Write(new byte[0], 0, 0);
        }

        public void rxData(byte[] buff, int offset, int len) {

            for (int i = 0; i < len;) {
                i += port.Read(buff, offset + i, len - i);
            }
        }

        public void txDataACK(byte[] buff, int offset, int len) {

            while (len > 0) {

                int resp = rx8();

                if (resp != 0) {
                    throw new Exception("tx ack: " + resp.ToString("X2"));
                }

                int block = Math.Min(len, ack_block_size);

                txData(buff, offset, block);

                len -= block;
                offset += block;
            }
        }

        public void tx8(int arg) {

            byte[] buff = new byte[1];
            buff[0] = (byte)(arg);
            txData(buff, 0, buff.Length);
        }

        public byte rx8() {
            return (byte)port.ReadByte();
        }

        public void tx16(int arg) {

            byte[] buff = num16(arg);
            txData(buff, 0, buff.Length);
        }

        public UInt16 rx16() {

            byte[] buff = new byte[2];
            rxData(buff, 0, buff.Length);
            return num16(buff);
        }

        public byte[] rxData(int len) {
            byte[] buff = new byte[len];
            rxData(buff, 0, len);
            return buff;
        }

        public void tx32(int arg) {

            byte[] buff = num32(arg);
            txData(buff, 0, buff.Length);
        }

        public int rx32() {

            byte[] buff = new byte[4];
            rxData(buff, 0, buff.Length);
            return num32(buff);
        }

        public void txCMD(byte cmd_code) {
            byte[] cmd = new byte[4];
            cmd[0] = (byte)('+');
            cmd[1] = (byte)('+' ^ 0xff);
            cmd[2] = cmd_code;
            cmd[3] = (byte)(cmd_code ^ 0xff);
            txData(cmd);
        }

        public void txCMD(byte cmd_code, byte scmd) {

            byte[] cmd = new byte[5];
            cmd[0] = (byte)('+');
            cmd[1] = (byte)('+' ^ 0xff);
            cmd[2] = cmd_code;
            cmd[3] = (byte)(cmd_code ^ 0xff);
            cmd[4] = scmd;
            txData(cmd);
        }

        public int waitResp(int max_time_ms) {

            int old_tout = port.ReadTimeout;
            port.ReadTimeout = max_time_ms;
            int resp = port.ReadByte();
            port.ReadTimeout = old_tout;
            return resp;
        }
        //************************************************************************************************
        int num(byte[] val, int bytes) {

            int val_out = 0;

            for (int i = 0; i < bytes; i++) {

                val_out <<= 8;
                if (swap_endians) {
                    val_out |= val[i];
                } else {
                    val_out |= val[bytes - 1 - i];
                }
            }

            return val_out;
        }

        byte[] num(int val, int bytes) {

            byte[] val_out = new byte[bytes];

            for (int i = 0; i < bytes; i++) {

                if (swap_endians) {
                    val_out[bytes - 1 - i] = (byte)val;
                } else {
                    val_out[i] = (byte)val;
                }
                val >>= 8;
            }

            return val_out;
        }

        public byte[] num32(int val) {

            return num(val, 4);
        }
        public int num32(byte[] val) {

            return (int)num(val, 4);
        }

        public byte[] num16(int val) {

            return num(val, 2);
        }

        public UInt16 num16(byte[] val) {
            return (UInt16)num(val, 2);
        }

        void OpenConnection(string pname) {

            try {
                port = new SerialPort(pname);
                port.ReadTimeout = 200;
                port.WriteTimeout = 200;
                port.BaudRate = 921600;
                port.Open();
                txData(new byte[64 + 2]);
                FlushPort();
                getID();
                port.ReadTimeout = 2000;
                port.WriteTimeout = 2000;
                return;
            } catch (Exception) { }


            try {
                port.Close();
            } catch (Exception) { }

            port = null;

            throw new Exception("EverDrive not found");
        }

        void PortBlkCheck(string pname) {

            for (int i = 0; i < port_blk.Length; i++) {

                if (pname.Equals(port_blk[i])) {
                    //skip ports if they was in list during last seek
                    throw new Exception("Port in black list");
                }
            }
        }

        void PortBlkInit(string linked_port) {

            if (port_blk.Length != 0) {
                return;
            }

            string[] ports = SerialPort.GetPortNames();
            port_blk = new string[ports.Length];

            for (int i = 0; i < ports.Length; i++) {

                port_blk[i] = ports[i];
                if (linked_port.Equals(ports[i])) {
                    port_blk[i] += "-LINKED";
                }
            }
        }

        void getID() {

            byte protocol_id = this.protocol_id;
            byte device_id = this.device_id;

            txCMD(CMD_STATUS2);
            txCMD(CMD_STATUS);

            byte[] id = rxData(4);
            Thread.Sleep(5);
            FlushPort();

            if (protocol_id == 0) {
                protocol_id = id[1];
            }

            if (device_id == 0) {
                device_id = id[2];
            }

            if (id[0] != STATUS_KEY) {
                throw new Exception("Ivalid status key");
            }


            if (id[1] != protocol_id) {
                throw new Exception("Ivalid protocol id");
            }

            if (id[2] != device_id) {
                throw new Exception("Ivalid device id");
            }

            this.protocol_id = protocol_id;
            this.device_id = device_id;
        }

    }
}
