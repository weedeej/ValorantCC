using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ValorantCC.src.Crosshair
{
    public class CrosshairMain
    {
        public static Data ChangeInActiveProfile(List<Color> Colors, int SelectedIndex, Data UserSettings, ProfileList FetchedProfiles)
        {
            Utilities.Utils.Log("Updating inactive color");

            var SelectedProfile = FetchedProfiles.Profiles[SelectedIndex];
            if (!SelectedProfile.bUseAdvancedOptions) SelectedProfile.bUseAdvancedOptions = true;
            SelectedProfile.Primary.Color.R = Colors[0].R; SelectedProfile.Primary.Color.G = Colors[0].G; SelectedProfile.Primary.Color.B = Colors[0].B; SelectedProfile.Primary.Color.A = Colors[0].A;
            SelectedProfile.Primary.OutlineColor.R = Colors[1].R; SelectedProfile.Primary.OutlineColor.G = Colors[1].G; SelectedProfile.Primary.OutlineColor.B = Colors[1].B; SelectedProfile.Primary.OutlineColor.A = Colors[1].A;
            SelectedProfile.aDS.Color.R = Colors[2].R; SelectedProfile.aDS.Color.G = Colors[2].G; SelectedProfile.aDS.Color.B = Colors[2].B; SelectedProfile.aDS.Color.A = Colors[2].A;
            SelectedProfile.aDS.OutlineColor.R = Colors[3].R; SelectedProfile.aDS.OutlineColor.G = Colors[3].G; SelectedProfile.aDS.OutlineColor.B = Colors[3].B; SelectedProfile.aDS.OutlineColor.A = Colors[3].A;
            SelectedProfile.Sniper.CenterDotColor.R = Colors[4].R; SelectedProfile.Sniper.CenterDotColor.G = Colors[4].G; SelectedProfile.Sniper.CenterDotColor.B = Colors[4].B; SelectedProfile.Sniper.CenterDotColor.A = Colors[4].A;
            return UserSettings;
        }

        public static Data ChangeActiveProfile(List<Color> Colors, int SelectedIndex, Data UserSettings, ProfileList FetchedProfiles)
        {
            Utilities.Utils.Log("Updating active color");
            try
            {
                Stringsetting activeProfileColor = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
                activeProfileColor.value = Utilities.Utils.ColorToString(Colors[0]);
            }
            catch
            {
                Utilities.Utils.Log("User has no entry for CrosshairColor, Creating one...");
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairColor", value = Utilities.Utils.ColorToString(Colors[0]) });
            }
            finally
            {
                var SelectedProfile = FetchedProfiles.Profiles[SelectedIndex];
                if (!SelectedProfile.bUseAdvancedOptions) SelectedProfile.bUseAdvancedOptions = true;
                Utilities.Utils.Log("Removing Old colors.");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairSniperCenterDotColor");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSColor");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairOutlineColor");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSOutlineColor");
                Utilities.Utils.Log("Appending new colors.");
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairSniperCenterDotColor", value = Utilities.Utils.ColorToString(Colors[4]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairADSColor", value = Utilities.Utils.ColorToString(Colors[2]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairOutlineColor", value = Utilities.Utils.ColorToString(Colors[1]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairADSOutlineColor", value = Utilities.Utils.ColorToString(Colors[3]) });
                Utilities.Utils.Log("Modifying profile colors.");
                SelectedProfile.Primary.Color.R = Colors[0].R; SelectedProfile.Primary.Color.G = Colors[0].G; SelectedProfile.Primary.Color.B = Colors[0].B; SelectedProfile.Primary.Color.A = Colors[0].A;
                SelectedProfile.Primary.OutlineColor.R = Colors[1].R; SelectedProfile.Primary.OutlineColor.G = Colors[1].G; SelectedProfile.Primary.OutlineColor.B = Colors[1].B; SelectedProfile.Primary.OutlineColor.A = Colors[1].A;
                SelectedProfile.aDS.Color.R = Colors[2].R; SelectedProfile.aDS.Color.G = Colors[2].G; SelectedProfile.aDS.Color.B = Colors[2].B; SelectedProfile.aDS.Color.A = Colors[2].A;
                SelectedProfile.aDS.OutlineColor.R = Colors[3].R; SelectedProfile.aDS.OutlineColor.G = Colors[3].G; SelectedProfile.aDS.OutlineColor.B = Colors[3].B; SelectedProfile.aDS.OutlineColor.A = Colors[3].A;
                SelectedProfile.Sniper.CenterDotColor.R = Colors[4].R; SelectedProfile.Sniper.CenterDotColor.G = Colors[4].G; SelectedProfile.Sniper.CenterDotColor.B = Colors[4].B; SelectedProfile.Sniper.CenterDotColor.A = Colors[4].A;
            }
            return UserSettings;
        }
    }
}
