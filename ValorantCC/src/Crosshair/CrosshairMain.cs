using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace ValorantCC.src.Crosshair
{
    public class CrosshairMain
    {
        private static Color WhiteColor = new Color { R = 255, G = 255, B = 255 };
        public static Data ChangeInActiveProfile(List<Color> Colors, int SelectedIndex, Data UserSettings, ProfileList FetchedProfiles)
        {
            Utilities.Utils.Log("Updating inactive color");

            var SelectedProfile = FetchedProfiles.Profiles[SelectedIndex];
            if (!SelectedProfile.bUseAdvancedOptions) SelectedProfile.bUseAdvancedOptions = true;

            ChangeProfileColors(SelectedProfile, Colors);

            return UserSettings;
        }

        public static Data ChangeActiveProfile(List<Color> Colors, int SelectedIndex, Data UserSettings, ProfileList FetchedProfiles)
        {
            Utilities.Utils.Log("Updating active color");
            try
            {
                Stringsetting activeProfileColor = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
                activeProfileColor.value = Utilities.Utils.ColorToString(WhiteColor);
            }
            catch
            {
                Utilities.Utils.Log("User has no entry for CrosshairColor, Creating one...");
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairColor", value = Utilities.Utils.ColorToString(WhiteColor) });
            }
            finally
            {
                var SelectedProfile = FetchedProfiles.Profiles[SelectedIndex];
                if (!SelectedProfile.bUseAdvancedOptions) SelectedProfile.bUseAdvancedOptions = true;
                Utilities.Utils.Log("Removing Old colors.");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColorCustom");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairOutlineColor");

                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSColor");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSColorCustom");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSOutlineColor");

                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairSniperCenterDotColor");
                UserSettings.stringSettings.RemoveAll(setting => setting.settingEnum == "EAresStringSettingName::CrosshairSniperCenterDotColorCustom");

                UserSettings.boolSettings.RemoveAll(setting => setting.settingEnum == "EAresBoolSettingName::CrosshairUseCustomColor");
                UserSettings.boolSettings.RemoveAll(setting => setting.settingEnum == "EAresBoolSettingName::CrosshairADSUseCustomColor");
                UserSettings.boolSettings.RemoveAll(setting => setting.settingEnum == "EAresBoolSettingName::CrosshairSniperUseCustomColor");
                Utilities.Utils.Log("Appending new colors.");
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairColor", value = Utilities.Utils.ColorToString(Colors[0]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairColorCustom", value = Utilities.Utils.ColorToString(Colors[0]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairOutlineColor", value = Utilities.Utils.ColorToString(Colors[1]) });

                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairADSColor", value = Utilities.Utils.ColorToString(Colors[2]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairADSColorCustom", value = Utilities.Utils.ColorToString(Colors[2]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairADSOutlineColor", value = Utilities.Utils.ColorToString(Colors[3]) });

                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairSniperCenterDotColor", value = Utilities.Utils.ColorToString(Colors[4]) });
                UserSettings.stringSettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairSniperCenterDotColorCustom", value = Utilities.Utils.ColorToString(Colors[4]) });

                UserSettings.boolSettings.Add(new Boolsetting { settingEnum = "EAresBoolSettingName::CrosshairUseCustomColor", value = true });
                UserSettings.boolSettings.Add(new Boolsetting { settingEnum = "EAresBoolSettingName::CrosshairADSUseCustomColor", value = true });
                UserSettings.boolSettings.Add(new Boolsetting { settingEnum = "EAresBoolSettingName::CrosshairSniperUseCustomColor", value = true });
                Utilities.Utils.Log("Modifying profile colors.");

                ChangeProfileColors(SelectedProfile, Colors);
            }
            return UserSettings;
        }

        public static CrosshairProfile ChangeProfileColors(CrosshairProfile SelectedProfile, List<Color> Colors)
        {
            SelectedProfile.Primary.bUseCustomColor = true;
            SelectedProfile.Primary.colorCustom = Colors[0];
            SelectedProfile.Primary.Color = Colors[0];
            SelectedProfile.Primary.OutlineColor = Colors[1];

            SelectedProfile.aDS.bUseCustomColor = true;
            SelectedProfile.aDS.colorCustom = Colors[2];
            SelectedProfile.aDS.Color = Colors[2];
            SelectedProfile.aDS.OutlineColor = Colors[3];

            SelectedProfile.Sniper.bUseCustomCenterDotColor = true;
            SelectedProfile.Sniper.CenterDotColor = Colors[4];
            SelectedProfile.Sniper.centerDotColorCustom = Colors[4];
            return SelectedProfile;
        }

        public static CrosshairProfile ChangeProfileParams(CrosshairProfile SelectedProfile, CrosshairProfile CommunityProfile)
        {
            // TODO: Implement changing of CRosshair Params such as Length, Opacity, etc
            CommunityProfile.ProfileName = SelectedProfile.ProfileName;
            return CommunityProfile;
        }
    }
}
