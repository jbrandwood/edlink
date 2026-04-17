using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edlink.Device {
    abstract class Diagnostics {

        protected DeviceIO dev;

        public abstract void Start();

        protected int testMEM(string name, int addr, int size) {

            byte[] buff;
            Console.Write("Testing " + name + "...");

            //simple data bus test
            buff = new byte[] { 0xAA };
            dev.MemWR(addr, buff, 0, buff.Length);
            dev.MemRD(addr, buff, 0, buff.Length);
            if (buff[0] != 0xAA) return 0x01;
            buff = new byte[] { 0x55 };
            dev.MemWR(addr, buff, 0, buff.Length);
            dev.MemRD(addr, buff, 0, buff.Length);
            if (buff[0] != 0x55) return 0x01;


            //full data bus + partial address bus test
            buff = new byte[256];
            for (int i = 0; i < buff.Length; i++) buff[i] = (byte)i;
            dev.MemWR(addr, buff, 0, buff.Length);
            dev.MemRD(addr, buff, 0, buff.Length);
            for (int i = 0; i < buff.Length; i++) {
                if (buff[i] != i) return 0x02;
            }

            //full address bus test
            for (int i = 0; i < size; i *= 2) {
                buff = BitConverter.GetBytes(i);
                dev.MemWR(addr + i, buff, 0, buff.Length);
                if (i == 0) i = 2;
            }
            for (int i = 0; i < size; i *= 2) {
                dev.MemRD(addr + i, buff, 0, buff.Length);
                int val = BitConverter.ToInt32(buff, 0);
                if (val != i) return 0x03;
                if (i == 0) i = 2;
            }

            //random data test
            Random rnd = new Random((int)DateTime.Now.Ticks);
            byte[] rnd_dat = new byte[Math.Min(0x10000, size)];
            buff = new byte[rnd_dat.Length];
            rnd.NextBytes(rnd_dat);
            dev.MemWR(addr, rnd_dat, 0, rnd_dat.Length); 
            dev.MemRD(addr, buff, 0, buff.Length);
            for (int i = 0; i < buff.Length; i++) {
                if (buff[i] != rnd_dat[i]) return 0x04;
            }

            return 0;
        }

        protected int testRTC() {

            RtcTime rtc_old;
            RtcTime rtc_now;
            Console.Write("Testing RTC...");

            //check if rtc working at all
            rtc_old = dev.RtcGet();
            Thread.Sleep(1100);
            if (dev.RtcGet().sec == rtc_old.sec) return 0x01;

            //check accuracy
            rtc_old = dev.RtcGet();
            rtc_now = rtc_old;


            while (rtc_now.sec == rtc_old.sec) {
                rtc_now = dev.RtcGet();
            }
            rtc_old = rtc_now;

            long ticks = DateTime.Now.Ticks;
            while (rtc_old.sec == dev.RtcGet().sec) ;

            ticks = (DateTime.Now.Ticks - ticks) / 10000;
            if (ticks > 1020) return 0x02;
            if (ticks < 980) return 0x03;

            return 0;
        }

        protected int printVDC(string name, UInt16 vdc, int min, int max) {

            bool ok = vdc >= min && vdc <= max;
            Console.Write(name + " - " + (vdc >> 8).ToString("X2") + "." + (vdc & 0xff).ToString("X2"));
            ConsoleColor old = Console.ForegroundColor;

            if (ok) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" OK");
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" ERROR");
            }

            Console.ForegroundColor = old;

            return ok ? 0 : 1;
        }

        protected void printResp(int resp) {

            ConsoleColor old = Console.ForegroundColor;

            if (resp == 0) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK");
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: 0x" + resp.ToString("X2"));
            }

            Console.ForegroundColor = old;
        }
    }
}
