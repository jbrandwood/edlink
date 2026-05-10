using System;
using System.Net;
using System.Net.Sockets;

namespace Edlink {
    internal class NtpTime {

        static long max_delta_x = TimeSpan.FromMilliseconds(10).Ticks;

        static string[] server_list = {

            "time.google.com",
             "time.aws.com",
             "time.apple.com",
             "pool.ntp.org",
             "time.windows.com",
        };

        public static long GetDeltaTicks() {

            for (int i = 0; i < server_list.Length; i++) {

                try {
                    return TryGetDeltaTicks(server_list[i]);
                } catch (Exception) { }

            }

            throw new Exception("ntp server");
        }

        public static DateTime GetDeltaTime(long delta_ticks) {

            return new DateTime(DateTime.Now.Ticks - delta_ticks);
        }

        public static DateTime GetNetTime() {

            long delta = GetDeltaTicks();

            return new DateTime(DateTime.Now.Ticks - delta);
        }

        public static DateTime GetNetTime(string server) {

            byte[] ntp_data = new byte[48];
            ntp_data[0] = 0x1B;

            var addresses = Dns.GetHostEntry(server).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);

            var socket = new UdpClient();
            socket.Client.ReceiveTimeout = 1000; // ms
            socket.Connect(ipEndPoint);

            var t1 = DateTime.UtcNow;

            socket.Send(ntp_data, ntp_data.Length);
            var recv = socket.Receive(ref ipEndPoint);

            var t4 = DateTime.UtcNow;

            DateTime t2 = ReadTstamp(recv, 32);
            DateTime t3 = ReadTstamp(recv, 40);

            TimeSpan delta = (t2 - t1) + (t3 - t4);
            TimeSpan offset = TimeSpan.FromTicks(delta.Ticks / 2);

            return (t4 + offset).ToLocalTime();
        }

        public static long GetDeltaTicks(string server, int avg_num) {

            avg_num = Math.Min(avg_num, 100);
            avg_num = Math.Max(avg_num, 1);

            long delta = 0;

            for (int i = 0; i < avg_num; i++) {

                long t_net = GetNetTime(server).Ticks;
                long t_loc = DateTime.Now.Ticks;
                delta += (t_loc - t_net);
            }

            return delta / avg_num;
        }

        static long TryGetDeltaTicks(string server) {

            int avg_num = 2;

            long delta_a = GetDeltaTicks(server, avg_num);
            long delta_b = GetDeltaTicks(server, avg_num);

            long delta_x;
            if (Math.Abs(delta_a) > Math.Abs(delta_b)) {
                delta_x = Math.Abs(delta_a) - Math.Abs(delta_b);
            } else {
                delta_x = Math.Abs(delta_b) - Math.Abs(delta_a);
            }

            if (delta_x > max_delta_x) {
                throw new Exception("ntp sync. delta-x: " + delta_x / TimeSpan.FromMilliseconds(1).Ticks);
            }

            //Console.WriteLine("delta-x: " + delta_x / TimeSpan.FromMilliseconds(1).Ticks);

            return (delta_a + delta_b) / 2;
        }

        static DateTime ReadTstamp(byte[] data, int offset) {

            ulong intPart = SwapEndian(BitConverter.ToUInt32(data, offset));
            ulong fractPart = SwapEndian(BitConverter.ToUInt32(data, offset + 4));

            ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            return new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);
        }

        static uint SwapEndian(ulong x) {

            return (uint)(
                ((x & 0x000000ff) << 24) +
                ((x & 0x0000ff00) << 8) +
                ((x & 0x00ff0000) >> 8) +
                ((x & 0xff000000) >> 24));
        }

    }
}
