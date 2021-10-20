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
        private int DefaultProfileIndex = 0;
        private bool ProfileListed;
        public List<string> ProfileNames;
        private ProfileList FetchedProfiles;
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

        private void Construct()
        {
            Utils.Log("Constructing Properties -->");

            client.AddDefaultHeaders(Utils.ConstructHeaders(AuthResponse));
            UserSettings = FetchUserSettings();
            ProfileListed = CheckIfList(UserSettings);
            Utils.Log("Multiple Profiles: "+ProfileListed.ToString());
            if (ProfileListed)
            {
                SavedProfilesIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::SavedCrosshairProfileData");
                DefaultProfileIndex = UserSettings.stringSettings.ToList().FindIndex(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
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
                        Primary = new Primary {Color = ParsedColor},
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

        public bool SaveNewColor(Color Color, int Index)
        {
            Utils.Log("Save buutton clicked. Saving...");
            if (ProfileListed)
            {
                FetchedProfiles.Profiles[Index].Primary.Color = new CrosshairColor
                {
                    R = Color.R,
                    G = Color.G,
                    B = Color.B,
                    A = 255
                };
                UserSettings.stringSettings[SavedProfilesIndex].value = JsonConvert.SerializeObject(FetchedProfiles);
                if (Index == 0)
                {
                    UserSettings.stringSettings[DefaultProfileIndex].value = Utils.ColorToString(Color);
                }
                

            }
            else
            {
                UserSettings.stringSettings[SavedProfilesIndex].value = Utils.ColorToString(Color);
            }
            if (putUserSettings(UserSettings)) return true;
            return false;
        }
    }
}
