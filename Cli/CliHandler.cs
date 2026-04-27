using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Edlink.Device;


namespace Edlink {
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

                if (cmd_list[i].Name.Equals(Cli.CmdLink)) {
                    base.LinkConfig(cmd_list[i], link);
                    continue;
                }
            }

            bool set_def_mode = true;

            for (int i = 0; i < cmd_list.Length; i++) {

                if (cmd_list[i].Name.StartsWith(".")) {
                    continue;
                }

                if (cmd_list[i].Name.Equals(Cli.CmdMcuMode)) {
                    set_def_mode = false;
                }
                break;
            }

            if (set_def_mode && cmd_list.Length > 0) {
                string[] mode_cmd = new string[] { Cli.CmdMcuMode, Cli.ArgMode, Cli.ModeApp };
                cmd_list = CmdLine.Parse(mode_cmd).Concat(cmd_list).ToArray();
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

                case Cli.CmdMcuMode:
                    base.McuMode(cmd);
                    break;

                case Cli.CmdMemRd:
                    base.MemRD(cmd);
                    break;

                case Cli.CmdMemWr:
                    base.MemWR(cmd);
                    break;

                case Cli.CmdFlaRd:
                    base.FlaRD(cmd);
                    break;

                case Cli.CmdFlaWr:
                    base.FlaWR(cmd);
                    break;

                case Cli.CmdReset:
                    base.Reset(cmd);
                    break;

                case Cli.CmdRun:
                    base.Run(cmd);
                    break;

                case Cli.CmdFpga:
                    base.FpgaInit(cmd);
                    break;

                case Cli.CmdCp:
                    base.Copy(cmd);
                    break;

                case Cli.CmdRtcSet:
                    base.RtcSet(cmd);
                    break;

                case Cli.CmdRtcCal:
                    base.RtcCal(cmd);
                    break;

                case Cli.CmdMcuApp:
                    base.McuApp(cmd);
                    break;

                case Cli.CmdMcuBoot:
                    base.McuBoot(cmd);
                    break;

                case Cli.CmdUsbSpd:
                    base.UsbSpd(cmd);
                    break;

                case Cli.CmdDevInf:
                    base.DevInf(cmd);
                    break;

                case Cli.CmdUsbRd:
                    base.UsbRD(cmd);
                    break;

                case Cli.CmdScreen:
                    base.Screen(cmd);
                    break;

                case Cli.CmdDiag:
                    base.Diag(cmd);
                    break;

                case Cli.CmdDscmd:
                    base.Dscmd(cmd);
                    break;

                default:
                    throw new CmdException(CmdExceptionType.UnknownCmd);
            }
        }


    }
}
