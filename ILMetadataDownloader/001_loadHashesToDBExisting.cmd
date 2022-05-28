@ECHO OFF

REM load hashes by parsing qBittorrent log files to the existing SQLite DB

CD /D %~dp0

CD /D %G_BIN%\MetadataDownloader

SET BT_LOGS_DIR=c:\tmp1

REM concatenate qBittorrent log files
find "- handleDHT" %BT_LOGS_DIR%\qbittorrent.log* > z_in_log_hashes.txt

REM Load new hashes to DB
MetadataDownloader.exe -s -i z_in_log_hashes.txt

IF %ERRORLEVEL% NEQ 0 (
  ECHO Something went wrong..
) ELSE (
  DEL /Q %BT_LOGS_DIR%\qbittorrent.log.bak*
  CALL 002_updateTorrentStatus.cmd
)

REM Clean ban words from DB
MetadataDownloader.exe -b

REM Print DB Stats
MetadataDownloader.exe -r

PAUSE
