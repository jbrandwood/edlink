using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace edlink {
    internal class CliHandler : CliHandlerCmd {


        Link link;
        CmdLine[] cmd_list;

        public CliHandler(string[] argc) {

            cmd_list = CmdLine.Parse(argc);
            link = new Link();
        }

        public void Start() {

            Configure();
            OpenDevice();
            CmdExec();
            Stop();
        }

        public void Stop() {

            if (dcmd != null) {
                dcmd.Stop();
            }
            if (link != null) {
                link.Close();
            }
        }

        //************************************************************************************************
        void OpenDevice() {

            link.Open();
            dcmd = DeviceCmdFacotoy.Create(link);

            string msg = "Connected:";
            msg += " port=" + link.PortName;
            msg += ", protocol=0x" + link.ProtocolID.ToString("X02");
            msg += ", device=0x" + link.DeviceID.ToString("X02");
            msg += ", name=" + dcmd.DeviceName;
            Console.WriteLine(msg);
            Console.WriteLine();
        }

        void Configure() {

            for (int i = 0; i < cmd_list.Length; i++) {

                if (cmd_list[i].Name.Equals(".link")) {
                    LinkConfig(cmd_list[i]);
                    continue;
                }
            }

            bool set_def_mode = true;

            for (int i = 0; i < cmd_list.Length; i++) {

                if (cmd_list[i].Name.StartsWith(".")) {
                    continue;
                }

                if (cmd_list[i].Name.Equals("setmode")) {
                    set_def_mode = false;
                }
                break;
            }

            if (set_def_mode && cmd_list.Length > 0) {
                string[] mode_cmd = new string[] { "setmode", "--mode", "app" };
                cmd_list = cmd_list.Concat(CmdLine.Parse(mode_cmd)).ToArray();
            }
        }

        void LinkConfig(CmdLine cmd) {//move to Cmd base

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

        void CmdExceptionHandler(CmdLine cmd, CmdException x) {
            throw new Exception(cmd.Name + ": " + x.Message);

        }

        void CmdExec() {

            CmdLine cmd = null;

            try {
                for (int i = 0; i < cmd_list.Length; i++) {
                    cmd = cmd_list[i];
                    CmdExec(cmd);
                }
            } catch (CmdException x) {
                CmdExceptionHandler(cmd, x);
            }
        }

        void CmdExec(CmdLine cmd) {

            if (cmd.Name.StartsWith(".")) {
                return;
            }

            switch (cmd.Name) {

                case "setmode":
                    base.SetMode(cmd);
                    break;

                case "memprint":
                    base.MemPrint(cmd);
                    break;

                case "memrd":
                    base.MemRD(cmd);
                    break;

                case "memwr":
                    base.MemWR(cmd);
                    break;

                case "flard":
                    base.FlaRD(cmd);
                    break;

                case "flawr":
                    base.FlaWR(cmd);
                    break;

                case "reset":
                    base.Reset(cmd);
                    break;

                case "run":
                    base.Run(cmd);
                    break;

                case "fpga":
                    base.FpgaInit(cmd);
                    break;

                case "cp":
                    base.Copy(cmd);
                    break;

                default:
                    dcmd.SpecialCmd(cmd);
                    break;
            }
        }


    }
}
