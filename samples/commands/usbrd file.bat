@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% usbrd --file usbrd.txt
pause