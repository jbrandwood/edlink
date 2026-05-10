@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% cp --src "cp file.bat" --dst "sd:cp file.bat"
pause