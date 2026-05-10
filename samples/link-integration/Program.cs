using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace link_integration {
    internal class Program {


        public const string StdioPath = "-";

        static Process p = null;
        static Stream stdin = null;
        static Stream stdout = null;
        static readonly Encoding Encoder = new UTF8Encoding(false);

        static void Main(string[] args) {

            try {
                EdlinkIO();
            } catch (Exception x) {
                Console.WriteLine("ERROR: " + x);

                try {
                    Console.WriteLine("");
                    Console.WriteLine("EDLINK ERROR MSG: " + p.StandardError.ReadToEnd());
                } catch (Exception) { }

            }


        }

        static void EdlinkIO() {

            var psi = new ProcessStartInfo {

                FileName = "../../../../edlink/bin/Release/edlink.exe",
                Arguments = ".stdio",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };


            p = Process.Start(psi);

            stdin = p.StandardOutput.BaseStream;
            stdout = p.StandardInput.BaseStream;

            byte[] buff;

            //read cart's device info
            WriteLine("devinf --file " + StdioPath);
            buff = ReadFile();
            Console.WriteLine(Encoder.GetString(buff).Trim());
            Console.WriteLine();


            //cart memory write
            buff = new byte[256];
            for (int i = 0; i < 256; i++) {
                buff[i] = (byte)i;
            }
            WriteLine("memwr --addr 0 --file " + StdioPath);
            WriteFile(buff);

            //cart memory read
            WriteLine("memrd --addr 0 --len 256 --file " + StdioPath);
            buff = ReadFile();
            for (int i = 0; i < buff.Length; i += 16) {
                Console.WriteLine(BitConverter.ToString(buff, i, 16));
            }
            Console.WriteLine();



            p.Kill();
        }

        public static string ReadLine() {
            //return stdin_txt.ReadLine();

            MemoryStream ms = new MemoryStream();

            while (true) {

                int b = stdin.ReadByte();

                if (b == -1) {
                    throw new Exception("EOF");
                }

                if (b == '\n') {
                    break;
                }

                if (b >= ' ') {
                    ms.WriteByte((byte)b);
                }
            }

            //return Encoder.GetString(ms.ToArray()).TrimStart('\uFEFF').Trim();
            return Encoder.GetString(ms.ToArray()).Trim();
        }
        public static void WriteLine(string txt) {

            byte[] buff = Encoder.GetBytes(txt + "\n");
            StdouWrite(buff, 0, buff.Length);
        }

        public static byte[] ReadFile() {



            int len = GetInt(ReadLine());
            byte[] buff = new byte[len];

            for (int i = 0; i < len;) {
                int block = stdin.Read(buff, 0, buff.Length - i);
                if (block < 0) {
                    throw new Exception("EOF");
                }
                i += block;
            }

            return buff;

        }

        public static void WriteFile(byte[] buff, int offset, int len) {

            WriteLine(len.ToString());
            StdouWrite(buff, offset, len);
        }
        public static void WriteFile(byte[] buff) {

            WriteFile(buff, 0, buff.Length);
        }

        public static void WriteFile(string txt) {

            byte[] buff = Encoding.UTF8.GetBytes(txt);
            WriteFile(buff);
        }


        static void StdouWrite(byte[] buff, int offset, int len) {

            stdout.Write(buff, offset, len);
            stdout.Flush();
        }

        static int GetInt(string val) {

            if (val.ToLower().StartsWith("0x")) {
                return int.Parse(val.Substring(2), NumberStyles.HexNumber);
            } else {
                return int.Parse(val);
            }
        }
    }
}
