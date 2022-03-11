using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ValorantCC.src
{
    internal static class Crosshair_Parser
    {
        public enum Position
        {
            North = 0,
            South = 1,
            West = 2,
            East = 3,
        }

        public static void Generate(int column, Grid grid, ProfileSettings settings)
        {
            Rectangle[] rectangles = new Rectangle[16];
            int recindex = 0;
            for (int i = 0; i < rectangles.Length; i++)
            {
                Rectangle rectangle = new()
                {
                    Width = 3,
                    Height = 3
                };
                if (recindex > 7)
                    rectangle.Name = "OL";
                rectangles[recindex] = rectangle;
                recindex++;
            }

            recindex = 1;
            for (int i = 0; i < rectangles.Length; i += 2)
            {
                var pos = Position.East;
                switch (recindex)
                {
                    case 1:
                        pos = Position.East;
                        break;
                    case 2:
                        pos = Position.West;
                        break;
                    case 3:
                        pos = Position.North;
                        break;
                    case 4:
                        pos = Position.South;
                        break;
                }
                if (recindex == 4)
                    recindex = 0;
                recindex++;

                rectangle_redraw(rectangles[i], rectangles[i + 1], pos, settings);
                rectangles[i].Fill = new SolidColorBrush(Color.FromArgb(settings.Color.A, settings.Color.R, settings.Color.G, settings.Color.B));
                rectangles[i + 1].Stroke = new SolidColorBrush(Color.FromArgb(settings.OutlineColor.A, settings.OutlineColor.R, settings.OutlineColor.G, settings.OutlineColor.B));
                Grid.SetColumn(rectangles[i], column);
                Grid.SetRow(rectangles[i], 0);
                Grid.SetColumn(rectangles[i + 1], column);
                Grid.SetRow(rectangles[i + 1], 0);

                grid.Children.Add(rectangles[i + 1]);
                grid.Children.Add(rectangles[i]);
            }

            Rectangle dot = new Rectangle()
            {
                Margin = new Thickness(0, 0, 0, 0)
            };
            Rectangle dotOT = new Rectangle()
            {
                Margin = new Thickness(0, 0, 0, 0)
            };

            dot_redraw(dot, dotOT, settings);
            dot.Fill = new SolidColorBrush(Color.FromArgb(settings.Color.A, settings.Color.R, settings.Color.G, settings.Color.B));
            dotOT.Stroke = new SolidColorBrush(Color.FromArgb(settings.OutlineColor.A, settings.OutlineColor.R, settings.OutlineColor.G, settings.OutlineColor.B));
            Grid.SetColumn(dot, column);
            Grid.SetRow(dot, 0);
            Grid.SetColumn(dotOT, column);
            Grid.SetColumn(dotOT, 0);

            grid.Children.Add(dotOT);
            grid.Children.Add(dot);
        }

        public static void Generate(int column, Grid grid, SniperSettings settings)
        {
            Ellipse ellipse = new();
            dot_redraw(ellipse, settings);
            ellipse.Fill = new SolidColorBrush(Color.FromArgb(settings.CenterDotColor.A, settings.CenterDotColor.R, settings.CenterDotColor.G, settings.CenterDotColor.B));
            Grid.SetColumn(ellipse, column);
            Grid.SetRow(ellipse, 0);

            grid.Children.Add(ellipse);
        }

        public static void dot_redraw(Rectangle Rect, Rectangle RectOT, ProfileSettings settings)
        {
            Rect.Width = Rect.Height = settings.CenterDotSize;
            Rect.Opacity = settings.CenterDotOpacity;

            RectOT.Width = RectOT.Height = settings.CenterDotSize + settings.OutlineThickness + .5;
            RectOT.StrokeThickness = settings.OutlineThickness;
            RectOT.Opacity = settings.OutlineOpacity;

            if (settings.bHasOutline)
                RectOT.Visibility = Visibility.Visible;
            else
                RectOT.Visibility = Visibility.Hidden;

            if (settings.bDisplayCenterDot)
                Rect.Visibility = RectOT.Visibility = Visibility.Visible;
            else
                Rect.Visibility = RectOT.Visibility = Visibility.Hidden;
        }

        public static void dot_redraw(Ellipse ellipse, SniperSettings settings)
        {
            ellipse.Width = ellipse.Height = settings.CenterDotSize * 6;
            ellipse.Opacity = settings.CenterDotOpacity;

            if (settings.bDisplayCenterDot)
                ellipse.Visibility = Visibility.Visible;
            else
                ellipse.Visibility = Visibility.Hidden;
        }

        public static void rectangle_redraw(Rectangle Rect, Rectangle RectOT, Position pos, ProfileSettings settings)
        {
            double N, S, W, E, Width, Height, Margin;
            N = S = W = E = Margin = 0;
            Width = Rect.Width;
            Height = Rect.Height;

            LineSettings line = settings.InnerLines;
            if (Rect.Name.Contains("OL"))
                line = settings.OuterLines;

            if ((bool)(line?.bShowShootingError))
                Margin += 8;

            switch (pos)
            {
                case Position.North:
                    Height = line.LineLength;
                    Width = line.LineThickness;
                    N = line.LineOffset * 2 + Margin + Height;
                    break;
                case Position.South:
                    Height = line.LineLength;
                    Width = line.LineThickness;
                    S = line.LineOffset * 2 + Margin + Height;
                    break;
                case Position.West:
                    Width = line.LineLength;
                    Height = line.LineThickness;
                    W = line.LineOffset * 2 + Margin + Width;
                    break;
                case Position.East:
                    Width = line.LineLength;
                    Height = line.LineThickness;
                    E = line.LineOffset * 2 + Margin + Width;
                    break;
            }

            Rect.Width = Width;
            Rect.Height = Height;
            Rect.Margin = new Thickness(E, S, W, N);
            Rect.Opacity = line.Opacity;

            RectOT.Width = Rect.Width + settings.OutlineThickness + .5;
            RectOT.Height = Rect.Height + settings.OutlineThickness + .5;
            RectOT.StrokeThickness = settings.OutlineThickness;
            RectOT.Margin = new Thickness(E, S, W, N);
            RectOT.Opacity = settings.OutlineOpacity;


            if (line.bShowLines)
                Rect.Visibility = Visibility.Visible;
            else
                Rect.Visibility = Visibility.Hidden;

            if (settings.bHasOutline && line.LineThickness > 0 && line.bShowLines)
                RectOT.Visibility = Visibility.Visible;
            else
                RectOT.Visibility = Visibility.Hidden;
        }
    }
}