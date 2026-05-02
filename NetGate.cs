using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Edlink {
    class NetGate {

        Link link;
        DeviceIO dev;

        const byte CMD_TST = 0xA0;
        const byte CMD_TCP_OPEN = 0xA1;
        const byte CMD_TCP_CLOSE = 0xA2;
        const byte CMD_TCP_CLALL = 0xA3;
        const byte CMD_TCP_WR = 0xA4;
        const byte CMD_TCP_RD = 0xA5;
        const byte CMD_TCP_RDA = 0xA6;
        const byte CMD_TCP_AVB = 0xA7;

        const byte RSP_OK = 0xB0;
        const byte RSP_ERR = 0xB1;

        TcpClient[] tcp_clients = new TcpClient[256];
        NetworkStream[] net_stream = new NetworkStream[256];
        string[] tcp_hosts = new string[256];

        bool link_act = true;

        public NetGate(DeviceIO io) {

            dev = io;
            link = io.Link;
        }

        public void Start() {

            Console.WriteLine("NetGate bridge started!");

            while (link_act) {

                if (link.BytesToRead < 1) {
                    continue;
                }

                if (link.Rx8() != '-') {
                    continue;
                }

                if (link.Rx8() != ('-' ^ 0xff)) {
                    continue;
                }

                byte cmd = link.Rx8();

                if (link.Rx8() != (cmd ^ 0xff)) {
                    continue;
                }

                switch (cmd) {
                    case CMD_TST:
                        TxByte(RSP_OK);
                        break;
                    case CMD_TCP_OPEN:
                        Cmd_open();
                        break;
                    case CMD_TCP_CLOSE:
                        Cmd_close();
                        break;
                    case CMD_TCP_CLALL:
                        Cmd_closeAll();
                        break;
                    case CMD_TCP_WR:
                        Cmd_WR();
                        break;
                    case CMD_TCP_RD:
                        Cmd_RD();
                        break;
                    case CMD_TCP_RDA:
                        Cmd_RDA();
                        break;
                    case CMD_TCP_AVB:
                        Cmd_avb();
                        break;

                }
            }

        }
        public void Stop() {
            link_act = false;
        }

        void TxByte(int val) {

            dev.FifoWR(new byte[] { (byte)val }, 0, 1);
        }

        void Tx16(int val) {

            byte[] buff = link.Num16(val);
            dev.FifoWR(buff, 0, buff.Length);
        }

        void Tx32(int val) {

            byte[] buff = link.Num32(val);
            dev.FifoWR(buff, 0, buff.Length);
        }

        void Cmd_open() {

            string url = link.RxString();
            Console.WriteLine("open connection to " + url);


            Uri uri = null;

            try {
                uri = new Uri(url);
            } catch (Exception) {
                Console.WriteLine("Error: invalid URL");
                Console.WriteLine("Expected format: tcp://host:port");
                return;
            }

            for (int i = 0; i < tcp_clients.Length; i++) {

                if (tcp_clients[i] != null) {
                    continue;
                }

                try {
                    tcp_hosts[i] = url;
                    tcp_clients[i] = new TcpClient(uri.Host, uri.Port);
                    net_stream[i] = tcp_clients[i].GetStream();
                    dev.FifoWR(new byte[] { RSP_OK, (byte)i }, 0, 2);
                    return;

                } catch (Exception x) {
                    Console.WriteLine("connection open error: " + x.Message);
                    TxByte(RSP_ERR);
                    return;
                }

            }

            Console.WriteLine("connection open error: no free slots");
            TxByte(RSP_ERR);
        }


        void Cmd_closeAll() {

            for (int i = 0; i < tcp_clients.Length; i++) {
                Cmd_close(i);
            }
        }
        void Cmd_close() {

            byte con_idx = link.Rx8();
            Cmd_close(con_idx);
        }
        void Cmd_close(int con_idx) {

            if (tcp_clients[con_idx] == null) {
                return;
            }

            try {
                Console.WriteLine("close connection with " + tcp_hosts[con_idx]);
                tcp_clients[con_idx].Close();
                tcp_clients[con_idx] = null;
            } catch (Exception x) {
                Console.WriteLine("connection close error: " + x.Message);
            }
        }


        void Cmd_WR() {

            byte con_idx = link.Rx8();
            int len = link.Rx16();

            byte[] buff = link.RxData(len);
            net_stream[con_idx].Write(buff, 0, buff.Length);
        }


        void Cmd_RD() {

            byte con_idx = link.Rx8();
            int len = link.Rx16();

            byte[] buff = new byte[len];

            for (int i = 0; i < len;) {
                int rdx = net_stream[con_idx].Read(buff, i, len - i);
                dev.FifoWR(buff, i, rdx);
                i += rdx;
            }
        }

        void Cmd_RDA() {

            //Do not read more than 2048 bytes at once (FIFO limit).
            byte con_idx = link.Rx8();
            int len = link.Rx16();

            byte[] buff = new byte[len];

            len = Math.Min(len, tcp_clients[con_idx].Available);

            len = net_stream[con_idx].Read(buff, 0, len);

            Tx16(len);
            dev.FifoWR(buff, 0, len);
        }

        void Cmd_avb() {

            //byte con_idx = link.Rx8();
            //TxByte(net_stream[con_idx].DataAvailable ? 1 : 0);
            byte con_idx = link.Rx8();
            Tx32(tcp_clients[con_idx].Available);
        }

    }
}
