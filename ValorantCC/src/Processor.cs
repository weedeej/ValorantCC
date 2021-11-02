using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using RestSharp;
using Newtonsoft.Json;
using Utilities;

namespace ValorantCC
{
    class Processor
    {
        private AuthResponse AuthResponse;
        public bool isLoggedIn = false;
        private RestClient client = new RestClient("https://playerpreferences.riotgames.com");
        private Data UserSettings;
        private int SavedProfilesIndex = 0;
        public bool ProfileListed;
        public List<string> ProfileNames;
        private ProfileList FetchedProfiles;
        public int CurrentProfile;

        public AuthResponse Login()
        {
            Utils.Log("Login started");
            AuthObj AuthObj = new AuthObj();
            AuthResponse = AuthObj.StartAuth(false);
            if (!AuthResponse.Success) return AuthResponse;
            Utils.Log("Auth Success");
            Construct();
            return AuthResponse;
        }

        public void Construct()
        {
            Utils.Log("Constructing Properties -->");

            client.AddDefaultHeaders(Utils.ConstructHeaders(AuthResponse));
            UserSettings = FetchUserSettings();
            ProfileListed = CheckIfList(UserSettings);
            Utils.Log("Multiple Profiles: " + ProfileListed.ToString());
            if (ProfileListed)
            {
                SavedProfilesIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::SavedCrosshairProfileData");
            }
            else
            {
                SavedProfilesIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
                if (SavedProfilesIndex == -1)
                {
                    Utils.Log("User is new account/Using White Color");
                    Stringsetting[] StringSettings = UserSettings.stringSettings.Append(new Stringsetting
                    {
                        settingEnum = "EAresStringSettingName::CrosshairColor",
                        value = "(R=0,G=0,B=0,A=255)"
                    }).ToArray();
                    UserSettings.stringSettings = StringSettings;
                    SavedProfilesIndex = StringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
                }
            }

            FetchedProfiles = FetchProfiles(UserSettings.stringSettings[SavedProfilesIndex].value);
            ProfileNames = FetchProfileNames(FetchedProfiles);
            CurrentProfile = FetchedProfiles.CurrentProfile;

            isLoggedIn = true;
            Utils.Log("<-- Constructing Properties");
        }

        private bool CheckIfList (Data settings)
        {
            Utils.Log("Checking if User has multiple profiles");
            return settings.stringSettings.Any(setting => setting.settingEnum == "EAresStringSettingName::SavedCrosshairProfileData");
        }

        private Data FetchUserSettings()
        {
            Utils.Log("Obtaining User Settings");
            RestRequest request = new RestRequest("/playerPref/v3/getPreference/Ares.PlayerSettings");
            string responseContext = client.Get(request).Content;
            Dictionary<string, object> response = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContext);
            Data settings = Utils.Decompress(Convert.ToString(response["data"]));
            return settings;
        }

        private bool putUserSettings(Data newData)
        {
            Utils.Log("Saving New Data: (BACKUP) "+ JsonConvert.SerializeObject(newData));
            
            IRestRequest request = new RestRequest("/playerPref/v3/savePreference");
            request.AddJsonBody(new { type = "Ares.PlayerSettings", data = Utils.Compress(newData) });
            IRestResponse response = client.Put(request);
            if (!response.IsSuccessful) return false;

            Dictionary<string, object> responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
            if (responseDict.ContainsKey("data")) return true;
            return false;
        }

        private ProfileList FetchProfiles(string SettingValue)
        {
            Utils.Log("Fetching/Creating Profile/s");
            Utils.Log("Setting Value: " + JsonConvert.SerializeObject(UserSettings));
            if (ProfileListed) return JsonConvert.DeserializeObject<ProfileList>(SettingValue);
            CrosshairColor ParsedColor = Utils.parseCrosshairColor(UserSettings.stringSettings[SavedProfilesIndex].value);
            int NameIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairProfileName");

            return new ProfileList
            {
                CurrentProfile = 0,
                Profiles = new List<CrosshairProfile>
                {
                    new CrosshairProfile
                    {
                        Primary = new ProfileSettings {Color = ParsedColor},
                        ProfileName = UserSettings.stringSettings[NameIndex].value
                    }
                }
            };
        }

        private List<string> FetchProfileNames (ProfileList ProfileList)
        {
            Utils.Log("Fetching Profile names");
            if (ProfileListed) return (from profile in ProfileList.Profiles
                        select profile.ProfileName).ToList();

            int NameIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairProfileName");
            return new List<string> { UserSettings.stringSettings[NameIndex].value };
        }

        public CrosshairProfile ProfileFromIndex(int Index)
        {
            Utils.Log("Obtained Profile from index: " + FetchedProfiles.Profiles[Index].ProfileName);
            return FetchedProfiles.Profiles[Index];
        }

        private void ChangeActiveProfile(List<Color> Colors, int SelectedIndex)
        {
            Utils.Log("Updating active color");
            int DefaultProfileIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
            UserSettings.stringSettings[DefaultProfileIndex].value = Utils.ColorToString(Colors[0]);
            List<Stringsetting> DummySettings = UserSettings.stringSettings.ToList();
            if (FetchedProfiles.Profiles[SelectedIndex].bUseAdvancedOptions)
            {
                Utils.Log("Removing Old colors.");
                int SniperCrosshairIndex = DummySettings.FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairSniperCenterDotColor");
                if (SniperCrosshairIndex != -1) DummySettings.RemoveAt(SniperCrosshairIndex);
                int aDSCrosshairIndex = DummySettings.FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSColor");
                if (aDSCrosshairIndex != -1) DummySettings.RemoveAt(aDSCrosshairIndex);
            }
            Utils.Log("Appending new colors.");
            DummySettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairSniperCenterDotColor", value = Utils.ColorToString(Colors[4]) });
            DummySettings.Add(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairADSColor", value = Utils.ColorToString(Colors[2]) });
            UserSettings.stringSettings = DummySettings.ToArray();
        }
        private void SaveListedSettings(List<Color> Colors, int SelectedIndex)
        {
            if (SelectedIndex == FetchedProfiles.CurrentProfile)
            {
                ChangeActiveProfile(Colors, SelectedIndex);
            }
            Utils.Log("Modifying Selected Profile.");
            FetchedProfiles.Profiles[SelectedIndex].bUseAdvancedOptions = true;
            FetchedProfiles.Profiles[SelectedIndex].bUsePrimaryCrosshairForADS = false;
            FetchedProfiles.Profiles[SelectedIndex].Primary.Color = new CrosshairColor { R = Colors[0].R, G = Colors[0].G, B = Colors[0].B, A = 255 };
            FetchedProfiles.Profiles[SelectedIndex].Primary.OutlineColor = new CrosshairColor { R = Colors[1].R, G = Colors[1].G, B = Colors[1].B, A = 255 };
            FetchedProfiles.Profiles[SelectedIndex].aDS.Color = new CrosshairColor { R = Colors[2].R, G = Colors[2].G, B = Colors[2].B, A = 255 };
            FetchedProfiles.Profiles[SelectedIndex].aDS.OutlineColor = new CrosshairColor { R = Colors[3].R, G = Colors[3].G, B = Colors[3].B, A = 255 };

            if (FetchedProfiles.Profiles[SelectedIndex].Sniper == null)
            {
                FetchedProfiles.Profiles[SelectedIndex].Sniper = new SniperSettings
                {
                    bDisplayCenterDot = true,
                    CenterDotColor = new CrosshairColor { R = Colors[4].R, G = Colors[4].G, B = Colors[4].B, A = 255 },
                    CenterDotOpacity = 1,
                    CenterDotSize = 1
                };
            }
            else
                FetchedProfiles.Profiles[SelectedIndex].Sniper.CenterDotColor = new CrosshairColor { R = Colors[4].R, G = Colors[4].G, B = Colors[4].B, A = 255 };
        }
        public bool SaveNewColor(List<Color> Colors, int SelectedIndex, string ProfileName)
        {
            Utils.Log("Save button clicked. Saving...");
            if (ProfileListed)
            {
                Utils.Log("Profile type: List");
                SaveListedSettings(Colors, SelectedIndex);
                FetchedProfiles.Profiles[SelectedIndex].ProfileName = ProfileName;
                SavedProfilesIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::SavedCrosshairProfileData");
                UserSettings.stringSettings[SavedProfilesIndex].value = JsonConvert.SerializeObject(FetchedProfiles);
            }
            else
            {
                Utils.Log("Profile type: Enum");
                int NameIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairProfileName");
                UserSettings.stringSettings[SavedProfilesIndex].value = Utils.ColorToString(Colors[0]);
                UserSettings.stringSettings[NameIndex].value = ProfileName;
            }
            if (putUserSettings(UserSettings))
            {
                Utils.Log("Reconstructing Data");
                Construct();
                return true;
            }
            return false;
        }
    }
}
