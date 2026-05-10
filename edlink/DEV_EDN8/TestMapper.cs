using System;
using System.IO;
using System.Reflection;
using Edlink.Device;


namespace Edlink.DEV_EDN8 {
    internal class TestMapper {

       
        internal static string getPath(string rom_path) {

            //it used for automatic mapper loading from mappers librarry stored on pc
            //drag rom on edlink.exe, if mapper for this file in librry, it will be used,
            //otherwise standard mapper stored on sd

            if (Link.IsDevPath(rom_path)) {
                return null;
            }

            string lib_bath = GetLib(rom_path);

            if (lib_bath == null) {
                return null;
            }

            if (!Directory.Exists(lib_bath)) {
                return null;
            }

            try {

                byte[] maprout = File.ReadAllBytes(lib_bath + "/maprout.bin");
                int mapper;

                if (rom_path.ToLower().EndsWith(".fds")) {
                    mapper = 254;
                } else {
                    byte[] rom = File.ReadAllBytes(rom_path);
                    mapper = (byte)((rom[6] >> 4) | (rom[7] & 0xf0));
                    if ((rom[7] & 0x0C) == 0x08) {//NES2.0
                        mapper |= (rom[8] & 0x0F) << 8;
                    }
                }

                if (maprout[mapper] == 0xff) {
                    return null;
                }

                string map_path = lib_bath + "/" + maprout[mapper].ToString("D3") + "/output_files/top.rbf";

                if (File.Exists(map_path)) {
                    return map_path;
                } else {
                    return null;
                }

            } catch (Exception) { }


            return null;
        }

        static string TrimPath(string path) {
            return path.Replace("\\", "/").TrimStart('/');
        }

        static string GetLib(string rom_path) {

            string link_name = "/testpath.txt";

            string link_path = Path.GetDirectoryName(rom_path) + link_name;
            link_path = TrimPath(link_path);

            if (File.Exists(link_path)) {
                return TrimPath(File.ReadAllText(link_path).Trim());
            }

            string exe_path = Assembly.GetExecutingAssembly().Location;
            link_path = Path.GetDirectoryName(exe_path) + link_name;
            link_path = TrimPath(link_path);

            if (File.Exists(link_path)) {
                return TrimPath(File.ReadAllText(link_path).Trim());
            }

            return null;
        }
    }
}
