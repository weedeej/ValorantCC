using System.Linq;
using System.Windows;

namespace ValorantCC.SubWindow
{
    /// <summary>
    /// Lógica de interacción para MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public static void Show(string Message, string Title = null)
        {
            MessageWindow MessageWin = new(Message, Title);
            MessageWin.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            MessageWin.ShowDialog();
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
            Close();
        }
    }
}
