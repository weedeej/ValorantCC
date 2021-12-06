using EZ_Updater;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;

namespace ValorantCC
{
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            InitializeComponent();
            OKbtn.Visibility = Visibility.Hidden;
            Updater.Update(UIChange);
        }

        private void UIChange(object sender, EventArgs e)
        {
            Messagelbl.Content = Updater.Message;
            progressBar1.Value = Updater.ProgressPercentage;

            switch (Updater.State)
            {
                case UpdaterState.Canceled:
                case UpdaterState.InstallFailed:
                    OKbtn.Visibility = Visibility.Visible;
                    break;
                case UpdaterState.Installed:
                    Process.Start(Updater.ProgramFileName);
                    Application.Current.Shutdown();
                    break;
            }
        }

        private void OKbtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
