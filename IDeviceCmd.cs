using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edlink {

    public interface IDeviceCmd {


        string DeviceName { get; }

        void Stop();

        void SetMode(string mode);

        void MemWR(int addr, byte[] buff, int offset, int len);

        void MemRD(int addr, byte[] buff, int offset, int len);

        void FlaWR(int addr, byte[] buff, int offset, int len);

        void FlaRD(int addr, byte[] buff, int offset, int len);

        void Reset();

        void Run(string rom_path, string fpga_path);

        void FpgaInit(string path);

        void CopyFile(string src, string dst);

        void SpecialCmd(CmdLine cmd);
    }
}
