using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Diagnostics;
using Utilities;

namespace ValorantCC
{
    class BackgroundAuth
    {
        MainWindow main = (MainWindow) Application.Current.MainWindow;
        Processor processor;
        public BackgroundAuth(Processor processor1)
        {
            processor = processor1;
        }
        public async void LoopCheck()
        {
            string LockfilePath = Environment.GetEnvironmentVariable("LocalAppData") + "\\Riot Games\\Riot Client\\Config\\lockfile"; //Copy pasted from Auth.cs because why not?
            bool lockfilexists = false;
            while (true)
            {
                if (AuthObj.CheckLockFile(LockfilePath) && !lockfilexists)
                {
                    main.StatusTxt.Foreground = Brushes.Yellow;
                    main.StatusTxt.Text = "Waiting for session. . .";
                    lockfilexists = true;
                }
                if (!await LoginFlagExists())
                {
                    await Task.Delay(1500);
                    continue;
                }
                if (lockfilexists && AuthObj.ObtainLockfileData(LockfilePath).Success)
                {
                    main.StatusTxt.Text = "Logging in. . .";
                    processor.Login();
                    AuthResponse AuthResponse = processor.Login();
                    main.DataProcessor = processor;
                    main.LoggedIn = AuthResponse.Success;
                    if (!main.LoggedIn)
                    {
                        main.StatusTxt.Text = "Failed. Please login to Riot Client or Start Valorant.";
                        await Task.Delay(1500);
                        continue;
                    }
                    main.profiles.ItemsSource = processor.ProfileNames;
                    main.profiles.SelectedIndex = processor.CurrentProfile;
                    main.profiles.IsReadOnly = false;
                    main.ValCCAPI = new API(AuthResponse.AuthTokens, main.SelectedProfile, 2, (main.chkbxShareable.IsChecked ?? false));

                    main.DotTxt.Foreground = Brushes.Lime;
                    main.StatusTxt.Foreground = Brushes.Lime;
                    main.MessageTxt.Foreground = Brushes.Lime;
                    main.MessageTxt.Text = Utils.LoginResponse(processor);
                    main.StatusTxt.Text = "Logged In!";
                    return;
                }
                await Task.Delay(1500);
            }
        }

        public async static Task<bool> LoginFlagExists()
        {
            String LogDir = Environment.GetEnvironmentVariable("LocalAppData") + "\\Riot Games\\Riot Client\\Logs\\Riot Client Logs";
            String[] logs = Directory.GetFiles(LogDir, "*.log");
            string content;
            using (FileStream fileStream = File.Open(logs.Last(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    content = sr.ReadToEnd();
                }
            }
            bool t = false;
            if (content.Contains("riot-messaging-service: State is now Connected"))
            {
                t = true;
            }
            await Task.Delay(1);
            return t;
        }
    }
}
