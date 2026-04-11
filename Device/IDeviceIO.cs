using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink.Device {
    internal interface IDeviceIO {

        Link Link { get; }
        void exitServiceMode();
        void enterServiceMode();
        void MemWR(int addr, byte[] buff, int offset, int len);
        void MemRD(int addr, byte[] buff, int offset, int len);
        void FlaWR(int addr, byte[] buff, int offset, int len);
        void FlaRD(int addr, byte[] buff, int offset, int len);
        void fpgInit(byte[] data);
        void fileOpen(string path, int mode);
        void fileClose();
        UInt64 fileAvailable();
        void fileRead(byte[] buff, int offset, int len);
        void fileWrite(byte[] buff, int offset, int len);
        void rtcSet(DateTime dt);
    }
}
