@ECHO OFF

REM load hashes by parsing qBittorrent log files to the existing SQLite DB

CD /D %~dp0

CD /D %G_BIN%\MetadataDownloader

SET BT_LOGS_DIR=c:\tmp1

ECHO Find and concatenate..
REM concatenate qBittorrent log files
find "- handleDHT" %BT_LOGS_DIR%\qbittorrent.log* > z_in_log_hashes.txt

ECHO Done phase ONE

ECHO Launching MetadataDownloader..
REM Load new hashes to DB
MetadataDownloader.exe -s -i z_in_log_hashes.txt

ECHO Done phase TWO

IF %ERRORLEVEL% NEQ 0 (
  ECHO Something went wrong..
  PAUSE
) ELSE (
  DEL /Q %BT_LOGS_DIR%\qbittorrent.log.bak*

  REM Update their status based on previously downloaded torrents that might not be in the list just loaded,
  REM   so to avoid downloading torrent file that have already been downloaded previously
  MetadataDownloader.exe -t -d "x:\torr_archived"
  MetadataDownloader.exe -t -d "%LOCALAPPDATA%\qBittorrent\BT_Backup"
)

ECHO Done phase THREE

ECHO Clean ban words from DB
MetadataDownloader.exe -b

ECHO Print DB Stats
MetadataDownloader.exe -r

ECHO Done ALL

PAUSE
