using EZ_Updater;
using System;
using System.Diagnostics;
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

            switch (Updater.ShortState)
            {
                case UpdaterShortState.Canceled:
                    OKbtn.Visibility = Visibility.Visible;
                    break;
                case UpdaterShortState.Installed:
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
