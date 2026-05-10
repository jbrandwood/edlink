@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% memwr --addr 0x00000000 --file dump.bin
pause