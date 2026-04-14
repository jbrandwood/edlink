using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.Device {
    internal interface IDeviceIO {

        Link Link { get; }
        void ExitServiceMode();
        void EnterServiceMode();
        void MemWR(int addr, byte[] buff, int offset, int len);
        void MemRD(int addr, byte[] buff, int offset, int len);
        void FlaWR(int addr, byte[] buff, int offset, int len);
        void FlaRD(int addr, byte[] buff, int offset, int len);
        void FpgInit(byte[] data);
        void FileOpen(string path, int mode);
        void FileClose();
        UInt64 FileAvailable();
        void FileRead(byte[] buff, int offset, int len);
        void FileWrite(byte[] buff, int offset, int len);
        void RtcSet(DateTime dt);
        int RtcCal(DateTime dt, byte arg);
        void RtcCalSet(int ppm_val);
    }
}
