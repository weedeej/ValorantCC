using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ValorantCC
{
    /// <summary>
    /// Lógica de interacción para UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        string OriginalContent = null;
        public UpdateWindow()
        {
            InitializeComponent();
            OKbtn.Visibility = Visibility.Hidden;
            OriginalContent = (string)Messagelbl.Content;
            EZ_Updater.Update(CanceledDownload, RetryDownload, DownloadProgress, Restart);
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
        int test = 1;
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
