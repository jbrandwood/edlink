
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
                resp = TestMEM("RAM0", DeviceIO.ADDR_PRG1, dev.SIZE_PRG1);
                PrintResp(resp);
            }

            if (dev.SIZE_PRG2 > 0) {
                resp = TestMEM("RAM1", DeviceIO.ADDR_PRG2, dev.SIZE_PRG2);
                PrintResp(resp);
            }

            if (dev.SIZE_SRAM > 0) {
                resp = TestMEM("RAM2", DeviceIO.ADDR_SRAM, dev.SIZE_SRAM);
                PrintResp(resp);
            }

            if (dev.SIZE_BRAM > 0) {
                resp = TestMEM("RAM3", DeviceIO.ADDR_BRAM, dev.SIZE_BRAM);
                PrintResp(resp);
            }

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
