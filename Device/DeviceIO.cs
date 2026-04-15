using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public abstract Link Link { get; }
        public abstract void ExitServiceMode();
        public abstract void EnterServiceMode();
        public abstract void MemWR(int addr, byte[] buff, int offset, int len);
        public abstract void MemRD(int addr, byte[] buff, int offset, int len);
        public abstract void FlaWR(int addr, byte[] buff, int offset, int len);
        public abstract void FlaRD(int addr, byte[] buff, int offset, int len);
        public abstract void FpgInit(byte[] data);
        public abstract void FpgInit(string path);
        public abstract void FileOpen(string path, int mode);
        public abstract void FileClose();
        public abstract UInt64 FileAvailable();
        public abstract void FileRead(byte[] buff, int offset, int len);
        public abstract void FileWrite(byte[] buff, int offset, int len);
        public abstract void RtcSet(DateTime dt);
        public abstract int RtcCal(DateTime dt, byte arg);
        public abstract void RtcCalSet(int ppm_val);
    }
}
