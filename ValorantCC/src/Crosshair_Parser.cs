using System.Diagnostics;
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
            /*for (int i = 1; i <= 4; i++)
            {
                string name = $"prime{}";
                switch (i)
                {
                    case 0:
                        name += "X";
                        break;
                    case 1:
                        name += "Y";
                        break;
                    case 2:
                        name += "OLX";
                        break;
                    case 3:
                        name += "OLY";
                        break;
                }

                for (int j = 1; j <= 4; j++)
                {
                    string subname = name;
                    if (j % 2 != 0)
                        subname += "OT";

                    Rectangle rectangle = new Rectangle()
                    {
                        Name = subname,
                        Width = 3,
                        Height = 3
                    };
                    rectangles[recindex] = rectangle;
                    recindex++;
                }
            }*/

            for (int i = 0; i < rectangles.Length; i++)
            {
                Rectangle rectangle = new Rectangle()
                {
                    Width = 3,
                    Height = 3
                };
                rectangles[recindex] = rectangle;
                recindex++;
            }

            recindex = 1;
            for (int i = 0; i < rectangles.Length; i+=2)
            {
                var pos = Position.East;
                switch (recindex % 4)
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
                    case 0:
                        pos = Position.South;
                        break;
                }
                recindex++;
                if(recindex == 4)
                    recindex = 0;

                rectangle_redraw(rectangles[i], rectangles[i + 1], pos, settings);
                rectangles[i].Fill = new SolidColorBrush(Color.FromRgb(settings.Color.R, settings.Color.G, settings.Color.B));
                rectangles[i + 1].Stroke = new SolidColorBrush(Color.FromRgb(settings.OutlineColor.R, settings.OutlineColor.G, settings.OutlineColor.B));
                Grid.SetColumn(rectangles[i], column);
                Grid.SetRow(rectangles[i], 0);
                Grid.SetColumn(rectangles[i + 1], column);
                Grid.SetRow(rectangles[i + 1], 0);

                grid.Children.Add(rectangles[i]);
                grid.Children.Add(rectangles[i + 1]);
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
            Grid.SetColumn(dot, column);
            Grid.SetRow(dot, 0);
            Grid.SetColumn(dotOT, column);
            Grid.SetColumn(dotOT, 0);

            grid.Children.Add(dot);
            grid.Children.Add(dotOT);
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

            if (line.bShowShootingError)
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
