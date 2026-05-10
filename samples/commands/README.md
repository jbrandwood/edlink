# Command Examples

| Path | Description |
|---|---|
| `edlink.py` | Cross-platform launcher script for `edlink`. |
| `edlink.bat` | Runs `edlink` without arguments and shows available commands list. |
| `.link.bat` | Connects only to selected USB port and cartridge family (`protocol-id`) instead of automatic device scanning.
| `cp dir.bat` | Copies `rtc-cal` folder to cartridge SD card. |
| `cp file.bat` | Copies a file to cartridge SD card. |
| `devinf.bat` | Prints cartridge information and saves it to `inf.txt`. |
| `diag.bat` | Runs basic cartridge hardware diagnostics. |
| `flard.bat` | Reads 4KB from cartridge system flash memory and saves it to `dump.bin`. |
| `memrd file.bat` | Reads 256 bytes from cartridge ROM memory and saves them to `dump.bin`. |
| `memrd print.bat` | Reads 128 bytes from cartridge ROM memory and prints them as hexadecimal dump. |
| `memrd stdout.bat` | Reads 256 bytes from cartridge ROM memory and sends them to `stdout`. |
| `netgate.bat` | Writes `dump.bin` into cartridge ROM memory. |
| `reset.bat` | Resets the console. |
| `screen.bat` | Captures screenshot from cartridge menu. |
| `usbrd file.bat` | Reads USB data transmitted by application running on the console and saves it to `usbrd.txt`. |
| `usbrd print.bat` | Reads USB data transmitted by application running on the console and prints it to the screen. |
| `usbspd.bat` | USB transfer speed test. |
| `/rtc-cal` | Set of commands for RTC crystal calibration. |