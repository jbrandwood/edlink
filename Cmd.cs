using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edlink {
    internal class Cmd {

        string[] cmd;

        public Cmd(string[] args, int offset) {

            int cmd_size = 0;

            for (int i = offset; i < args.Length; i++) {

                if (args[i].Trim().Equals("--")) {
                    break;
                }

                cmd_size++;
            }

            cmd = new string[cmd_size];
            for (int i = 0; i < cmd_size; i++) {

                cmd[i] = args[offset + i];
                if (cmd[i].StartsWith("--") || i == 0) {
                    cmd[i] = cmd[i].ToLower().Trim();
                }
            }
        }

        public string Name {
            get { return cmd[0]; }
        }

        public bool HasArg(string arg) {

            try {
                SeekArg(arg);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public string getStr(string arg_name) {

            int arg_idx = SeekArg(arg_name);
            int val_idx = SeekVal(arg_idx);

            return cmd[val_idx].Trim();
        }

        public int getInt(string arg_name) {

            string val = getStr(arg_name).ToLower();

            if (val.StartsWith("0x")) {
                return int.Parse(val.Substring(2), NumberStyles.HexNumber);
            } else {
                return int.Parse(val);
            }
        }

        public static Cmd[] Parse(string[] args) {

            args = ParseSpecial(args);

            var cmd_list = new List<Cmd>();

            if (args.Length == 1) {
            }

            for (int i = 0; i < args.Length;) {

                Cmd c = new Cmd(args, i);
                if (c.Size == 0) {
                    i++;
                    continue;
                }
                i += c.Size;
                cmd_list.Add(c);
            }

            return cmd_list.ToArray();
        }
        //************************************************************************************************
        static string[] ParseSpecial(string[] args) {

            if (args.Length != 1) {
                return args;
            }

            try {
                if (!File.Exists(args[0])) {
                    return args;
                }
            } catch (Exception) {
                return args;
            }

            return new string[] { "run", "--file", args[0] };
        }
        int Size {
            get { return cmd.Length; }
        }

        int SeekVal(int arg_idx) {

            if (arg_idx + 1 >= cmd.Length || cmd[arg_idx + 1].StartsWith("--")) {
                throw new Exception("argument " + cmd[arg_idx] + " requires a value");
            }

            return arg_idx + 1;
        }

        int SeekArg(string arg) {

            if (!arg.StartsWith("--")) {
                throw new Exception("invalid argument name " + arg);
            }

            arg = arg.ToLower();

            for (int i = 1; i < cmd.Length; i++) {
                if (cmd[i].Equals(arg)) {
                    return i;
                }
            }

            throw new Exception("missing required option " + arg);
        }
    }
}
