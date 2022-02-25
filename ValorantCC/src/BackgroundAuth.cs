using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ValorantCC
{
    class BackgroundAuth
    {
        MainWindow main = (MainWindow)Application.Current.MainWindow;
        Processor processor;
        public BackgroundAuth(Processor processor1)
        {
            processor = processor1;
        }
        public async void LoopCheck()
        {
            string LockfilePath = Environment.GetEnvironmentVariable("LocalAppData") + "\\Riot Games\\Riot Client\\Config\\lockfile"; //Copy pasted from Auth.cs because why not?
            bool lockfilexists = false;
            int FlagExistsCount = 0;
            main.ch_display.Visibility = Visibility.Collapsed;
            main.buttons_group.Visibility = Visibility.Collapsed;
            main.controls_group.Visibility = Visibility.Collapsed;
            main.ForceLoginBtn.Visibility = Visibility.Collapsed;
            while (true)
            {
                if (AuthObj.CheckLockFile(LockfilePath) && !lockfilexists)
                {
                    main.StatusTxt.Foreground = Brushes.Yellow;
                    main.StatusTxt.Text = "Waiting for session. . .";
                    lockfilexists = true;
                }
                if (!await LoginFlagExists() && !main.PressedForceLogin)
                {
                    await Task.Delay(1500);
                    FlagExistsCount++;
                    if (FlagExistsCount > 5)
                        main.ForceLoginBtn.Visibility = Visibility.Visible;
                    continue;
                }
                if (lockfilexists && AuthObj.ObtainLockfileData(LockfilePath).Success)
                {
                    main.StatusTxt.Text = "Logging in. . .";
                    AuthResponse AuthResponse = await processor.Login();
                    main.DataProcessor = processor;
                    main.LoggedIn = AuthResponse.Success;
                    if (!main.LoggedIn)
                    {
                        main.StatusTxt.Text = "Failed. Please login to Riot Client or Start Valorant.";
                        main.ForceLoginBtn.Visibility = Visibility.Collapsed;
                        await Task.Delay(1500);
                        continue;
                    }
                    main.profiles.ItemsSource = processor.ProfileNames;
                    main.profiles.SelectedIndex = processor.CurrentProfile;
                    main.profiles.IsReadOnly = false;
                    main.ValCCAPI = new API(AuthResponse.AuthTokens, main.SelectedProfile, 2, (main.chkbxShareable.IsChecked ?? false));

                    main.DotTxt.Foreground = Brushes.Lime;
                    main.StatusTxt.Foreground = Brushes.Lime;
                    Utilities.Utils.MessageText(Utilities.Utils.LoginResponse(processor), Brushes.Lime);
                    main.StatusTxt.Text = "Logged In!";

                    main.UpdateLayout();
                    double OriginalHeight = main.Height;

                    main.ch_display.Visibility = Visibility.Visible;
                    main.buttons_group.Visibility = Visibility.Visible;
                    main.controls_group.Visibility = Visibility.Visible;
                    main.chkbxShareable.Visibility = Visibility.Visible;

                    main.spinner.Visibility = Visibility.Collapsed;
                    main.ForceLoginBtn.Visibility = Visibility.Collapsed;
                    main.spinner.Spin = false;
                    main.UpdateLayout();
                    Trace.WriteLine(main.Height + " || " + OriginalHeight);
                    main.Top = main.Top - (main.Height - OriginalHeight) / 2;

                    return;
                }
                await Task.Delay(500);
            }
        }

        public async static Task<bool> LoginFlagExists()
        {
            DirectoryInfo LogDir = new DirectoryInfo(Environment.GetEnvironmentVariable("LocalAppData") + "\\Riot Games\\Riot Client\\Logs\\Riot Client Logs");
            var log = LogDir.GetFiles().OrderByDescending(f => f.LastWriteTime).First();

            string content;
            using (FileStream fileStream = File.Open(log.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fileStream))
                    content = (String)sr.ReadToEnd().Clone();

            bool t = false;
            if (content.Contains("riot-messaging-service: State is now Connected"))
                t = true;

            await Task.Delay(1);
            return t;
        }
    }
}
