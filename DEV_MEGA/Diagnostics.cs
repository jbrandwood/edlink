using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Edlink.Device.DeviceIO_V1;

namespace Edlink.DEV_MEGA {
    internal class Diagnostics : Device.Diagnostics {

        protected new DeviceIO dev;
        public Diagnostics(DeviceIO dev) {

            base.dev = this.dev = dev;
        }


        public override void Start() {

            int resp;

            dev.ExitServiceMode();

            if (dev.SIZE_PRG1 > 0) {
                resp = testMEM("RAM0", DeviceIO.ADDR_PRG1, dev.SIZE_PRG1);
                printResp(resp);
            }

            if (dev.SIZE_PRG2 > 0) {
                resp = testMEM("RAM1", DeviceIO.ADDR_PRG2, dev.SIZE_PRG2);
                printResp(resp);
            }

            if (dev.SIZE_SRAM > 0) {
                resp = testMEM("RAM2", DeviceIO.ADDR_SRAM, dev.SIZE_SRAM);
                printResp(resp);
            }

            if (dev.SIZE_BRAM > 0) {
                resp = testMEM("RAM3", DeviceIO.ADDR_BRAM, dev.SIZE_BRAM);
                printResp(resp);
            }

            resp = testRTC();
            printResp(resp);

            testVDC();

            dev.RtcGet().Print();
        }

        void testVDC() {

            Vdc vdc;

            dev.GetVdc();
            vdc = dev.GetVdc();

            printVDC("Battery ", vdc.bat, 0x250, 0x345);
            printVDC("VCC 5.0v", vdc.v50, 0x440, 0x510);
            printVDC("VCC 2.5v", vdc.v25, 0x240, 0x260);
            printVDC("VCC 1.2v", vdc.v12, 0x110, 0x130);

        }

    }
}
