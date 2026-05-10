@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% .link --protocol-id 0x07 --port COM6 devinf
pause