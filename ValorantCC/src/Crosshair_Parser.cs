using System.Windows;
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
