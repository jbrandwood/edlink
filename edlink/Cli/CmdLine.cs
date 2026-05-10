using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Edlink {
    public class CmdLine {

        string[] cmd;

        public CmdLine(string[] args, int offset) {

            var cmd_args = new List<string>();

            if (offset >= args.Length) {
                throw new Exception("cmd line: out of args list");
            }

            if (args[offset].StartsWith(Cli.ArgPrefix)) {
                throw new Exception("cmd line: invalid cmd format '" + args[offset] + "'");
            }

            string last_val = Cli.ArgPrefix;

            for (int i = offset; i < args.Length; i++) {

                string val = args[i];


                if (val.StartsWith(Cli.ArgPrefix) || i == offset) {
                    //fixed format for arg and Cmd names
                    val = val.ToLower().Trim();
                }

                if (!val.StartsWith(Cli.ArgPrefix) && !last_val.StartsWith(Cli.ArgPrefix)) {
                    break;
                }

                if (val.Equals(Cli.ArgPrefix)) {
                    throw new Exception("cmd line: invalid arg '" + args[i] + "'");
                }


                last_val = val;

                cmd_args.Add(val);
            }

            cmd = cmd_args.ToArray();
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

        public string GetStr(string arg_name) {

            int arg_idx = SeekArg(arg_name);
            int val_idx = SeekVal(arg_idx);

            return cmd[val_idx].Trim();
        }

        public int GetInt(string arg_name) {

            string val = GetStr(arg_name).ToLower();

            if (val.StartsWith("0x")) {
                return int.Parse(val.Substring(2), NumberStyles.HexNumber);
            } else {
                return int.Parse(val);
            }
        }

        public static CmdLine[] Parse(string[] args) {

            args = ParseSpecial(args);

            var cmd_list = new List<CmdLine>();

            if (args.Length == 1) {
            }

            for (int i = 0; i < args.Length;) {

                CmdLine c = new CmdLine(args, i);

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

            //drag and exec
            return new string[] { Cli.CmdRun, Cli.ArgFile, args[0] };
        }
        int Size {
            get { return cmd.Length; }
        }

        int SeekVal(int arg_idx) {

            if (arg_idx + 1 >= cmd.Length || cmd[arg_idx + 1].StartsWith(Cli.ArgPrefix)) {
                throw new CmdException("argument " + cmd[arg_idx] + " requires a value");
            }

            return arg_idx + 1;
        }

        int SeekArg(string arg) {

            if (!arg.StartsWith(Cli.ArgPrefix)) {
                throw new CmdException("invalid argument name " + arg);
            }

            arg = arg.ToLower();

            for (int i = 1; i < cmd.Length; i++) {
                if (cmd[i].Equals(arg)) {
                    return i;
                }
            }

            throw new CmdException("missing required option " + arg);
        }
    }
}
