@ECHO OFF

CD /D %~dp0

ArchiveTorrents.exe

PAUSE

CD /D %~dp0

ECHO Archive torrens currently processed by BT client, if needed
ArchiveTorrents.exe -s -d "%APPDATA%\BiglyBT\active"              -x "*.dat"

PAUSE
