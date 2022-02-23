using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace ValorantCC.src
{
    public static class Modifier
    {
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
                if (FetchedProfiles.Profiles[SelectedIndex].bUseAdvancedOptions)
                {
                    Utilities.Utils.Log("Removing Old colors.");
                    UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairSniperCenterDotColor");
                    UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSColor");
                    UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairOutlineColor");
                    UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSOutlineColor");
                }
                Utilities.Utils.Log("Appending new colors.");
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairSniperCenterDotColor", value = Utilities.Utils.ColorToString(Colors[4]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairADSColor", value = Utilities.Utils.ColorToString(Colors[2]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairOutlineColor", value = Utilities.Utils.ColorToString(Colors[1]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairADSOutlineColor", value = Utilities.Utils.ColorToString(Colors[3]) });
            }
            return UserSettings;
        }
    }
}
