using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utilities;
namespace ValorantCC
{
    public partial class MainWindow : Window
    {
        Processor DataProcessor = new Processor();
        bool LoggedIn;
        CrosshairProfile SelectedProfile;
        Color SelectedColor;
        public MainWindow()
        {
            InitializeComponent();
            string LogFile = Directory.GetCurrentDirectory() + "/logs.txt";
            if (File.Exists(LogFile)) File.Delete(LogFile);
            Utils.Log("App Started. Deleted old logfile.");
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!LoggedIn)
            {
                MessageBox.Show("You are not logged in!");
                return;
            }
            if (DataProcessor.SaveNewColor(SelectedColor, profiles.SelectedIndex))
            {
                MessageBox.Show("Saved! If Valorant is open, Please restart it without touching the settings.");
                return;
            }
            MessageBox.Show("Failed. Consult the developer.");
            return;
        }

        private void colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var bc = new BrushConverter();
            lineX1.Stroke = (Brush)bc.ConvertFrom(colorpicker.SelectedColor.ToString());
            lineX2.Stroke = (Brush)bc.ConvertFrom(colorpicker.SelectedColor.ToString());
            lineY1.Stroke = (Brush)bc.ConvertFrom(colorpicker.SelectedColor.ToString());
            lineY2.Stroke = (Brush)bc.ConvertFrom(colorpicker.SelectedColor.ToString());

            txt_CurrentColor.Foreground = (Brush)bc.ConvertFrom(colorpicker.SelectedColor.ToString());

            SelectedColor = (Color)colorpicker.SelectedColor;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            AuthResponse AuthResponse = DataProcessor.Login();
            LoggedIn = AuthResponse.Success;
            if (LoggedIn)
            {
                profiles.ItemsSource = DataProcessor.ProfileNames;

                txt_LoggedIn.Foreground = Brushes.Lime;
                txt_ProfileCount.Foreground = Brushes.Lime;
                txt_ProfileCount.Text = DataProcessor.ProfileNames.Count.ToString();
                MessageBox.Show("Logged In! You may now close Valorant.");
            }
            else
            {
                MessageBox.Show(AuthResponse.Response);
            }
            btnLogin.IsEnabled = !LoggedIn;
        }

        private void profiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedProfile = DataProcessor.ProfileFromIndex(profiles.SelectedIndex);
            colorpicker.SelectedColor = Color.FromRgb(SelectedProfile.Primary.Color.R, SelectedProfile.Primary.Color.G, SelectedProfile.Primary.Color.B);
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
            Close();
        }
    }
}
