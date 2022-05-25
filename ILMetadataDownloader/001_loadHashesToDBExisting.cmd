CD /D %~dp0

CD /D %G_BIN%\MetadataDownloader

REM concatenate qBittorrent log files
rem type c:\tmp1\qbtmp1\*.log* | find "- handleDHT" >  z_in_log_hashes.txt
type c:\tmp1\*.log* | find "- handleDHT"         > z_in_log_hashes.txt

rem MetadataDownloader.exe -t -d c:\tmp_out
rem MetadataDownloader.exe -t -d x:\torr_archived


REM Load new hashes to DB
MetadataDownloader.exe -s -i z_in_log_hashes.txt

REM Update their status based on previously downloaded torrents that might not be in the list just loaded
MetadataDownloader.exe -t -d x:\torr_archived

PAUSE
