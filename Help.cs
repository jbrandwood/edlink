using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Edlink {
    internal class Help {


        static readonly string[][] help =
{
new[] { Cli.CmdLink,"configure USB connection",
@"
Syntax:
  edlink .link [--port <name>] [--dev-id <id>] [--protocol-id <id>]

Arguments:
  --port         COM port name
  --dev-id       target device type
  --protocol-id  device family

Description:
  Configures USB connection parameters.

Examples:
  edlink .link --port COM22 devinf
  edlink .link --dev-id 0x27 --port COM22 devinf
edlink .link --protocol-id 0x10 devinf

Note:
  At least one argument must be specified.
  If .link is not used, the program scans ports and connects to the first available device.
"
},

new[] { Cli.CmdMute,"disable console output",
@"
Syntax:
  edlink .mute

Description:
  Disables console output to allow binary data exchange via stdout
  with another program.

Examples:
  edlink .mute
"
},

 new[] { Cli.CmdHelp,"show command help",
@"
Syntax:
  edlink .help --cmd <name>

Arguments:
  --cmd  command name

Description:
  Shows detailed help for the specified command.

Examples:
  edlink .help --cmd cp
  edlink .help --cmd run
"
},

 new[] { Cli.CmdCp,"copy file",
@"
Syntax:
  edlink cp --src <source_path> --dst <destination_path>

Arguments:
  --src    Path to source file (e.g. d:/file.bin)
  --dst    Path to destination file (e.g. sd:/file.bin)

Description:
  Copies a file from source path to destination path.

Example:
  edlink cp --src d:/file.bin --dst sd:/file.bin

Note:
  ""sd:"" paths refer to the cartridge; others refer to the host.
"
},

new[] { Cli.CmdDevInf,"show device info",
@"
Syntax:
  edlink devinf [--file <path>]

Arguments:
  --file  optional, save output to file

Examples:
  edlink devinf
  edlink devinf --file d:/info.txt
"
},

new[] { Cli.CmdDiag,"run diagnostics",
@"
Syntax:
  edlink diag

Description:
  Tests the cartridge for errors.

Examples:
  edlink diag
"
},

new[] { Cli.CmdDscmd,"device-specific command",
@"
Syntax:
  edlink dscmd --cmd <name> [options]

Arguments:
  --cmd   subcommand name

Description:
  Sends a subcommand to the device command handler. Additional options
  depend on the selected subcommand.

Examples:
  edlink dscmd --cmd cicupd --file build/ed64-cic.bin
"
},

new[] { Cli.CmdFifoWr,"write to FIFO",
@"
Syntax:
  edlink fifowr --file <path> [--offset <value>] [--len <value>]

Arguments:
  --file    source file
  --offset  optional, file offset
  --len     optional, data length

Description:
  Writes data directly to the FIFO buffer read by the console.
  Used with usbrd cmd for direct communication between console and PC.

Examples:
  edlink fifowr --file data.bin
  edlink fifowr --file - --len 512
  edlink fifowr --file data.bin --offset 0x100 --len 512

Note:
  ""--file -"" reads data from stdin.
  --len is required when using stdin.
"
},

new[] { Cli.CmdFlaRd,"read flash",
@"
Syntax:
  edlink flard --file <path> --addr <value> --len <value>

Arguments:
  --file  output file
  --addr  flash address
  --len   data length

Description:
  Reads data from on-board flash memory.

Examples:
  edlink flard --file dump.bin --addr 0x00000000 --len 0x1000
"
},

new[] { Cli.CmdFlaWr,"write flash",
@"
Syntax:
  edlink flawr --file <path> --addr <value> [--offset <value>] [--len <value>]

Arguments:
  --file    input file
  --addr    flash address
  --offset  optional, file offset
  --len     optional, data length

Description:
  Writes data to on-board flash memory.

Examples:
  edlink flawr --file dump.bin --addr 0x00000000
  edlink flawr --file dump.bin --addr 0x00000000 --offset 0x100 --len 0x1000

Note:
  Use only for recovery. Incorrect use may damage cartridge functionality.
"
},

new[] { Cli.CmdFpga,"configure FPGA",
@"
Syntax:
  edlink fpga --file <path>

Arguments:
  --file  bitstream file

Description:
  Configures FPGA with the specified file.

Examples:
  edlink fpga --file core.rbf
"
},


new[] { Cli.CmdMcuMode,"set MCU mode",
@"
Syntax:
  edlink mcumode --mode <app|service>

Arguments:
  --mode  target mode: app or service

Description:
  Sets MCU operating mode.

Examples:
  edlink mcumode --mode app
  edlink mcumode --mode service

Note:
  Service mode is for firmware recovery only.
  If not used, device defaults to app mode.
"
},

new[] { Cli.CmdMemRd,"read FCI bus",
@"
Syntax:
  edlink memrd --addr <value> --len <value> [--file <path>] [--print]

Arguments:
  --addr   address
  --len    data length
  --file   optional, output file
  --print  optional, print hex dump

Description:
  Reads internal memory on FCI bus (ROM, RAM, FPGA registers, etc.).

Examples:
  edlink memrd --addr 0x00000000 --len 0x100 --print
  edlink memrd --addr 0x00000000 --len 0x100 --file dump.bin
  edlink memrd --addr 0x00000000 --len 0x100 --file -

Note:
  At least one of --file or --print must be specified.
  If both are set, data is printed and saved.
  ""--file -"" sends raw binary data to stdout.
"
},

new[] { Cli.CmdMemWr,"write FCI bus",
@"
Syntax:
  edlink memwr --addr <value> --file <path> [--offset <value>] [--len <value>]

Arguments:
  --addr    address
  --file    input file
  --offset  optional, file offset
  --len     optional, data length

Description:
  Writes data to internal memory on FCI bus (ROM, RAM, FPGA registers, etc.).

Examples:
  edlink memwr --addr 0x00000000 --file data.bin
  edlink memwr --addr 0x00000000 --file - --len 512
  edlink memwr --addr 0x00000000 --file data.bin --offset 0x100 --len 0x200

Note:
  ""--file -"" reads data from stdin.
  --len is required when using stdin.
"
},

new[] { Cli.CmdNetGame,"TCP/IP bridge",
@"
Syntax:
  edlink netgate

Description:
  Starts a simple TCP/IP bridge over USB for console applications.

Examples:
  edlink netgate

Note:
  See NetGate.cs and edio samples for details.
"
},

new[] { Cli.CmdReset,"reset console",
@"
Syntax:
  edlink reset [--mode <hard|soft|off>]

Arguments:
  --mode  optional, reset mode

Description:
  Resets the console.

Examples:
  edlink reset
  edlink reset --mode hard
  edlink reset --mode hard reset --mode off

Note:
  Behavior may vary across platforms. On some platforms, the command may work only 
  when the menu is running, or the --mode argument may be ignored.
"
},

new[] { Cli.CmdRtcCal,"RTC calibration",
@"
Syntax:
  edlink rtccal (--val <value> | --cmd <phase>)

Arguments:
  --val   set calibration value (-255..255)
  --cmd   calibration phase:
          0 - cancel calibration, set time
          1 - start calibration
          2 - finish calibration (run after 10–20 hours)
          3 - show current calibration value
          4 - show calculated calibration value (phase 1 only)
          5 - show clock drift

Description:
  Calibrates RTC crystal. Automatic calibration is performed in two phases:
  start (phase 1) and finish (phase 2).

Examples:
  edlink rtccal --val 10
  edlink rtccal --cmd 1
  edlink rtccal --cmd 2

Note:
  Use either --val or --cmd (not both).
  Phase 2 should be executed 10–20 hours after phase 1.
  Internet access is required for accurate time synchronization.
"
},

new[] { Cli.CmdRtcSet,"set RTC time",
@"
Syntax:
  edlink rtcset

Description:
  Sets cartridge system time using current PC time.

Examples:
  edlink rtcset
"
},

new[] { Cli.CmdRun,"run ROM",
@"
Syntax:
  edlink run --file <path> [--fpga <path>]

Arguments:
  --file  ROM file (host or ""sd:"" path)
  --fpga  optional, FPGA bitstream

Description:
  Runs a ROM on the console. Supports loading from host or cartridge SD.
  Optional FPGA bitstream can be loaded before execution to enable custom mappers.

Examples:
  edlink run --file d:/game.n64
  edlink run --file d:/game.n64 --fpga d:/core.rbf
  edlink run --file sd:/games/game.n64

Note:
  Paths starting with ""sd:"" refer to the cartridge.
  --fpga is not supported on all devices.
  If edlink is launched with a single argument (file path), it is treated as:
  run --file <arg>.
  Drag-and-drop of a ROM onto edlink.exe is supported.
"
},

new[] { Cli.CmdScreen,"capture menu screenshot",
@"
Syntax:
  edlink screen [--file <path>]

Arguments:
  --file  optional, output file

Description:
  Captures a screenshot of the menu.

Examples:
  edlink screen
  edlink screen --file shot.png

Note:
  If --file is not set, screenshot is saved in the working directory
  with a name based on current date and time.
  Mainly used for manual screenshots.
"
},

new[] { Cli.CmdUsbRd,"read from USB",
@"
Syntax:
  edlink usbrd [--len <value>] [--file <path>] [--print]

Arguments:
  --len    optional, data length (0 = unlimited)
  --file   optional, output file
  --print  optional, print as UTF-8 text

Description:
  Receives data from USB. 
  Used with fifowr cmd for direct communication between console and PC.

Examples:
  edlink usbrd --print
  edlink usbrd --len 512 --file dump.bin
  edlink usbrd --file -

Note:
  At least one of --file or --print must be specified.
  If both are set, data is printed and saved.
  If --len is 0 or not set, reception continues until interrupted.
  ""--file -"" redirects raw data to stdout.
"
},

new[] { Cli.CmdUsbSpd,"USB speed test",
@"
Syntax:
  edlink usbspd [--addr <value>] [--len <value>]

Arguments:
  --addr  optional, FCI address (default: 0)
  --len   optional, test block size (default: 2 MB)

Description:
  Tests USB data transfer speed between PC and cartridge.

Examples:
  edlink usbspd
  edlink usbspd --addr 0x00000000 --len 0x200000

Note:
  Default address 0 typically maps to ROM area.
  Transfer speed may vary depending on the motherboard USB controller.
"
},

};


        public static void Print() {

            int max_name_len = 0;

            for (int i = 0; i < help.Length; i++) {
                max_name_len = Math.Max(max_name_len, help[i][0].Length);
            }

            for (int i = 0; i < help.Length; i++) {

                Console.WriteLine(help[i][0].PadRight(max_name_len) + " - " + help[i][1]);
            }

            Console.WriteLine("");
            Console.WriteLine("Type \".help --cmd <name>\" for detailed command info.");
            Console.WriteLine("");

        }

        public static void Print(string cmd) {


            

            for (int i = 0; i < help.Length; i++) {

                if (!help[i][0].EndsWith(cmd)) {
                    continue;
                }

                ConsoleColor c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(help[i][0] + " - " + help[i][1]);
                Console.ForegroundColor = c;
                Console.WriteLine(help[i][2]);
                return;
            }

            throw new Exception("unknown command: " + cmd);
            //Console.WriteLine("unknown command: " + cmd);
        }


    }
}
