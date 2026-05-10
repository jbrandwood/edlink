@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% .link --dev-id 0x27 --port COM6 devinf
pause