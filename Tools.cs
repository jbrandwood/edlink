using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Edlink {
    internal class Tools {

        public static string VdcToStr(int vdc) {


            return (vdc >> 8).ToString("X2") + "." + (vdc & 0xff).ToString("X2");
        }

        public static string TsToDate(int ts) {

            UInt32 date = (UInt32)ts & 0xffff;

            string msg = "";

            msg += ByteToBcd(date & 31).ToString("X2");
            msg += ".";
            msg += ByteToBcd((date >> 5) & 15).ToString("X2");
            msg += ".";
            msg += ((date >> 9) + 1980);

            return msg;
        }


        public static string TsToVersion(int ts) {

            UInt32 ver = TsToVersion((UInt32)ts);
            string msg = "";

            msg += ((ver >> 16) & 0xff).ToString("X2");
            msg += ".";
            msg += (ver & 0xffff).ToString("X4");

            return msg;
        }

        static UInt32 TsToVersion(UInt32 ts) {

            UInt32 ver = 0;
            UInt32 date = ts & 0xffff;


            ver |= ByteToBcd((date >> 9) - 20) << 8;
            ver <<= 8;
            ver |= ByteToBcd((date >> 5) & 15) << 8;
            ver |= ByteToBcd(date & 31);

            return ver;
        }

        public static string SizeToStr(int size) {

            string msg = "";

            if (size < 1024) {
                msg = size + "B";
            } else if (size < 0x100000) {
                msg = size / 1024 + "kB";
            } else {
                msg = size / 1024 / 1024 + "MB";
            }

            return msg;
        }

        public static void PrintLine(string msg, ConsoleColor color) {

            Print(msg, color);
            Console.WriteLine();
        }

        public static void Print(string msg, ConsoleColor color) {

            ConsoleColor old_color = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ForegroundColor = old_color;
        }

        static UInt32 ByteToBcd(UInt32 val) {

            val &= 0xff;

            if (val > 99) {
                val = 99;
            }
            return (val / 10 << 4) | val % 10;
        }
        

    }

}
