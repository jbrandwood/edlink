@echo off
set USB_TOOL=python ../edlink.py

%USB_TOOL% rtccal --cmd 3
pause
