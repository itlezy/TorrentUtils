@ECHO OFF

REM load hashes by parsing qBittorrent log files to the existing SQLite DB

CD /D %~dp0

REM ArchiveTorrents.exe -c

ECHO Update torrent status based on previously downloaded torrents
ArchiveTorrents.exe -t -d x:\torr_archived
ArchiveTorrents.exe -t -d "%LOCALAPPDATA%\qBittorrent\BT_Backup"

ECHO Update downloaded files status based on previously downloaded files
ArchiveTorrents.exe -f -d x:\torr_OK
ArchiveTorrents.exe -f -d x:\zprn\zzprn_SELECT
ArchiveTorrents.exe -f -d u:\zprn
