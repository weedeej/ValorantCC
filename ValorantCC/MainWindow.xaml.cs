using EZ_Updater;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Utilities;
using ValorantCC.src;

namespace ValorantCC
{
    public partial class MainWindow : Window
    {
        Processor DataProcessor = new Processor();
        bool LoggedIn;
        CrosshairProfile SelectedProfile;
        List<Color> SelectedColors;
        int SelectedIndex;
        BrushConverter bc = new BrushConverter();
        readonly string LoggingDir = Environment.GetEnvironmentVariable("LocalAppData") + @"\VTools\Logs\";
        public MainWindow()
        {
            // Create logging dir
            if (!Directory.Exists(LoggingDir)) Directory.CreateDirectory(LoggingDir);
            // Replace old logs 
            string LogFile = LoggingDir + "/logs.txt";
            if (File.Exists(LogFile)) File.Move(LogFile, LogFile + ".old", true);
            Version ProgramFileVersion = new Version(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).ProductVersion);

            InitializeComponent();
            Utils.Log($"App Started | v{ProgramFileVersion}. Replaced old logfile.");
            Txt_CurrVer.Content = $"v{ProgramFileVersion}";
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Updater.CustomLogger = Utils.Log;

            if (await Updater.CheckUpdateAsync("weedeej", "ValorantCC"))
            {
                var update = new UpdateWindow();
                update.Owner = this;
                update.ShowDialog();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!LoggedIn)
            {
                MessageBox.Show("You are not logged in!");
                return;
            }
            if (DataProcessor.ProfileListed)
            {
                SelectedColors = new List<Color> { (Color)primary_color.SelectedColor, (Color)prim_outline_color.SelectedColor, (Color)ads_color.SelectedColor, (Color)ads_outline_color.SelectedColor, (Color)sniper_dot_color.SelectedColor };
            }
            else
            {
                SelectedColors = new List<Color> { (Color)primary_color.SelectedColor };
            }
            if (DataProcessor.SaveNewColor(SelectedColors, profiles.SelectedIndex, profiles.Text))
            {
                DataProcessor.Construct();
                profiles.Items.Refresh();
                profiles.SelectedIndex = DataProcessor.CurrentProfile;
                MessageBox.Show("Saved! If Valorant is open, Please restart it without touching the settings.");
                return;
            }
            MessageBox.Show("Failed. Consult the developer.");
            return;
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            AuthResponse AuthResponse = DataProcessor.Login();
            LoggedIn = AuthResponse.Success;
            if (!LoggedIn)
            {
                MessageBox.Show(AuthResponse.Response);
                return;
            }

            profiles.ItemsSource = DataProcessor.ProfileNames;
            txt_LoggedIn.Foreground = Brushes.Lime;
            profiles.SelectedIndex = DataProcessor.CurrentProfile;
            profiles.IsReadOnly = false;
            MessageBox.Show(Utils.LoginResponse(DataProcessor));
            btnLogin.IsEnabled = true;
        }

        private void profiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (profiles.SelectedIndex == -1) return;

            SelectedIndex = profiles.SelectedIndex;
            SelectedProfile = DataProcessor.ProfileFromIndex(SelectedIndex);

            primary_color.SelectedColor = Color.FromRgb(SelectedProfile.Primary.Color.R, SelectedProfile.Primary.Color.G, SelectedProfile.Primary.Color.B);
            if (!DataProcessor.ProfileListed)
            {
                prim_outline_color.IsEnabled = false;
                ads_color.IsEnabled = false;
                ads_outline_color.IsEnabled = false;
                sniper_dot_color.IsEnabled = false;
                return;
            }
            prim_outline_color.SelectedColor = Color.FromRgb(SelectedProfile.Primary.OutlineColor.R, SelectedProfile.Primary.OutlineColor.G, SelectedProfile.Primary.OutlineColor.B);
            if (SelectedProfile.aDS == null) SelectedProfile.aDS = SelectedProfile.Primary;
            if(SelectedProfile.bUsePrimaryCrosshairForADS) SelectedProfile.aDS.Color = SelectedProfile.Primary.Color;
            ads_color.SelectedColor = Color.FromRgb(SelectedProfile.aDS.Color.R, SelectedProfile.aDS.Color.G, SelectedProfile.aDS.Color.B);
            ads_outline_color.SelectedColor = Color.FromRgb(SelectedProfile.aDS.OutlineColor.R, SelectedProfile.aDS.OutlineColor.G, SelectedProfile.aDS.OutlineColor.B);
            sniper_dot_color.SelectedColor = Color.FromRgb(SelectedProfile.Sniper.CenterDotColor.R, SelectedProfile.Sniper.CenterDotColor.G, SelectedProfile.Sniper.CenterDotColor.B);

            Crosshair_load();
        }
        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            if (!LoggedIn)
            {
                MessageBox.Show("You are not logged in!");
                return;
            }
            Utils.Log("Reload Clicked > Reconstructing Processor.");
            DataProcessor.Construct();
            profiles.ItemsSource = DataProcessor.ProfileNames;
            profiles.Items.Refresh();
            profiles.SelectedIndex = DataProcessor.CurrentProfile;
        }

        private void profiles_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!LoggedIn) return;
            DataProcessor.ProfileNames[SelectedIndex] = profiles.Text;
            profiles.Items.Refresh();
            profiles.SelectedIndex = SelectedIndex;
        }

        private void Crosshair_load()
        {
            //Primary
            Crosshair_Parser.dot_redraw(primeDOT, primeDOTOT, SelectedProfile.Primary);
            Crosshair_Parser.rectangle_redraw(primeX1, primeX1OT, Crosshair_Parser.Position.East, SelectedProfile.Primary);
            Crosshair_Parser.rectangle_redraw(primeX2, primeX2OT, Crosshair_Parser.Position.West, SelectedProfile.Primary);
            Crosshair_Parser.rectangle_redraw(primeY1, primeY1OT, Crosshair_Parser.Position.North, SelectedProfile.Primary);
            Crosshair_Parser.rectangle_redraw(primeY2, primeY2OT, Crosshair_Parser.Position.South, SelectedProfile.Primary);

            Crosshair_Parser.rectangle_redraw(primeOLX1, primeOLX1OT, Crosshair_Parser.Position.East, SelectedProfile.Primary);
            Crosshair_Parser.rectangle_redraw(primeOLX2, primeOLX2OT, Crosshair_Parser.Position.West, SelectedProfile.Primary);
            Crosshair_Parser.rectangle_redraw(primeOLY1, primeOLY1OT, Crosshair_Parser.Position.North, SelectedProfile.Primary);
            Crosshair_Parser.rectangle_redraw(primeOLY2, primeOLY2OT, Crosshair_Parser.Position.South, SelectedProfile.Primary);

            //ADS
            var cross = SelectedProfile.Primary;
            if (!SelectedProfile.bUsePrimaryCrosshairForADS)
                cross = SelectedProfile.aDS;

            Crosshair_Parser.dot_redraw(adsDOT, adsDOTOT, cross);
            Crosshair_Parser.rectangle_redraw(adsX1, adsX1OT, Crosshair_Parser.Position.East, cross);
            Crosshair_Parser.rectangle_redraw(adsX2, adsX2OT, Crosshair_Parser.Position.West, cross);
            Crosshair_Parser.rectangle_redraw(adsY1, adsY1OT, Crosshair_Parser.Position.North, cross);
            Crosshair_Parser.rectangle_redraw(adsY2, adsY2OT, Crosshair_Parser.Position.South, cross);

            Crosshair_Parser.rectangle_redraw(adsOLX1, adsOLX1OT, Crosshair_Parser.Position.East, cross);
            Crosshair_Parser.rectangle_redraw(adsOLX2, adsOLX2OT, Crosshair_Parser.Position.West, cross);
            Crosshair_Parser.rectangle_redraw(adsOLY1, adsOLY1OT, Crosshair_Parser.Position.North, cross);
            Crosshair_Parser.rectangle_redraw(adsOLY2, adsOLY2OT, Crosshair_Parser.Position.South, cross);

            //Sniper
            Crosshair_Parser.dot_redraw(sniperdot, SelectedProfile.Sniper);
        }

        private void primary_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            primeDOT.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeX1.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeX2.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeY1.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeY2.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());

            primeOLX1.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeOLX2.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeOLY1.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeOLY2.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
        }

        private void prim_outline_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            primeDOTOT.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeX1OT.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeX2OT.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeY1OT.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeY2OT.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());

            primeOLX1OT.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeOLX2OT.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeOLY1OT.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeOLY2OT.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
        }

        private void ads_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            adsDOT.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            adsX1.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            adsX2.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            adsY1.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            adsY2.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());

            adsOLX1.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            adsOLX2.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            adsOLY1.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            adsOLY2.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
        }

        private void ads_outline_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            adsDOTOT.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            adsX1OT.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            adsX2OT.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            adsY1OT.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            adsY2OT.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());

            adsOLX1OT.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            adsOLX2OT.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            adsOLY1OT.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            adsOLY2OT.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
        }

        private void sniper_dot_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            sniperdot.Fill = (Brush)bc.ConvertFrom(sniper_dot_color.SelectedColor.ToString());
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TabButtonEnter(object sender, MouseEventArgs e)
        {
            ((Button)sender).Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void TabButtonLeave(object sender, MouseEventArgs e)
        {
            ((Button)sender).Foreground = new SolidColorBrush(Colors.White);
        }

        private void ClipboardButtonEnter(object sender, MouseEventArgs e)
        {
            CopyButtonLabel.FontSize += 1;
        }
        private void ClipboardButtonLeave(object sender, MouseEventArgs e)
        {
            CopyButtonLabel.FontSize -= 1;
        }

        private void btnOpenLogs_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo() { FileName = LoggingDir, UseShellExecute = true };
            p.Start();
            MessageBox.Show("Log folder opened. Please include the OLD file on your report as this helps us recreate the bug/error you will report.");
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            CrosshairBG.Source = Get_CrosshairBG();
        }

        private void previos_Click(object sender, RoutedEventArgs e)
        {
            CrosshairBG.Source = Get_CrosshairBG(false);
        }

        private ImageSource Get_CrosshairBG(bool next = true)
        {
            int number = int.Parse(System.IO.Path.GetFileNameWithoutExtension(CrosshairBG.Source.ToString()).Replace("CrosshairBG", "")) + (next ? 1 : -1);

            if (next && number > Resources.Count - 1)
                number = 0;
            else if (number < 0)
                number = Resources.Count - 1;

            return (ImageSource)FindResource("crosshairBG" + number);
        }
    }
}
