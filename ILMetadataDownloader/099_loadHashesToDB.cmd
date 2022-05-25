CD /D %~dp0

CD /D %G_BIN%\MetadataDownloader

REM concatenate qBittorrent log files
type c:\tmp1\qbtmp1\*.log* | find "- handleDHT" >  z_in_log_hashes.txt
type c:\tmp1\*.log* | find "- handleDHT"        >> z_in_log_hashes.txt

MetadataDownloader.exe -c
MetadataDownloader.exe -s -i z_in_log_hashes.txt

PAUSE
