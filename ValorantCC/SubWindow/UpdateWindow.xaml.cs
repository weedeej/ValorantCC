using EZ_Updater;
using System;
using System.Diagnostics;
using System.Windows;

namespace ValorantCC
{
    public partial class UpdateWindow : Window
    {
        private bool _OKpressed = false;

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

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateLayout();
            Top = Top + 14;
        }

        private void OKbtn_Click(object sender, RoutedEventArgs e)
        {
            _OKpressed = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_OKpressed)
                e.Cancel = true;
        }
    }
}
