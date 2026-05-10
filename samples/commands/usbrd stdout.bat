@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% .stdio usbrd --file -
pause