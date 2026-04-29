using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.Device {
    internal class RtcTime {

        public const int size = 8;
        public byte yar;
        public byte mon;
        public byte dom;
        public byte hur;
        public byte min;
        public byte sec;
        public byte dow;

        public RtcTime(byte[] data) {

            yar = data[0];
            mon = data[1];
            dom = data[2];
            hur = data[3];
            min = data[4];
            sec = data[5];
            dow = (byte)(data.Length > 6 ? data[6] : 1);
        }

        public RtcTime(DateTime dt) {

            yar = DecToBcd(dt.Year - 2000);
            mon = DecToBcd(dt.Month);
            dom = DecToBcd(dt.Day);
            hur = DecToBcd(dt.Hour);
            min = DecToBcd(dt.Minute);
            sec = DecToBcd(dt.Second);
            dow = DecToBcd((int)dt.DayOfWeek + 1);
        }

        byte DecToBcd(int val) {
            int hex = 0;
            hex |= (val / 10) << 4;
            hex |= (val % 10);
            return (byte)hex;
        }

        public byte[] GetVals() {

            byte[] vals = new byte[size];
            vals[0] = yar;
            vals[1] = mon;
            vals[2] = dom;
            vals[3] = hur;
            vals[4] = min;
            vals[5] = sec;
            vals[6] = dow;

            return vals;
        }

        public void Print() {

            Console.WriteLine("RTC date: " + dom.ToString("X2") + "." + mon.ToString("X2") + ".20" + yar.ToString("X2"));
            Console.WriteLine("RTC time: " + hur.ToString("X2") + ":" + min.ToString("X2") + ":" + sec.ToString("X2"));
        }
    }
}
