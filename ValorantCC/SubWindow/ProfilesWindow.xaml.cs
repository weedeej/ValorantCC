using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using ValorantCC.src;

namespace ValorantCC
{
    /// <summary>
    /// Interaction logic for ProfilesWindow.xaml
    /// </summary>
    
    public struct PublicProfile
    {
        public String owner { get; set; }
        public String sharecode { get; set; }
        public CrosshairProfile settings { get; set; }
        public int ID { get; set; }
    }

    public partial class ProfilesWindow : Window
    {
        public CrosshairProfile selected;
        private BrushConverter bc;
        private static API ValCCApi;
        private static MainWindow main;
        private static List<PublicProfile> PublicProfiles = new List<PublicProfile>();
        public ProfilesWindow(CrosshairProfile current, API ValCCAPI)
        {
            InitializeComponent();
            main = (MainWindow) Application.Current.MainWindow;
            selected = current;
            ValCCApi = ValCCAPI;
            bc = new BrushConverter();
        }

        private async void Border_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingTxt.Visibility = Visibility.Visible;
            await InitialFetch();
            LoadingTxt.Visibility = Visibility.Collapsed;
            await RenderProfiles();
        }

        /// <summary>
        /// Returns the searched sharecode or the list of shareables depending if sharecode is provided or not.
        /// </summary>
        /// <param name="sharecode">Nullable. Searched Code</param>
        private static async Task<bool> InitialFetch(String sharecode = null)
        {
            PublicProfiles.Clear();
            List<ShareableProfile> Shareables;
            if (sharecode != null)
            {
                ValCCApi.Action = 3;
                Shareables = (await ValCCApi.Fetch(sharecode)).data;
            }
            else
            {
                ValCCApi.Action = 2;
                Shareables = (await ValCCApi.Fetch()).data;

            }

            if (Shareables.Count == 0) return true;
            for (int i = 0; i < Shareables.Count; i++)
            {
                ShareableProfile currentShareable = Shareables[i];
                CrosshairProfile profile;
                try
                {
                    profile = JsonConvert.DeserializeObject<CrosshairProfile>(Regex.Unescape(currentShareable.settings));
                }
                catch
                {
                    continue;
                }
                PublicProfiles.Add(new PublicProfile() { owner = currentShareable.displayName, settings = profile, sharecode = currentShareable.shareCode, ID = i });
            }

            //Change this delay for the async backend
            await Task.Delay(1);
            return true;
        }

        /// <summary>
        /// Render the PublicProfiles var into frontend.
        /// </summary>
        private async Task<bool> RenderProfiles()
        {
            UIElementCollection shareablesElement = ShareablesContainer.Children;
            if (PublicProfiles.Count == 0) return true;
            
            for (int i = 0; i < PublicProfiles.Count; i++)
            {
                PublicProfile profile = PublicProfiles[i];
                shareablesElement.Add(await CreateRender(profile));
            }
            await Task.Delay(1);
            return true;
        }
        private async void btnSearchCode_Click(object sender, RoutedEventArgs e)
        {
            if (SearchCode.Text == null || SearchCode.Text == "") return;
            await InitialFetch(SearchCode.Text);
            await RenderProfiles();
        }

        public void shareBtnClicked(object sender, RoutedEventArgs e)
        {
            String shareCode = ((Button)sender).Name.Split('_')[1];
            Clipboard.SetText(shareCode);
            MessageBox.Show("\"" + shareCode + "\" has been copied to clipboard!");
        }

        public void detailsBtnClicked(object sender, RoutedEventArgs e)
        {
            PublicProfile pressedPubProfile = PublicProfiles[Int32.Parse(((Button)sender).Name.Split('_')[1])];
            CrosshairProfile pressedProfile = pressedPubProfile.settings;
            MessageBox.Show($"{pressedPubProfile.owner}'s Profile: {pressedProfile.ProfileName}\nPrimary:\n\tInner Lines: {pressedProfile.Primary.InnerLines.Opacity}, {pressedProfile.Primary.InnerLines.LineLength}, {pressedProfile.Primary.InnerLines.LineThickness}, {pressedProfile.Primary.InnerLines.LineOffset}\n\tOuter Lines: {pressedProfile.Primary.OuterLines.Opacity}, {pressedProfile.Primary.OuterLines.LineLength}, {pressedProfile.Primary.OuterLines.LineThickness}, {pressedProfile.Primary.OuterLines.LineOffset}\n\nADS:\n\tInner Lines: {pressedProfile.aDS.InnerLines.Opacity}, {pressedProfile.aDS.InnerLines.LineLength}, {pressedProfile.aDS.InnerLines.LineThickness}, {pressedProfile.aDS.InnerLines.LineOffset}\n\tOuter Lines: {pressedProfile.aDS.OuterLines.Opacity}, {pressedProfile.aDS.OuterLines.LineLength}, {pressedProfile.aDS.OuterLines.LineThickness}, {pressedProfile.aDS.OuterLines.LineOffset}");
        }

        public void applyBtnClicked(object sender, RoutedEventArgs e)
        {
            PublicProfile pressedPubProfile = PublicProfiles[Int32.Parse(((Button)sender).Name.Split('_')[1])];
            CrosshairProfile pressedProfile = pressedPubProfile.settings;
            selected.aDS = pressedProfile.aDS;
            selected.Primary = pressedProfile.Primary;
            selected.Sniper = pressedProfile.Sniper;
            selected.bUseAdvancedOptions = pressedProfile.bUseAdvancedOptions;
            selected.bUseCustomCrosshairOnAllPrimary = pressedProfile.bUseCustomCrosshairOnAllPrimary;
            selected.bUsePrimaryCrosshairForADS = pressedProfile.bUsePrimaryCrosshairForADS;

            main.primary_color.SelectedColor = new Color() { R = selected.Primary.Color.R, G = selected.Primary.Color.G, B = selected.Primary.Color.B, A = selected.Primary.Color.A };
            main.ads_color.SelectedColor = new Color() { R = selected.aDS.Color.R, G = selected.aDS.Color.G, B = selected.aDS.Color.B, A = selected.aDS.Color.A };

            main.prim_outline_color.SelectedColor = new Color() { R = selected.Primary.OutlineColor.R, G = selected.Primary.OutlineColor.G, B = selected.Primary.OutlineColor.B, A = selected.Primary.OutlineColor.A };
            main.ads_outline_color.SelectedColor = new Color() { R = selected.aDS.OutlineColor.R, G = selected.aDS.OutlineColor.G, B = selected.aDS.OutlineColor.B, A = selected.aDS.OutlineColor.A };

            main.sniper_dot_color.SelectedColor = new Color() { R = selected.Sniper.CenterDotColor.R, G = selected.Sniper.CenterDotColor.G, B = selected.Sniper.CenterDotColor.B, A = selected.Sniper.CenterDotColor.A };
            main.SelectedProfile = selected;
            main.Crosshair_load();
        }

        private async Task<UIElement> CreateRender(PublicProfile profile)
        {
            // This will be changed and will be replaced with more efficient method of rendering multiple settings.

            // These first 2 vars can be put somewhere else but I didn't because lazy.
            List<ImageSource> imageSources = new List<ImageSource>()
            {
                new BitmapImage(new Uri("pack://application:,,,/ValorantCC;component/Resources/CrosshairBG0.png")),
                new BitmapImage(new Uri("pack://application:,,,/ValorantCC;component/Resources/CrosshairBG1.png")),
                new BitmapImage(new Uri("pack://application:,,,/ValorantCC;component/Resources/CrosshairBG2.png")),
                new BitmapImage(new Uri("pack://application:,,,/ValorantCC;component/Resources/CrosshairBG3.png")),
                new BitmapImage(new Uri("pack://application:,,,/ValorantCC;component/Resources/CrosshairBG4.png")),
                new BitmapImage(new Uri("pack://application:,,,/ValorantCC;component/Resources/CrosshairBG5.png"))
            };
            Random rand = new Random();
            // Main border
            Border template = new Border()
            {
                Background = (Brush)bc.ConvertFromString("#FF393B44"),
                MinHeight = 95,
                MaxHeight = 95,
                MinWidth = 330,
                MaxWidth = 330,
                Margin = new Thickness() { Bottom = 5, Top = 5, Left = 0, Right = 0 }
            };
            // First Grid and defintions
            Grid Grid0 = new Grid();
            ColumnDefinition gridCol0 = new ColumnDefinition();
            ColumnDefinition gridCol1 = new ColumnDefinition();
            ColumnDefinition gridCol2 = new ColumnDefinition();
            RowDefinition gridRow0 = new RowDefinition() { Height = new GridLength(2.5, GridUnitType.Star) };
            RowDefinition gridRow1 = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) };
            Grid0.ColumnDefinitions.Add(gridCol0);
            Grid0.ColumnDefinitions.Add(gridCol1);
            Grid0.ColumnDefinitions.Add(gridCol2);
            Grid0.RowDefinitions.Add(gridRow0);
            Grid0.RowDefinitions.Add(gridRow1);

            // INner elements
            Image bg = new Image()
            {
                Source = imageSources[rand.Next(0,5)],
                Stretch = Stretch.Fill,
                Margin = new Thickness() { Bottom = 2, Top = 0, Left = 2, Right = 2 }
            };

            Grid.SetRow(bg, 0);
            Grid.SetColumn(bg, 0);
            Grid.SetColumnSpan(bg, 3);

            Label ownerName = new Label()
            {
                Content = profile.owner + "'s Profile",
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = Brushes.White
            };
            Grid.SetColumn(ownerName, 2);
            Grid.SetRow(ownerName, 0);

            Button shareButton = new Button()
            {
                Style = (Style)FindResource("RoundButton"),
                Margin = new Thickness() { Bottom = 0, Top = 0, Left = 5, Right = 5 },
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Content = "Share",
                Cursor = Cursors.Hand,
                Name = "A_"+profile.sharecode+"_shareBtn",
            };
            shareButton.Click += shareBtnClicked;

            Button detailsButton = new Button()
            {
                Style = (Style)FindResource("RoundButton"),
                Margin = new Thickness() { Bottom = 0, Top = 0, Left = 5, Right = 5 },
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Content = "Details",
                Cursor = Cursors.Hand,
                Name = "A_" + profile.ID.ToString() + "_detailsBtn"

            };
            detailsButton.Click += detailsBtnClicked;

            Button applyButton = new Button()
            {
                Style = (Style)FindResource("RoundButton"),
                Margin = new Thickness() { Bottom = 0, Top = 0, Left = 5, Right = 5 },
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Content = "Apply",
                Cursor = Cursors.Hand,
                Name = "A_" + profile.ID.ToString() + "_applyBtn"
            };
            applyButton.Click += applyBtnClicked;

            Grid.SetColumn(shareButton, 0);
            Grid.SetRow(shareButton, 1);
            Grid.SetColumn(detailsButton, 1);
            Grid.SetRow(detailsButton, 1);
            Grid.SetColumn(applyButton, 2);
            Grid.SetRow(applyButton, 1);

            Grid0.Children.Add(bg);
            Grid0.Children.Add(ownerName);

            Grid0.Children.Add(shareButton);
            Grid0.Children.Add(detailsButton);
            Grid0.Children.Add(applyButton);

            Crosshair_Parser.Generate(0, Grid0, profile.settings.Primary);
            Crosshair_Parser.Generate(2, Grid0, profile.settings.aDS);
            template.Child = Grid0;
            await Task.Delay(1);
            return template;
        }
    }
}
