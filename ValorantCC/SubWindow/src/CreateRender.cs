using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ValorantCC.src;

namespace ValorantCC
{
    public static class CreateRender
    {
        enum ButtonType
        {
            Share,
            Details,
            Apply
        }

        static Style style;
        static Random rand = new Random();
        static BrushConverter bc = new BrushConverter();
        static ImageBrush previousBG = null;
        static List<ImageBrush> imageSources = new List<ImageBrush>();
        static ImageBrush RandomBG => imageSources[rand.Next(0, imageSources.Count - 1)];

        static CreateRender()
        {
            ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("Resources/BGdictionary.xaml", UriKind.Relative));
            for (int i = 0; i < res.Count; i++)
                imageSources.Add(new ImageBrush((ImageSource)res["crosshairBG" + i]));
        }

        public static async Task<UIElement> GenerateRender(this ProfilesWindow profilesWindow, PublicProfile profile)
        {
            //More random BG xd
            var RandomBGC = RandomBG;
            while (RandomBGC == previousBG)
                RandomBGC = RandomBG;
            previousBG = RandomBGC;

            // Main border
            Border template = new Border()
            {
                Background = RandomBGC,
                CornerRadius = new CornerRadius(7, 7, 25, 25),
                MinHeight = 95,
                MaxHeight = 95,
                MinWidth = 300,
                MaxWidth = 300,
                Margin = new Thickness(5, 10, 0, 0)
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
            // General button background
            Border rect = new()
            {
                Background = (Brush)bc.ConvertFromString("#282A36"),
                CornerRadius = new CornerRadius(0, 0, 7, 7),
                MinWidth = 300,
                MaxWidth = 300,
                MinHeight = 27,
                MaxHeight = 27,
            };
            Grid.SetColumn(rect, 0);
            Grid.SetRow(rect, 1);
            Grid.SetColumnSpan(rect, 3);

            Label ownerName = new Label()
            {
                Content = profile.owner,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = Brushes.White
            };
            Grid.SetColumn(ownerName, 2);
            Grid.SetRow(ownerName, 0);

            style = (Style)profilesWindow.FindResource("MahApps.Styles.Button.MetroWindow.Base");

            Button shareButton = ButtonType.Share.Generate(profile.sharecode);
            shareButton.Click += profilesWindow.shareBtnClicked;
            shareButton.Foreground = Brushes.White;

            Button detailsButton = ButtonType.Details.Generate(profile.ID.ToString());
            detailsButton.Click += profilesWindow.detailsBtnClicked;
            detailsButton.Foreground = Brushes.White;

            Button applyButton = ButtonType.Apply.Generate(profile.ID.ToString());
            applyButton.Click += profilesWindow.applyBtnClicked;
            applyButton.Foreground = Brushes.White;

            Grid.SetColumn(shareButton, 0);
            Grid.SetRow(shareButton, 1);
            Grid.SetColumn(detailsButton, 1);
            Grid.SetRow(detailsButton, 1);
            Grid.SetColumn(applyButton, 2);
            Grid.SetRow(applyButton, 1);
            Grid0.Children.Add(rect);
            Grid0.Children.Add(ownerName);
            Grid0.Children.Add(shareButton);
            Grid0.Children.Add(detailsButton);
            Grid0.Children.Add(applyButton);
            ProfileSettings cross = profile.settings.Primary;

            Crosshair_Parser.Generate(0, Grid0, cross);
            if (!profile.settings.bUsePrimaryCrosshairForADS)
                cross = profile.settings.aDS;
            Crosshair_Parser.Generate(1, Grid0, cross);
            Crosshair_Parser.Generate(2, Grid0, profile.settings.Sniper);
            template.Child = Grid0;
            await Task.Delay(1);
            return template;
        }

        private static Button Generate(this ButtonType btntype, string namedata)
        {
            Button button = new Button()
            {
                Style = style,
                Background = (Brush)bc.ConvertFromString("#4648BF"),
                Margin = new Thickness() { Bottom = 0, Top = 2, Left = 5, Right = 5 },
                FontSize = 11,
                Height = 22,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
            };

            switch (btntype)
            {
                case ButtonType.Share:
                    button.Content = "Share";
                    button.Name = $"A_{namedata}_shareBtn";
                    break;
                case ButtonType.Details:
                    button.Content = "Details";
                    button.Name = $"A_{namedata}_detailsBtn";
                    break;
                case ButtonType.Apply:
                    button.Content = "Apply";
                    button.Name = $"A_{namedata}_applyBtn";
                    break;
            }

            return button;
        }
    }
}
