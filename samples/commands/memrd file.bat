@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% memrd --addr 0x00000000 --len 0x100 --file dump.bin
pause