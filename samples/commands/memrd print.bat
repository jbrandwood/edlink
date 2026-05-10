@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% memrd --addr 0 --len 128 --print
pause