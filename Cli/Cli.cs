using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edlink {
    internal class Cli {

        public const string NewCmd = "--";

        public const string ArgPrefix = "--";
        public const string ArgFile = "--file";
        public const string ArgOut = "--out";
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
        public const string ArgBoot = "--boot";
        public const string ArgApp = "--app";

        public const string CmdLink = ".link";
        public const string CmdRun = "run";
        public const string CmdSetMode = "setmode";
        public const string CmdMemRd = "memrd";
        public const string CmdMemWr = "memwr";
        public const string CmdFlaRd = "flard";
        public const string CmdFlaWr = "flawr";
        public const string CmdReset = "reset";
        public const string CmdFpga = "fpga";
        public const string CmdCp = "cp";
        public const string CmdRtcSet = "rtcset";
        public const string CmdMcuUpd = "mcuupd";


        public const string ModeApp = "app";
        public const string ModeService = "service";

    }
}
