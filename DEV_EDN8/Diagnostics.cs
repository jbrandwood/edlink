using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Edlink.Device.DeviceIO_V1;

namespace Edlink.DEV_EDN8 {
    internal class Diagnostics : Device.Diagnostics {

        protected new DeviceIO dev;
        public Diagnostics(DeviceIO dev) {

            base.dev = this.dev = dev;
        }


        public override void Start() {

            int resp;

            dev.ExitServiceMode();

            resp = TestMEM("PRG", DeviceIO.ADDR_FCI_PRG, dev.SIZE_PRG);
            PrintResp(resp);

            resp = TestMEM("CHR", DeviceIO.ADDR_FCI_CHR, dev.SIZE_CHR);
            PrintResp(resp);

            resp = TestMEM("SRM", DeviceIO.ADDR_FCI_SRM, dev.SIZE_SRM);
            PrintResp(resp);

            resp = TestRTC();
            PrintResp(resp);

            testVDC();


            dev.RtcGet().Print();
        }

        void testVDC() {

            Vdc vdc;

            dev.GetVdc();
            vdc = dev.GetVdc();

            PrintVDC("Battery ", vdc.bat, 0x250, 0x345);
            PrintVDC("VCC 5.0v", vdc.v50, 0x440, 0x510);
            PrintVDC("VCC 2.5v", vdc.v25, 0x240, 0x260);
            PrintVDC("VCC 1.2v", vdc.v12, 0x110, 0x130);

        }
    }
}
