using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace edlink {
    internal class CmdHandler {


        Link link;
        IDeviceIO dev;
        DeviceCmd dcmd;
        Cmd[] cmd_list;


        public CmdHandler(string[] argc) {

            cmd_list = Cmd.Parse(argc);
            link = new Link();


        }

        public void Start() {

            Preconfigure();
            OpenDevice();
            CmdExec();
            Stop();
        }

        public void Stop() {

            if (dev != null) {
                dev.Stop();
            }
            if (link != null) {
                link.Close();
            }
        }

        //************************************************************************************************
        void OpenDevice() {

            link.Open();
            dev = DeviceIOFacotoy.Create(link);

            string msg = "Connected:";
            msg += " port=" + link.PortName;
            msg += ", protocol=0x" + link.ProtocolID.ToString("X02");
            msg += ", device=0x" + link.DeviceID.ToString("X02");
            msg += ", name=" + dev.DeviceName;
            Console.WriteLine(msg);
        }

        void Preconfigure() {

            for (int i = 0; i < cmd_list.Length; i++) {

                if (cmd_list[i].Name.Equals("link")) {
                    LinkConfig(cmd_list[i]);
                    continue;
                }
            }
        }

        void LinkConfig(Cmd cmd) {

            string arg_port = "--port";
            string arg_dev_id = "--dev-id";
            string arg_prot_id = "--protocol-id";

            if (cmd.HasArg(arg_port)) {
                link.PortName = cmd.getStr(arg_port);
            }

            if (cmd.HasArg(arg_dev_id)) {
                link.DeviceID = (byte)cmd.getInt(arg_dev_id);
            }

            if (cmd.HasArg(arg_prot_id)) {
                link.ProtocolID = (byte)cmd.getInt(arg_prot_id);
            }
        }

        void CmdExec() {

            DeviceCmd dcmd = new DeviceCmd(dev);

            for (int i = 0; i < cmd_list.Length; i++) {
                CmdExec(dcmd, cmd_list[i]);
            }
        }

        void CmdExec(DeviceCmd dcmd, Cmd cmd) {

            switch (cmd.Name) {

                case "memprint":
                    dcmd.MemPrint(cmd);
                    break;

                case "memrd":
                    dcmd.MemRD(cmd);
                    break;

                case "memwr":
                    dcmd.MemWR(cmd);
                    break;

                case "reset":
                    dcmd.Reset(cmd);
                    break;

                case "run":
                    dcmd.Run(cmd);
                    break;

                case "fpga":
                    dcmd.FpgaInit(cmd);
                    break;
                default:
                    throw new Exception("unknown cmd: " + cmd.Name);
            }


        }


    }
}
