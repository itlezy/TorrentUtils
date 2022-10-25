@ECHO OFF

REM load hashes by parsing qBittorrent log files to the existing SQLite DB

CD /D %~dp0

CD /D %G_BIN%\MetadataDownloader

REM MetadataDownloader.exe -c

ECHO Launching MetadataDownloader..
REM Load new hashes to DB
MetadataDownloader.exe -s -d %USERPROFILE%\Downloads

GOTO :EOF



ECHO Done phase TWO

IF %ERRORLEVEL% NEQ 0 (
  ECHO Something went wrong..
  PAUSE
) ELSE (

  REM Update their status based on previously downloaded torrents that might not be in the list just loaded,
  REM   so to avoid downloading torrent file that have already been downloaded previously
  MetadataDownloader.exe -t -d "x:\torr_archived\arc\"
)

ECHO Done phase THREE

ECHO Clean ban words from DB
MetadataDownloader.exe -b

ECHO Print DB Stats
MetadataDownloader.exe -r

ECHO Done ALL

PAUSE
