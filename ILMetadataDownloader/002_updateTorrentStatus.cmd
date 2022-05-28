@ECHO OFF

REM load hashes by parsing qBittorrent log files to the existing SQLite DB

CD /D %~dp0

CD /D %G_BIN%\MetadataDownloader

REM Update their status based on previously downloaded torrents that might not be in the list just loaded,
REM   so to avoid downloading torrent file that have already been downloaded previously
MetadataDownloader.exe -t -d "x:\torr_archived"
MetadataDownloader.exe -t -d "%LOCALAPPDATA%\qBittorrent\BT_Backup"
