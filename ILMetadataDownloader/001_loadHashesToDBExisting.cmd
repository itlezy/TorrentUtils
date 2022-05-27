REM load hashes by parsing qBittorrent log files to the existing SQLite DB

CD /D %~dp0

CD /D %G_BIN%\MetadataDownloader

REM concatenate qBittorrent log files
find "- handleDHT" c:\tmp1\qb*.log* > z_in_log_hashes.txt

REM Load new hashes to DB
MetadataDownloader.exe -s -i z_in_log_hashes.txt

CALL 002_updateTorrentStatus.cmd

PAUSE
