@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% flard --file dump.bin --addr 0x00000000 --len 0x1000
pause