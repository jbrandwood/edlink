using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edlink {

    public interface IDeviceIO {


        string DeviceName { get; }

        void Stop();

        void MemWR(int addr, byte[] buff, int offset, int len);

        void MemRD(int addr, byte[] buff, int offset, int len);

        void Reset();

        void Run(string rom_path, string fpga_path);

        void FpgaInit(string path);
    }
}
