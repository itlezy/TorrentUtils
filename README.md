# MetadataDownloader
A tool to download Torrent metadata from info-hashes that have been captured by listening to the DHT, using MonoTorrent. Similar results can achieved with a number of tools, such as Dodder or https://github.com/the8472/mldht

## Getting the Hashes
There are many tools out there to log DHT hashes, in my case here I get them from a patched version of qBittorrent that logs the hashes.

![image](https://user-images.githubusercontent.com/24484050/188945664-f698ff03-2da0-481b-8472-a68e2213fb05.png)

## Usage
The code should provide enough insights in how to use the tool. A normal sequence of steps would be in the script here `001_loadHashesToDBExisting.cmd`

# ArchiveTorrents
The aim of this tool, as companion to the MetadataDownloader, is to copy downloaded torrents to qBittorent's watch folder, avoid duplicates, and archive torrents for latter usage. The tool keeps a SQLLite DB of downloaded torrents and files, so to identify already downloaded files as well. See the code and `001_ArchiveTorrents.cmd` for usage.
