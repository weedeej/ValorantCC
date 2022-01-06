using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
namespace ValorantCC
{
    /// <summary>
    /// Interaction logic for ProfilesWindow.xaml
    /// </summary>
    
    public struct PublicProfile
    {
        public String owner { get; set; }
        public CrosshairProfile settings { get; set; }
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
            ValCCApi = ValCCAPI;
            bc = new BrushConverter();
            InitialFetch();
            RenderProfiles();
        }

        /// <summary>
        /// Returns the searched sharecode or the list of shareables depending if sharecode is provided or not.
        /// </summary>
        /// <param name="sharecode">Nullable. Searched Code</param>
        private static void InitialFetch(String sharecode = null)
        {
            List<ShareableProfile> Shareables;
            if (sharecode != null)
            {
                ValCCApi.Action = 3;
                Shareables = ValCCApi.Fetch(sharecode).data;
            }
            else Shareables = ValCCApi.Fetch().data;

            if (Shareables.Count == 0) return;
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
                PublicProfiles.Add(new PublicProfile() { owner = currentShareable.displayName, settings = profile });
            }
        }

        /// <summary>
        /// Render the PublicProfiles var into frontend.
        /// </summary>
        private void RenderProfiles()
        {
            UIElementCollection shareablesElement = ShareablesContainer.Children;
            if (PublicProfiles.Count == 0) return;
            
            for (int i = 0; i < PublicProfiles.Count; i++)
            {
                PublicProfile profile = PublicProfiles[i];
                shareablesElement.Add(CreateRender(profile));
            }

        }
        private void btnSearchCode_Click(object sender, RoutedEventArgs e)
        {
            if (SearchCode.Text == null || SearchCode.Text == "") return;
            InitialFetch(SearchCode.Text);
            RenderProfiles();
        }

        private UIElement CreateRender(PublicProfile profile)
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
                Content = "Share"
            };
            Button detailsButton = new Button()
            {
                Style = (Style)FindResource("RoundButton"),
                Margin = new Thickness() { Bottom = 0, Top = 0, Left = 5, Right = 5 },
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Content = "Details"

            };
            Button applyButton = new Button()
            {
                Style = (Style)FindResource("RoundButton"),
                Margin = new Thickness() { Bottom = 0, Top = 0, Left = 5, Right = 5 },
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Content = "Apply"
            };
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
            template.Child = Grid0;
            return template;
        }
    }
}
