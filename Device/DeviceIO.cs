using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edlink.Device {

    abstract class DeviceIO {

        protected const byte STATUS_KEY = 0x5A;

        public const byte FA_READ = 0x01;
        public const byte FA_WRITE = 0x02;
        public const byte FA_OPEN_EXISTING = 0x00;
        public const byte FA_CREATE_NEW = 0x04;
        public const byte FA_CREATE_ALWAYS = 0x08;
        public const byte FA_OPEN_ALWAYS = 0x10;
        public const byte FA_OPEN_APPEND = 0x30;
        public const byte FS_MAKEPATH = 0x80; //make path if not exists

        public enum Rtcc {
            SET_TIME,
            CAL_START,
            CAL_END,
            GET_CURCAL,
            GET_ESTCAL,
            GET_DEVIAT,
            GET_SETCAL,
            MCO_OFF,
            MCO_ON,
        }

        protected Link link;

        public abstract Link Link { get; }
        public abstract void ExitServiceMode();
        public abstract void EnterServiceMode();
        public abstract void MemWR(int addr, byte[] buff, int offset, int len);
        public abstract void MemRD(int addr, byte[] buff, int offset, int len);
        public abstract void FlaWR(int addr, byte[] buff, int offset, int len);
        public abstract void FlaRD(int addr, byte[] buff, int offset, int len);
        public abstract void FifoWR(byte[] data, int offset, int len);
        public abstract void FpgInit(byte[] data);
        public abstract void FpgInit(string path);
        public abstract void FileOpen(string path, int mode);
        public abstract void FileClose();
        public abstract UInt64 FileAvailable();
        public abstract void FileRead(byte[] buff, int offset, int len);
        public abstract void FileWrite(byte[] buff, int offset, int len);
        public abstract RtcTime RtcGet();
        public abstract void RtcSet(DateTime dt);
        public abstract int RtcCal(DateTime dt, byte arg);
        public abstract void RtcCalSet(int ppm_val);



        public void FifoWR(string str) {

            byte[] bytes = Encoding.ASCII.GetBytes(str);
            FifoWR(bytes, 0, bytes.Length);
        }

        public void FifoTxString(string str) {

            byte[] bytes = Encoding.ASCII.GetBytes(str);
            byte[] len = link.Num16(bytes.Length);
            FifoWR(len, 0, 2);
            FifoWR(bytes, 0, bytes.Length);
        }


        protected int GetStatus() {
            return GetStatus(0);
        }

        protected int GetStatus(int timeout_ms) {

            byte[] resp = link.GetID(timeout_ms);

            if (resp[0] != STATUS_KEY || resp[1] != link.ProtocolID) {
                throw new Exception("unexpected status response (" + BitConverter.ToString(resp) + ")");
            }
            return resp[3];
        }

        protected void BootWait() {
            BootWait(5);
        }

        protected void BootWait(int max_time_sec) {

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
    }
}
