using Edlink.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Edlink.Device.DeviceIO_V1;

namespace Edlink.DEV_GBA {
    internal class Diagnostics : Device.Diagnostics {

        protected new DeviceIO dev;

        public Diagnostics(DeviceIO dev) {

            base.dev = this.dev = dev;
        }

        public override void Start() {

            int resp;

            dev.ExitServiceMode();

            if (dev.SIZE_RAM0 != 0) {
                resp = testMEM("RAM0", DeviceIO.ADDR_FCI_RAM0, dev.SIZE_RAM0);
                printResp(resp);
            }

            if (dev.SIZE_RAM1 != 0) {
                resp = testMEM("RAM1", DeviceIO.ADDR_FCI_RAM1, dev.SIZE_RAM1);
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
        }

    }
}
