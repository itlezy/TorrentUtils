@ECHO OFF

CD /D %~dp0

REM ArchiveTorrents.exe -c

ECHO Update torrent status based on previously downloaded torrents
ArchiveTorrents.exe -t -d "%APPDATA%\BiglyBT\active"              -x "*.dat"
ECHO Archive torrens currently processed by BT client, if needed
ArchiveTorrents.exe -s -d "%APPDATA%\BiglyBT\active"              -x "*.dat"
