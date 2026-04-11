using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Edlink {
    internal class Program {


        static void Main(string[] args) {

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("edlink v" + Assembly.GetEntryAssembly().GetName().Version);

            //args = new string[] { "link", "--dcmd-id", "0x27"};
            //args = new string[] { "memprint", "--addr", "0xFF00000" };
            //args = new string[] { "link", "--port", "COM22", "--dcmd-id", "0x27", "--", "memprint", "--addr", "0xFF00000" };

            //args = new string[] { "linkz", "--dcmd-id", "0x27" };
            //args = new string[] {"setmode", "--mode", "service"};

            //args = new string[] { "mcuupd", "--boot", "xxx.bin",  "--app"};

            long time = DateTime.Now.Ticks;

            CliHandler cmd = null;

            /*
            cmd = new CliHandler(args);
            cmd.Start();
            cmd.Stop();
            cmd = null;
            return;*/

            try {
                cmd = new CliHandler(args);
                cmd.Start();
                cmd = null;
            } catch (Exception x) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.WriteLine("ERROR: " + x.Message);
                Console.ResetColor();
            }

            if (cmd != null) {
                cmd.Stop();
            }

            time = (DateTime.Now.Ticks - time) / 10000;
            Console.WriteLine("Exec time: " + time);
        }

    }
}
