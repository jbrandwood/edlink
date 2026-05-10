using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;


namespace Edlink.Device {
    internal class Link {

        const byte STATUS_KEY = 0x5A;
        const byte STATUS_KEY_OLD = 0xA5;

        const byte DEVICE_ID_MEGA_PRO = 0x18;
        const byte DEVICE_ID_N8_PRO = 0x17;
        const byte PROTOCOL_ID_MEGA = 0x05;
        const byte PROTOCOL_ID_N8 = 0x06;


        const byte CMD_STATUS = 0x10;
        const byte CMD_STATUS2 = 0x40;//only for mega and n8  


        enum Protocol {
            Unknown,
            Gen1,//old n8 and mega firmware
            Gen2,//new n8 and mega firmware
            Gen3,//everything else
        }

        struct DeviceConfig {
            public Protocol ProtocolGen;
            public byte ProtocolId;
            public byte DeviceId;
        }

        SerialPort port;
        bool swap_endians = true;
        int ack_block_size = 1024;
        bool cfg_locked = false;
        string target_port = null;

        bool cold_start = true;

        string[] port_blk = new string[0];


        DeviceConfig dcfg;


        public Link() {

            dcfg.ProtocolGen = Protocol.Unknown;
            dcfg.ProtocolId = 0;
            dcfg.DeviceId = 0;
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

            get { return dcfg.DeviceId; }
            set {
                if (cfg_locked) {
                    throw new Exception("device id cannot be changed");
                }
                dcfg.DeviceId = value;
            }
        }
        public byte ProtocolID {

            get { return dcfg.ProtocolId; }
            set {
                if (cfg_locked) {
                    throw new Exception("protocol id cannot be changed");
                }
                dcfg.ProtocolId = value;
            }
        }

        public bool SwapEndians {

            get { return swap_endians; }
            set { swap_endians = value; }
        }

        public int BytesToRead {

            get { return port.BytesToRead; }
        }
        //************************************************************************************************
        public void Open() {

            try {

                TryOpen();

            } catch (Exception x) {

                if (cold_start) {
                    Thread.Sleep(100);
                    TryOpen();
                } else {
                    throw x;
                }
            }

            cold_start = false;
        }
        public void TryOpen() {

            cfg_locked = true;

            string[] ports;// = SerialPort.GetPortNames();

            if (target_port != null) {
                ports = new string[] { target_port };
            } else {
                ports = GetPorts();
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

            //port.ReadExisting(); //(do not work well in linux?)
            byte[] buff = new byte[port.BytesToRead];
            while (buff.Length > 0) {
                port.Read(buff, 0, buff.Length);
                buff = new byte[port.BytesToRead];
            }
        }

        public void TxData(byte[] buff) {
            TxData(buff, 0, buff.Length);
        }


        public void TxData(byte[] buff, int offset, int len) {

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

        public void TxData(string str) {

            port.Write(str);
            port.Write(new byte[0], 0, 0);
        }

        public void RxData(byte[] buff, int offset, int len) {

            for (int i = 0; i < len;) {
                i += port.Read(buff, offset + i, len - i);
            }
        }

        public void TxDataACK(byte[] buff, int offset, int len) {

            while (len > 0) {

                int resp = Rx8();

                if (resp != 0) {
                    throw new Exception("tx ack: " + resp.ToString("X2"));
                }

                int block = Math.Min(len, ack_block_size);

                TxData(buff, offset, block);

                len -= block;
                offset += block;
            }
        }

        public void Tx8(int arg) {

            byte[] buff = new byte[1];
            buff[0] = (byte)(arg);
            TxData(buff, 0, buff.Length);
        }

        public byte Rx8() {
            return (byte)port.ReadByte();
        }

        public void Tx16(int arg) {

            byte[] buff = Num16(arg);
            TxData(buff, 0, buff.Length);
        }

        public UInt16 Tx16() {

            byte[] buff = new byte[2];
            RxData(buff, 0, buff.Length);
            return Num16(buff);
        }

        public int Rx16() {

            byte[] buff = new byte[2];
            RxData(buff, 0, buff.Length);
            return Num16(buff);
        }

        public byte[] RxData(int len) {
            byte[] buff = new byte[len];
            RxData(buff, 0, len);
            return buff;
        }

        public void Tx32(int arg) {

            byte[] buff = Num32(arg);
            TxData(buff, 0, buff.Length);
        }

        public int Rx32() {

            byte[] buff = new byte[4];
            RxData(buff, 0, buff.Length);
            return Num32(buff);
        }

        public void TxString(string str) {

            Tx16(str.Length);
            TxData(str);
        }

        public string RxString() {

            int len = Rx16();
            byte[] buff = new byte[len];
            RxData(buff, 0, buff.Length);
            return System.Text.Encoding.UTF8.GetString(buff);
        }

        public void TxCMD(byte cmd_code) {

            byte[] cmd = new byte[4];
            cmd[0] = (byte)('+');
            cmd[1] = (byte)('+' ^ 0xff);
            cmd[2] = cmd_code;
            cmd[3] = (byte)(cmd_code ^ 0xff);
            TxData(cmd);
        }

        public void TxCMD(byte cmd_code, byte scmd) {

            byte[] cmd = new byte[5];
            cmd[0] = (byte)('+');
            cmd[1] = (byte)('+' ^ 0xff);
            cmd[2] = cmd_code;
            cmd[3] = (byte)(cmd_code ^ 0xff);
            cmd[4] = scmd;
            TxData(cmd);
        }

        public int WaitResp(int max_time_ms) {

            int old_tout = port.ReadTimeout;
            port.ReadTimeout = max_time_ms;
            int resp = port.ReadByte();
            port.ReadTimeout = old_tout;
            return resp;
        }

        public static bool IsDevPath(string path) {

            if (path.ToLower().StartsWith("sd:")) {
                return true;
            } else {
                return false;
            }
        }

        public static string MakeDevPath(string path) {

            if (IsDevPath(path)) {
                return path;
            } else {
                if (path.StartsWith("/")) {
                    path = path.Substring(1);
                }
                return "sd:" + path;
            }
        }

        public static string GetDevPath(string path) {

            if (IsDevPath(path)) {
                return path.Substring(3);
            } else {
                return path;
            }
        }
        //************************************************************************************************
        int Num(byte[] val, int bytes) {

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

        byte[] Num(int val, int bytes) {

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


        public byte[] Num32(int val) {

            return Num(val, 4);
        }

        public int Num32(byte[] val, int offset) {

            byte[] buff = new byte[4];
            Array.Copy(val, offset, buff, 0, buff.Length);
            return Num32(buff);
        }

        public int Num32(byte[] val) {

            return (int)Num(val, 4);
        }

        public byte[] Num16(int val) {

            return Num(val, 2);
        }

        public UInt16 Num16(byte[] val, int offset) {

            byte[] buff = new byte[2];
            Array.Copy(val, offset, buff, 0, buff.Length);
            return Num16(buff);
        }

        public UInt16 Num16(byte[] val) {
            return (UInt16)Num(val, 2);
        }

        void OpenConnection(string pname) {

            try {
                port = new SerialPort(pname);
                port.ReadTimeout = 200;
                port.WriteTimeout = 200;
                port.BaudRate = 921600;
                port.Open();
                TxData(new byte[64 + 2]);
                FlushPort();
                GetID();
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

        public byte[] GetID() {

            return GetID(0);
        }

        public byte[] GetID(int timeout_ms) {

            byte[] id = new byte[4];
            DeviceConfig cfg;

            if (dcfg.ProtocolGen == Protocol.Unknown) {
                cfg = GetDeviceConfig();
            } else {
                cfg = dcfg;
            }

            TxCMD(CMD_STATUS);

            if (timeout_ms != 0) {

                var sw = Stopwatch.StartNew();

                while (BytesToRead < 2) {
                    if (sw.ElapsedMilliseconds > timeout_ms) {
                        throw new Exception("link status timeout");
                    }
                }
            }

            if (cfg.ProtocolGen == Protocol.Gen3) {
                RxData(id, 0, id.Length);
            } else {

                //transform legacy status resp to Gen3
                if (cfg.ProtocolId == PROTOCOL_ID_N8) {
                    id[3] = Rx8();
                    id[0] = Rx8();
                } else {
                    id[0] = Rx8();
                    id[3] = Rx8();
                }

                if (id[0] != STATUS_KEY_OLD) {
                    throw new Exception("Ivalid status key");
                }

                id[0] = STATUS_KEY;
                id[1] = cfg.ProtocolId;
                id[2] = cfg.DeviceId;
            }

            byte target_protocol = dcfg.ProtocolId == 0 ? cfg.ProtocolId : dcfg.ProtocolId;
            byte target_device = dcfg.DeviceId == 0 ? cfg.DeviceId : dcfg.DeviceId;


            if (id[0] != STATUS_KEY) {
                throw new Exception("Ivalid status key");
            }

            if (id[1] != target_protocol) {
                throw new Exception("Ivalid protocol id");
            }

            if (id[2] != target_device) {
                throw new Exception("Ivalid device id");
            }

            if (dcfg.ProtocolGen == Protocol.Unknown) {
                dcfg = cfg;
            }

            return id;
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

            string[] ports = GetPorts();

            port_blk = new string[ports.Length];

            for (int i = 0; i < ports.Length; i++) {

                port_blk[i] = ports[i];
                if (linked_port.Equals(ports[i])) {
                    port_blk[i] += "-LINKED";
                }
            }
        }

        string[] GetPorts() {

            string[] ports = SerialPort.GetPortNames();
            string[] unique = ports.Distinct().ToArray();

            return unique;
        }

        DeviceConfig GetDeviceConfig() {

            DeviceConfig cfg;

            byte[] id = new byte[4];

            TxCMD(CMD_STATUS2);
            TxCMD(CMD_STATUS);

            RxData(id, 0, 2);


            if (id[0] == STATUS_KEY) {

                //new status Cmd. not supported by old firmware (and bootladers)
                RxData(id, 2, 2);//remain CMD_STATUS2 status bytes

                if (id[1] == PROTOCOL_ID_MEGA || id[1] == PROTOCOL_ID_N8) {
                    RxData(2);//remain CMD_STATUS status bytes
                    cfg.ProtocolGen = Protocol.Gen2;
                } else {
                    cfg.ProtocolGen = Protocol.Gen3;
                }

                cfg.ProtocolId = id[1];
                cfg.DeviceId = id[2];

            } else
             if (id[0] == STATUS_KEY_OLD) {
                //legacy status Cmd. early MEGA
                cfg.ProtocolGen = Protocol.Gen1;
                cfg.ProtocolId = PROTOCOL_ID_MEGA;
                cfg.DeviceId = DEVICE_ID_MEGA_PRO;
            } else
            if (id[1] == STATUS_KEY_OLD) {
                //legacy status Cmd. early N8
                cfg.ProtocolGen = Protocol.Gen1;
                cfg.ProtocolId = PROTOCOL_ID_N8;
                cfg.DeviceId = DEVICE_ID_N8_PRO;
            } else {
                throw new Exception("unexpected status key (" + id[0].ToString("X2") + ")");
            }


            return cfg;
        }

    }
}
