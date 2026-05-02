using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Edlink {
    internal class Cli {

        public const string NewCmd = "--";

        public const string ArgPrefix = "--";
        public const string ArgFile = "--file";
        public const string ArgPrint = "--print";
        public const string ArgAddr = "--addr";
        public const string ArgLen = "--len";
        public const string ArgOffset = "--offset";
        public const string ArgFpga = "--fpga";
        public const string ArgSrc = "--src";
        public const string ArgDst = "--dst";
        public const string ArgMode = "--mode";
        public const string ArgPort = "--port";
        public const string ArgDevId = "--dev-id";
        public const string ArgProtId = "--protocol-id";
        public const string ArgVal = "--val";
        public const string ArgCmd = "--cmd";// sub commands


        public const string CmdLink = ".link";
        public const string CmdMute = ".mute";
        public const string CmdHelp = ".help";
        public const string CmdRun = "run";
        public const string CmdMcuMode = "mcumode";
        public const string CmdMemRd = "memrd";
        public const string CmdMemWr = "memwr";
        public const string CmdFlaRd = "flard";
        public const string CmdFlaWr = "flawr";
        public const string CmdUsbRd = "usbrd";
        public const string CmdFifoWr = "fifowr";
        public const string CmdReset = "reset";
        public const string CmdFpga = "fpga";
        public const string CmdCp = "cp";
        public const string CmdRtcSet = "rtcset";
        public const string CmdRtcCal = "rtccal";
        public const string CmdMcuBoot = "mcuboot";
        public const string CmdMcuApp = "mcuapp";
        public const string CmdUsbSpd = "usbspd";
        public const string CmdDevInf = "devinf";
        public const string CmdScreen = "screen";
        public const string CmdDiag = "diag";
        public const string CmdDscmd = "dscmd";//       device specific cmd
        public const string CmdNetGame = "netgate";//

        //ArgMode
        public const string ModeApp = "app";
        public const string ModeService = "service";

    }
}
