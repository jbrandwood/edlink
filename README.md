# edlink

Cross-platform utility for communication with EverDrive over USB.

Supports all EverDrive PRO and CORE series cartridges.  
A single unified utility is now used for all cartridges.

Works on Windows, Linux and macOS.  
Linux and macOS require installed Mono runtime.

Compared to previous USB utilities, functionality has been significantly expanded, including support for third-party software integration through `stdio`.

Edlink is also a good reference for cartridge communication on the console side.  
Console applications use the same command interface as USB communication, but through a FIFO port mapped into the CPU address space.

If edlink cannot connect to the cartridge, firmware update may be required.

## Contents

| Path | Description |
|---|---|
| `/edlink` | Edlink source code. |
| `/samples` | Command examples and third-party integration examples. |