@ECHO OFF

CD /D %~dp0

ECHO Archiving torrents from Incoming Directory..
ArchiveTorrents.exe
ECHO:

PAUSE

CD /D %~dp0

ECHO Archiving torrents currently processed by BT client - Step 1/2
ArchiveTorrents.exe -s -d "%APPDATA%\BiglyBT\active"              -x "*.dat"
ECHO:

PAUSE

CD /D %~dp0

ECHO Archiving torrents currently processed by BT client - Step 2/2
ArchiveTorrents.exe -k
ECHO:

PAUSE
