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
        private static Color WhiteColor = new Color { R = 255, G = 255, B = 255 };
        public static Data ChangeInActiveProfile(List<Color> Colors, int SelectedIndex, Data UserSettings, ProfileList FetchedProfiles)
        {
            Utilities.Utils.Log("Updating inactive color");

            var SelectedProfile = FetchedProfiles.Profiles[SelectedIndex];
            if (!SelectedProfile.bUseAdvancedOptions) SelectedProfile.bUseAdvancedOptions = true;
            SelectedProfile.Primary.bUseCustomColor = true;
            SelectedProfile.Primary.Color.R = Colors[0].R; SelectedProfile.Primary.Color.G = Colors[0].G; SelectedProfile.Primary.Color.B = Colors[0].B; SelectedProfile.Primary.Color.A = Colors[0].A;
            SelectedProfile.Primary.colorCustom.R = Colors[0].R; SelectedProfile.Primary.colorCustom.G = Colors[0].G; SelectedProfile.Primary.colorCustom.B = Colors[0].B; SelectedProfile.Primary.colorCustom.A = Colors[0].A;
            SelectedProfile.Primary.OutlineColor.R = Colors[1].R; SelectedProfile.Primary.OutlineColor.G = Colors[1].G; SelectedProfile.Primary.OutlineColor.B = Colors[1].B; SelectedProfile.Primary.OutlineColor.A = Colors[1].A;
            SelectedProfile.aDS.bUseCustomColor = true;
            SelectedProfile.aDS.Color.R = Colors[2].R; SelectedProfile.aDS.Color.G = Colors[2].G; SelectedProfile.aDS.Color.B = Colors[2].B; SelectedProfile.aDS.Color.A = Colors[2].A;
            SelectedProfile.aDS.colorCustom.R = Colors[2].R; SelectedProfile.aDS.colorCustom.G = Colors[2].G; SelectedProfile.aDS.colorCustom.B = Colors[2].B; SelectedProfile.aDS.colorCustom.A = Colors[2].A;
            SelectedProfile.aDS.OutlineColor.R = Colors[3].R; SelectedProfile.aDS.OutlineColor.G = Colors[3].G; SelectedProfile.aDS.OutlineColor.B = Colors[3].B; SelectedProfile.aDS.OutlineColor.A = Colors[3].A;
            SelectedProfile.Sniper.bUseCustomCenterDotColor = true;
            SelectedProfile.Sniper.CenterDotColor.R = Colors[4].R; SelectedProfile.Sniper.CenterDotColor.G = Colors[4].G; SelectedProfile.Sniper.CenterDotColor.B = Colors[4].B; SelectedProfile.Sniper.CenterDotColor.A = Colors[4].A;
            SelectedProfile.Sniper.centerDotColorCustom.R = Colors[4].R; SelectedProfile.Sniper.centerDotColorCustom.G = Colors[4].G; SelectedProfile.Sniper.centerDotColorCustom.B = Colors[4].B; SelectedProfile.Sniper.centerDotColorCustom.A = Colors[4].A;
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
                SelectedProfile.Primary.bUseCustomColor = true;
                SelectedProfile.Primary.Color.R = Colors[0].R; SelectedProfile.Primary.Color.G = Colors[0].G; SelectedProfile.Primary.Color.B = Colors[0].B; SelectedProfile.Primary.Color.A = Colors[0].A;
                SelectedProfile.Primary.colorCustom.R = Colors[0].R; SelectedProfile.Primary.colorCustom.G = Colors[0].G; SelectedProfile.Primary.colorCustom.B = Colors[0].B; SelectedProfile.Primary.colorCustom.A = Colors[0].A;
                SelectedProfile.Primary.OutlineColor.R = Colors[1].R; SelectedProfile.Primary.OutlineColor.G = Colors[1].G; SelectedProfile.Primary.OutlineColor.B = Colors[1].B; SelectedProfile.Primary.OutlineColor.A = Colors[1].A;
                SelectedProfile.aDS.bUseCustomColor = true;
                SelectedProfile.aDS.Color.R = Colors[2].R; SelectedProfile.aDS.Color.G = Colors[2].G; SelectedProfile.aDS.Color.B = Colors[2].B; SelectedProfile.aDS.Color.A = Colors[2].A;
                SelectedProfile.aDS.colorCustom.R = Colors[2].R; SelectedProfile.aDS.colorCustom.G = Colors[2].G; SelectedProfile.aDS.colorCustom.B = Colors[2].B; SelectedProfile.aDS.colorCustom.A = Colors[2].A;
                SelectedProfile.aDS.OutlineColor.R = Colors[3].R; SelectedProfile.aDS.OutlineColor.G = Colors[3].G; SelectedProfile.aDS.OutlineColor.B = Colors[3].B; SelectedProfile.aDS.OutlineColor.A = Colors[3].A;
                SelectedProfile.Sniper.bUseCustomCenterDotColor = true;
                SelectedProfile.Sniper.CenterDotColor.R = Colors[4].R; SelectedProfile.Sniper.CenterDotColor.G = Colors[4].G; SelectedProfile.Sniper.CenterDotColor.B = Colors[4].B; SelectedProfile.Sniper.CenterDotColor.A = Colors[4].A;
                SelectedProfile.Sniper.centerDotColorCustom.R = Colors[4].R; SelectedProfile.Sniper.centerDotColorCustom.G = Colors[4].G; SelectedProfile.Sniper.centerDotColorCustom.B = Colors[4].B; SelectedProfile.Sniper.centerDotColorCustom.A = Colors[4].A;
            }
            return UserSettings;
        }
    }
}
