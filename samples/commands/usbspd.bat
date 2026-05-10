@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% usbspd --addr 0x00000000 --len 0x400000
pause