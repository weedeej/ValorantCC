using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        private static API ValCCApi;
        private static MainWindow main;
        private static List<PublicProfile> PublicProfiles = new List<PublicProfile>();
        public ProfilesWindow(CrosshairProfile current, API ValCCAPI)
        {
            InitializeComponent();
            main = (MainWindow) Application.Current.MainWindow;
            ValCCApi = ValCCAPI;
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


            for (int i = 0; i < Shareables.Count; i++)
            {
                ShareableProfile currentShareable = Shareables[i];
                CrosshairProfile profile;
                try
                {
                    profile = JsonConvert.DeserializeObject<CrosshairProfile>(currentShareable.settings);
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
        private static void RenderProfiles()
        {

        }
        private void btnSearchCode_Click(object sender, RoutedEventArgs e)
        {
            if (SearchCode.Text == null || SearchCode.Text == "") return;
            InitialFetch(SearchCode.Text);
            RenderProfiles();
        }
    }
}
