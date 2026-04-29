using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Edlink {
    class NetGate {

        static Link link;
        static DeviceIO dev;

        const byte CMD_TST = 0xA0;
        const byte CMD_TCP_OPEN = 0xA1;
        const byte CMD_TCP_CLOSE = 0xA2;
        const byte CMD_TCP_CLALL = 0xA3;
        const byte CMD_TCP_RD = 0xA4;
        const byte CMD_TCP_WR = 0xA5;
        const byte CMD_TCP_CANRD = 0xA6;

        const byte RSP_OK = 0xB0;
        const byte RSP_ERR = 0xB1;

        static TcpClient[] tcp_clients = new TcpClient[256];
        static NetworkStream[] net_stream = new NetworkStream[256];
        static string[] tcp_hosts = new string[256];

        public static void Start(DeviceIO io) {

            bool link_act = true;
            dev = io;
            link = io.Link;

            Console.WriteLine("Enter to NetGate mode...");

            while (link_act) {

                if (link.BytesToRead < 1) {
                    continue;
                }

                byte cmd = link.Rx8();

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
                    case CMD_TCP_CANRD:
                        Cmd_canRD();
                        break;
                    case CMD_TCP_RD:
                        Cmd_RD();
                        break;
                    case CMD_TCP_WR:
                        Cmd_WR();
                        break;
                }
            }

        }

        static void TxByte(int val) {

            dev.FifoWR(new byte[] { (byte)val }, 0, 1);
        }


        static void Cmd_open() {

            int port = link.Rx32();
            string host = link.RxString();

            Console.WriteLine("open connection to " + host + ":" + port);

            for (int i = 0; i < tcp_clients.Length; i++) {

                if (tcp_clients[i] != null) {
                    continue;
                }

                try {
                    tcp_hosts[i] = host + ":" + port;
                    tcp_clients[i] = new TcpClient(host, port);
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


        static void Cmd_closeAll() {

            for (int i = 0; i < tcp_clients.Length; i++) {
                Cmd_close(i);
            }
        }
        static void Cmd_close() {

            byte con_idx = link.Rx8();
            Cmd_close(con_idx);
        }
        static void Cmd_close(int con_idx) {

            if (tcp_clients[con_idx] == null) return;

            try {
                Console.WriteLine("close connection with " + tcp_hosts[con_idx]);
                tcp_clients[con_idx].Close();
                net_stream[con_idx] = null;
            } catch (Exception x) {
                Console.WriteLine("connection close error: " + x.Message);
            }
        }

        static void Cmd_canRD() {

            byte con_idx = link.Rx8();
            TxByte(net_stream[con_idx].DataAvailable ? 1 : 0);
        }


        static void Cmd_WR() {

            byte con_idx = link.Rx8();
            int len = link.Rx16();

            byte[] buff = link.RxData(len);
            net_stream[con_idx].Write(buff, 0, buff.Length);
        }

        static void Cmd_RD() {

            byte con_idx = link.Rx8();
            int len = link.Rx16();

            byte[] buff = new byte[len];

            for (int i = 0; i < len;) {
                int rdx = net_stream[con_idx].Read(buff, i, len - i);
                dev.FifoWR(buff, i, rdx);
                i += rdx;
            }

        }

    }
}
