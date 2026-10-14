using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Edlink.DEV_ED64 {

    internal class Diagnostics : Device.Diagnostics {

        protected new DeviceIO dev;
        public Diagnostics(DeviceIO dev) {

            base.dev = this.dev = dev;
        }

        public override void Start() {

            int resp;
            
            if (dev.SIZE_RAM0 != 0) {
                resp = TestMEM_ext("RAM0", DeviceIO.ADDR_FCI_RAM0, dev.SIZE_RAM0);
                PrintResp(resp);
            }

            if (dev.SIZE_RAM1 != 0) {
                resp = TestMEM_ext("RAM1", DeviceIO.ADDR_FCI_RAM1, dev.SIZE_RAM1);
                PrintResp(resp);
            }

            resp = TestRTC();
            PrintResp(resp);

            testVDC();

            dev.RtcGet().Print();
        }


        void testVDC() {

            int max_deviation = 5;//5% 

            PrintVDC("Battery ", (ushort)dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_BAT), 0x250, 0x345);
            PrintVDC_pd("VCC 3.3v", (ushort)dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_3V3), 0x330, max_deviation);
            PrintVDC_pd("VCC 2.5v", (ushort)dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_2V5), 0x250, max_deviation);
            PrintVDC_pd("VCC 1.2v", (ushort)dev.SysGetInf(DeviceIO.SysInf.INFD_VCC_1V2), 0x120, max_deviation);
        }
    }
}
