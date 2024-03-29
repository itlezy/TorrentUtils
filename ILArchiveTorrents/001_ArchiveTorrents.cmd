@ECHO OFF

REM load hashes by parsing qBittorrent log files to the existing SQLite DB

CD /D %~dp0

REM ArchiveTorrents.exe -c

ECHO Update torrent status based on previously downloaded torrents
ArchiveTorrents.exe -t -d "m:\dldz\TORR_ARCHIVED\arc"
ArchiveTorrents.exe -t -d "%LOCALAPPDATA%\qBittorrent\BT_Backup"
ArchiveTorrents.exe -t -d "%APPDATA%\BiglyBT\active"              -x "*.dat"

ECHO Update downloaded files status based on previously downloaded files
ArchiveTorrents.exe -f -d x:\torr_OK
ArchiveTorrents.exe -f -d x:\zprn\zzprn_SELECT
ArchiveTorrents.exe -f -d u:\zprn
