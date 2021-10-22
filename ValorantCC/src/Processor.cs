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
        private bool ProfileListed;
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
            Utils.Log("Obtained Profile from index: "+ FetchedProfiles.Profiles[Index].ProfileName);
            return FetchedProfiles.Profiles[Index];
        }

        public bool SaveNewColor(Color Color, int SelectedIndex, string ProfileName)
        {
            Utils.Log("Save button clicked. Saving...");
            if (ProfileListed)
            {
                CrosshairColor NewColor = new CrosshairColor
                {
                    R = Color.R,
                    G = Color.G,
                    B = Color.B,
                    A = 255
                };
                if (SelectedIndex == FetchedProfiles.CurrentProfile)
                {
                    int DefaultProfileIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
                    UserSettings.stringSettings[DefaultProfileIndex].value = Utils.ColorToString(Color);
                    List<Stringsetting> DummySettings = UserSettings.stringSettings.ToList();
                    if (FetchedProfiles.Profiles[SelectedIndex].bUseAdvancedOptions)
                    {
                        int SniperCrosshairIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairSniperCenterDotColor");
                        int aDSCrosshairIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSColor");
                        
                        if (SniperCrosshairIndex != -1) DummySettings.RemoveAt(SniperCrosshairIndex);
                        if (aDSCrosshairIndex != -1) DummySettings.RemoveAt(aDSCrosshairIndex-1);
                        SavedProfilesIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::SavedCrosshairProfileData");
                    }
                    DummySettings = DummySettings.Append(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairSniperCenterDotColor", value = Utils.ColorToString(Color) }).ToList();
                    DummySettings = DummySettings.Append(new Stringsetting { settingEnum = "EAresStringSettingName::CrosshairADSColor", value = Utils.ColorToString(Color) }).ToList();
                    UserSettings.stringSettings = DummySettings.ToArray();
                }
                FetchedProfiles.Profiles[SelectedIndex].bUseAdvancedOptions = true;
                FetchedProfiles.Profiles[SelectedIndex].Primary.Color = NewColor;
                FetchedProfiles.Profiles[SelectedIndex].aDS = FetchedProfiles.Profiles[SelectedIndex].Primary;
                if (FetchedProfiles.Profiles[SelectedIndex].Sniper == null)
                {
                    FetchedProfiles.Profiles[SelectedIndex].Sniper = new SniperSettings
                    {
                        bDisplayCenterDot = true,
                        CenterDotColor = NewColor,
                        CenterDotOpacity = 1,
                        CenterDotSize = 1
                    };
                }
                else
                    FetchedProfiles.Profiles[SelectedIndex].Sniper.CenterDotColor = NewColor;

                FetchedProfiles.Profiles[SelectedIndex].ProfileName = ProfileName;
                UserSettings.stringSettings[SavedProfilesIndex].value = JsonConvert.SerializeObject(FetchedProfiles);
            }
            else
            {
                int NameIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairProfileName");
                UserSettings.stringSettings[SavedProfilesIndex].value = Utils.ColorToString(Color);
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
