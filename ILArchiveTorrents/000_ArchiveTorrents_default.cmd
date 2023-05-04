@ECHO OFF

CD /D %~dp0

COPY   /Y ArchiveTorrents.exe.config.my ArchiveTorrents.exe.config
ATTRIB -R ArchiveTorrents.exe.config

ECHO Archiving torrents from Incoming Directory.. (skip copy)
ArchiveTorrents.exe -k
ECHO:

SET /P VPROCEED="Proceed copying from BT dir [y/n] "

IF "%VPROCEED%" == "y" (

    CD /D %~dp0

    ECHO Archiving torrents currently processed by BT client - Step 1/2 (copies to input dir actually)
    ArchiveTorrents.exe -s -d "%APPDATA%\BiglyBT\active"              -x "*.dat"
    ECHO:

    PAUSE

    CD /D %~dp0

    ECHO Archiving torrents currently processed by BT client - Step 2/2 (skip copy to archive dir)
    ArchiveTorrents.exe -k
    ECHO:

    PAUSE

)
