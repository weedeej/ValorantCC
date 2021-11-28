using EZ_Updater;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;

namespace ValorantCC
{
    public partial class UpdateWindow : Window
    {
        int test = 1;
        string OriginalContent = null;

        public UpdateWindow()
        {
            InitializeComponent();
            OKbtn.Visibility = Visibility.Hidden;
            OriginalContent = (string)Messagelbl.Content;
            Updater.Update(CanceledDownload, RetryDownload, DownloadProgress, Restart);
        }

        private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            Messagelbl.Content = OriginalContent;

            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            test = 1;
        }

        private void RetryDownload()
        {
            Messagelbl.Content = "Retrying download... " + test++ + "/4";
            return;
        }

        private void CanceledDownload()
        {
            Messagelbl.Content = "Download canceled";
            progressBar1.Value = 0;
            OKbtn.Visibility = Visibility.Visible;
        }

        private void Restart()
        {
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Application.Current.Shutdown();
        }

        private void OKbtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
