using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static System.Net.WebRequestMethods;

namespace Edlink {
    internal class Program {


        static void Main(string[] args) {

            //args = new string[] { ".help", "--cmd", "fifowr", ".help", "--cmd", "memwr" };


            if (args.Length > 0 && args[0].ToLower().Equals(Cli.CmdStdio)) {
                Console.SetOut(TextWriter.Null);
            }


            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("edlink v" + Assembly.GetEntryAssembly().GetName().Version);

            long time = DateTime.Now.Ticks;

            CliHandler cmd = null;

            if (args.Length == 0) {
                Help.Print();
            }

            /*
            Cmd = new CliHandler(args);
            Cmd.Start();
            Cmd.Stop();
            Cmd = null;
            return;*/

            try {
                cmd = new CliHandler(args);
                cmd.Start();
                cmd = null;
            } catch (Exception x) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.Error.WriteLine("ERROR: " + x.Message);
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
