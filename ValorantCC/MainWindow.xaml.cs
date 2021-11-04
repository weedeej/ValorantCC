﻿using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System;
using Utilities;

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
        public MainWindow()
        {
            InitializeComponent();
            string LogFile = Directory.GetCurrentDirectory() + "/logs.txt";
            if (File.Exists(LogFile)) File.Delete(LogFile);
            Utils.Log("App Started. Deleted old logfile.");
            if (Utils.CheckLatest())
            {
                MessageBox.Show("New version has been downloaded!");
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
            if (LoggedIn)
            {
                profiles.ItemsSource = DataProcessor.ProfileNames;

                txt_LoggedIn.Foreground = Brushes.Lime;
                profiles.SelectedIndex = DataProcessor.CurrentProfile;
                profiles.IsReadOnly = false;
                if (DataProcessor.ProfileListed)
                {
                    MessageBox.Show("Logged In! You may now close Valorant.");
                }
                else
                {
                    MessageBox.Show("Logged In! You may now close Valorant. NOTE: You only have 1 Profile. To use the other features, Please create an extra profile.");
                }
                
            }
            else
            {
                MessageBox.Show(AuthResponse.Response);
                return;
            }
            btnLogin.IsEnabled = !LoggedIn;
        }

        private void profiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (profiles.SelectedIndex != -1)
            {
                SelectedIndex = profiles.SelectedIndex;
                SelectedProfile = DataProcessor.ProfileFromIndex(profiles.SelectedIndex);
                
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
                ads_color.SelectedColor = Color.FromRgb(SelectedProfile.aDS.Color.R, SelectedProfile.aDS.Color.G, SelectedProfile.aDS.Color.B);
                ads_outline_color.SelectedColor = Color.FromRgb(SelectedProfile.aDS.OutlineColor.R, SelectedProfile.aDS.OutlineColor.G, SelectedProfile.aDS.OutlineColor.B);
                sniper_dot_color.SelectedColor = Color.FromRgb(SelectedProfile.Sniper.CenterDotColor.R, SelectedProfile.Sniper.CenterDotColor.G, SelectedProfile.Sniper.CenterDotColor.B);
            }
        }
        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
             Environment.Exit(0);
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
            if (LoggedIn)
            {
                DataProcessor.ProfileNames[SelectedIndex] = profiles.Text;
                profiles.Items.Refresh();
                profiles.SelectedIndex = SelectedIndex;
            }
        }

        private void primary_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            
            primeX1.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeX2.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeY1.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
            primeY2.Fill = (Brush)bc.ConvertFrom(primary_color.SelectedColor.ToString());
        }

        private void prim_outline_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            
            primeX1.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeX2.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeY1.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
            primeY2.Stroke = (Brush)bc.ConvertFrom(prim_outline_color.SelectedColor.ToString());
        }

        private void ads_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            
            aDSX1.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            aDSX2.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            aDSY1.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
            aDSY2.Fill = (Brush)bc.ConvertFrom(ads_color.SelectedColor.ToString());
        }

        private void ads_outline_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            
            aDSX1.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            aDSX2.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            aDSY1.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
            aDSY2.Stroke = (Brush)bc.ConvertFrom(ads_outline_color.SelectedColor.ToString());
        }

        private void sniper_dot_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            
            sniperdot.Fill = (Brush)bc.ConvertFrom(sniper_dot_color.SelectedColor.ToString());

        }
    }
}
