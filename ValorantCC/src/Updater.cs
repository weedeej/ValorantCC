using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Utilities;

namespace EZ_Updater
{
    struct GithubResponse
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("assets")]
        public List<Asset> Assets { get; set; }
    }
    struct Asset
    {
        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }
    static class Updater
    {
        readonly private static string ProgramFileName = AppDomain.CurrentDomain.FriendlyName + ".exe";
        readonly private static string ProgramFileBak = AppDomain.CurrentDomain.FriendlyName + ".bak";
        readonly private static Version ProgramFileVersion = new Version(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).ProductVersion);
        readonly private static string temppath = Path.GetTempPath();
        private static bool AlreadyLog = false;

        private static string DownloadUrl = null;
        private static Version DownloadVersion;
        private static WebClient client = new WebClient();
        private static Action CanceledDownload = null;
        private static Action RetryDownloadAction = null;
        private static Action<object, DownloadProgressChangedEventArgs> DownloadProgressEvent = null;
        private static Action RestartEvent = null;
        private static DispatcherTimer TimerDP = new DispatcherTimer();
        private static int RetryCount = 0;

        static Updater()
        {
            if (File.Exists(ProgramFileBak))
                File.Delete(ProgramFileBak);

            if (File.Exists(temppath + ProgramFileName))
                File.Delete(temppath + ProgramFileName);

            TimerDP.Interval = new TimeSpan(0, 0, 3);
            TimerDP.Tick += RetryDownload;
        }

        public static void CheckUpdate(Window owner)
        {
            Thread UpdaterThread = new Thread(() =>
            {
                if (CheckUpdate())
                    owner.Dispatcher.Invoke((Action)delegate
                    {
                        ((ValorantCC.MainWindow)owner).UpdateMessage();
                    });
            });
            UpdaterThread.Start();
        }

        private static bool CheckUpdate()
        {
            Utils.Log("Checking Github releases");
            Utils.Log($"Current Version: {ProgramFileVersion}");

            try
            {
                string ProgramFile = AppDomain.CurrentDomain.FriendlyName + ".exe";
                string ProgramFileBak = AppDomain.CurrentDomain.FriendlyName + ".bak";

                if (File.Exists(ProgramFileBak)) File.Delete(ProgramFileBak);

                client.Headers = new WebHeaderCollection() { { "User-Agent", "ValorantCC UserAgent" } };
                string ContentString = client.DownloadString("https://api.github.com/repos/weedeej/ValorantCC/releases/latest");
                GithubResponse ResponseObj = JsonConvert.DeserializeObject<GithubResponse>(ContentString);
                Utils.Log($"Latest Release: {ResponseObj.Name} | URL: {ResponseObj.Assets[0].BrowserDownloadUrl}");
                if (new Version(ResponseObj.TagName) > ProgramFileVersion)
                {
                    DownloadUrl = ResponseObj.Assets[0].BrowserDownloadUrl;
                    DownloadVersion = new Version(ResponseObj.TagName);
                    return true;
                }
            }
            catch (Exception e)
            {
                Utils.Log($"Error occured: {e}");
            }
            return false;
        }

        /// <summary>
        /// Updates the application wheter or not there is a new update
        /// </summary>
        /// <param name="CanceledDownloadR">Method u want to execute when download is canceled (Obligatory)</param>
        /// <param name="RetryDownloadR">Method u want to execute when retrying download(Obligatory)</param>
        /// <param name="DownloadProgressEventR">Event method to get DownloadProgressChangedEventArgs</param>
        /// <param name="RestartEventR">Event method to execute code after download had finished (like a application restart to apply update)</param>
        public static void Update(Action CanceledDownloadR = null, Action RetryDownloadR = null, Action<object, DownloadProgressChangedEventArgs> DownloadProgressEventR = null, Action RestartEventR = null)
        {

            if (!AlreadyLog)
            {
                AlreadyLog = true;
                Utils.Log($"Downloading new version | v{ProgramFileVersion.ToString()} -> v{DownloadVersion.ToString()}");
            }

            CanceledDownload = CanceledDownloadR;
            RetryDownloadAction = RetryDownloadR;
            DownloadProgressEvent = DownloadProgressEventR;
            RestartEvent = RestartEventR;

            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(RestartTimer);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedFile);

            if (DownloadProgressEvent != null)
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressEvent);


            client.DownloadFileAsync(new Uri(DownloadUrl), temppath + ProgramFileName);
        }

        private static void RestartTimer(object sender, DownloadProgressChangedEventArgs e)
        {
            RetryCount = 0;
            TimerDP.Stop();
            TimerDP.Interval = new TimeSpan(0, 0, 3);
            TimerDP.Start();
        }

        private static void RetryDownload(object sender, EventArgs e)
        {
            TimerDP.Interval = new TimeSpan(0, 0, 5);
            client.CancelAsync();
            client.Dispose();
            client = new WebClient();
            if (RetryCount >= 4)
            {
                TimerDP.Tick -= RetryDownload;
                TimerDP.Tick += Canceled;
                TimerDP.Interval = new TimeSpan(0, 0, 2);
                return;
            }
            RetryCount++;
            Utils.Log($"Retrying download: {RetryCount + 1}/4");
            Update(CanceledDownload, RetryDownloadAction, DownloadProgressEvent, RestartEvent);

            if (RetryDownloadAction != null)
                RetryDownloadAction();
        }

        private static void Canceled(object sender, EventArgs e)
        {
            TimerDP.Stop();
            client.CancelAsync();
            client.Dispose();

            Utils.Log("Download canceled");

            if (CanceledDownload != null)
                CanceledDownload();
        }

        private static void CompletedFile(object sender, AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                DispatcherTimer TimerSC = new DispatcherTimer
                {
                    Interval = new TimeSpan(0, 0, 1),
                };
                TimerSC.Tick += Restart;
                TimerSC.Start();
            }
        }

        private static void Restart(object sender, EventArgs e)
        {
            File.Move(ProgramFileName, ProgramFileBak);
            Utils.Log("Moved current .exe to .bak");
            File.Move(temppath + ProgramFileName, ProgramFileName);
            Utils.Log("Moved update to current position");
            Utils.Log("Update complete! Restarting.");
            if (RestartEvent != null)
                RestartEvent();
        }
    }
}