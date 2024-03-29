﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ILMagnetToURI
{
    public partial class MainForm : Form
    {
        public MainForm ()
        {
            InitializeComponent ();
        }

        private void btnParse_Click (object sender, EventArgs e)
        {
            ParseURLs ();
        }

        private void btnOpen_Click (object sender, EventArgs e)
        {
            OpenInBrowser ();
        }


        private void ParseURLs ()
        {
            txtHashIds.Clear ();

            foreach (var url in txtURLs.Lines) {
                MonoTorrent.MagnetLink u;

                if (MonoTorrent.MagnetLink.TryParse (url, out u)) {
                    txtHashIds.Text +=
                        u.InfoHashes.V1.ToHex ()
                        + Environment.NewLine;
                }

            }
        }

        private void OpenInBrowser ()
        {
            var baseURLs = new string[] {
            "https://itorrents.org/torrent/{0}.torrent",
            "https://torrage.info/torrent.php?h={0}",
            "https://btcache.me/torrent/{0}"
            };

            if (string.IsNullOrWhiteSpace (txtHashIds.Text)) {
                ParseURLs ();
            }

            if (!string.IsNullOrWhiteSpace (txtHashIds.Text)) {
                foreach (var hashId in txtHashIds.Lines) {
                    foreach (var baseURL in baseURLs) {
                        if (!string.IsNullOrWhiteSpace (hashId)) {

                            Process.Start (
                                new ProcessStartInfo {
                                    UseShellExecute = true,
                                    FileName = string.Format (baseURL, hashId)
                                });

                        }
                    }
                }
            }

            Clear ();
        }

        private void timerClipMonitor_Tick (object sender, EventArgs e)
        {
            var c = Clipboard.GetText ();

            if (string.IsNullOrWhiteSpace (c))
                return;

            if (c.IndexOf ("magnet:") >= 0) {
                c = Regex.Replace (c.Trim (), "^\"|\"$", "");

                MonoTorrent.MagnetLink u;

                if (MonoTorrent.MagnetLink.TryParse (c, out u)) {

                    if (!Array.Exists (
                            txtURLs.Lines,
                            m => m.IndexOf (u.InfoHashes.V1.ToHex (), StringComparison.InvariantCultureIgnoreCase) >= 0
                        )) {

                        if (!string.IsNullOrWhiteSpace (txtURLs.Text))
                            txtURLs.Text += Environment.NewLine;

                        txtURLs.Text += c;
                    }

                }

                ParseURLs ();
            }

            if (ckAutoProcess.Checked)
                OpenInBrowser ();
        }

        private void btnClear_Click (object sender, EventArgs e)
        {
            Clear ();
        }

        private void Clear ()
        {
            txtURLs.Clear ();
            txtHashIds.Clear ();

            var c = Clipboard.GetText ();

            if (c.IndexOf ("magnet:") >= 0)
                Clipboard.Clear ();
        }

        private void btnExit_Click (object sender, EventArgs e)
        {
            Application.Exit ();
        }

        private void btnRarBG_Click (object sender, EventArgs e)
        {
            var search = HttpUtility.UrlEncode (Clipboard.GetText ());

            Process.Start (
                 new ProcessStartInfo {
                     UseShellExecute = true,
                     FileName = "https://rarbgmirror.org/torrents.php?search=" + search + "&category%%5B%%5D=4&"
                 });
        }
    }
}
