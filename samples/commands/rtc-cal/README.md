# RTC Calibration

| Path | Description |
|---|---|
| `0 rtc-cal abort.bat` | Cancels calibration process and sets current time. |
| `1 rtc-cal start.bat` | Starts RTC calibration process. |
| `2 rtc-cal end.bat` | Finishes calibration process. Should be executed 10–20 hours after running `1 rtc-cal start.bat`. |
| `3 rtc-cal get cal.bat` | Shows current calibration value. |
| `4 rtc-cal get cal esti.bat` | Shows estimated calibration value. Available only during phase 1. |
| `5 rtc-cal get devi.bat` | Shows RTC clock drift. |