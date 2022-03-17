using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ValorantCC.SubWindow
{
    /// <summary>
    /// Lógica de interacción para MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public static bool Show(string Message, string Title = null, bool OkCancel = false)
        {
            MessageWindow MessageWin = new(Message, Title);
            MessageWin.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (OkCancel)
            {
                var CancelBtn = new Button()
                {
                    Content = "CANCEL",
                    Height = MessageWin.OKbtn.Height,
                    Width = MessageWin.OKbtn.Width,
                    Background = MessageWin.OKbtn.Background,
                    FontFamily = MessageWin.OKbtn.FontFamily,
                    FontSize = MessageWin.OKbtn.FontSize,
                    Style = MessageWin.OKbtn.Style,
                    Cursor = MessageWin.OKbtn.Cursor,
                    FontWeight = MessageWin.OKbtn.FontWeight,
                    Margin = new Thickness(5, 10, 0, 0)
                };
                CancelBtn.Click += MessageWin.CancelBtn_Click;
                MessageWin.ButtonGroup.Children.Add(CancelBtn);
            }
            MessageWin.ShowDialog();
            return (bool)MessageWin.DialogResult;
        }

        public MessageWindow(string Message, string Title = null)
        {
            InitializeComponent();
            TitleBar.Content = Title;
            Messagetxtbox.Text = Message;
            Messagetxtbox.Focusable = false;
            OKbtn.Focus();
        }

        private void OKbtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        public void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
