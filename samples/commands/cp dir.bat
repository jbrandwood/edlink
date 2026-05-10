@echo off
set USB_TOOL=python edlink.py

%USB_TOOL% cp --src "rtc-cal" --dst "sd:rtc-cal"
pause