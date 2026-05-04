using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;



namespace Edlink {
    internal class Stdio {

        public const string Dash = "-";

        static readonly Stream stdin = Console.OpenStandardInput();
        static readonly Stream stdout = Console.OpenStandardOutput();
        static readonly Encoding Encoder = new UTF8Encoding(false);


        public static string ReadLine() {
            //return stdin_txt.ReadLine();

            MemoryStream ms = new MemoryStream();

            while (true) {

                int b = stdin.ReadByte();

                if (b < 0) {
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

        public static byte[] Read(string path) {

            if (path.Equals(Dash)) {

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
            } else {
                return File.ReadAllBytes(path);
            }
        }

        public static void Write(string path, byte[] buff, int offset, int len) {

            if (path.Equals(Dash)) {

                WriteLine(len.ToString());
                StdouWrite(buff, offset, len);

            } else {
                File.WriteAllBytes(path, buff);
            }
        }
        public static void Write(string path, byte[] buff) {

            Write(path, buff, 0, buff.Length);
        }

        public static void Write(string path, string txt) {

            byte[] buff = Encoding.UTF8.GetBytes(txt);
            Write(path, buff);
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
