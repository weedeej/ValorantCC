using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ValorantCC.src;
namespace ValorantCC
{
    public class Processor
    {
        private AuthResponse AuthResponse;
        public bool isLoggedIn = false;
        private RestClient client = new RestClient("https://playerpreferences.riotgames.com");
        private Data UserSettings;
        public bool ProfileListed;
        public List<string> ProfileNames;
        private ProfileList FetchedProfiles;
        public int CurrentProfile;
        //Haruki's "bug" fix
        private Color[] DefaultColors = new Color[8];

        public Processor()
        {
            DefaultColors[0] = new Color { R = 0, G = 255, B = 0 }; //Green
            DefaultColors[1] = new Color { R = 127, G = 255, B = 0 }; //Greenish yellow
            DefaultColors[2] = new Color { R = 223, G = 255, B = 0 }; //Yellowish green
            DefaultColors[3] = new Color { R = 255, G = 255, B = 0 }; //Yellow
            DefaultColors[4] = new Color { R = 0, G = 255, B = 255 }; //Cyan
            DefaultColors[5] = new Color { R = 255, G = 0, B = 255 }; //Pink
            DefaultColors[6] = new Color { R = 255, G = 0, B = 0 }; //Red
            DefaultColors[7] = new Color { R = 255, G = 255, B = 255 }; //White
        }

        public async Task<AuthResponse> Login()
        {
            Utilities.Utils.Log("Login started");
            AuthObj AuthObj = new AuthObj();
            AuthResponse = await AuthObj.StartAuth();
            if (!AuthResponse.Success) return AuthResponse;
            Utilities.Utils.Log("Auth Success");
            await Construct();
            return AuthResponse;
        }

        public async Task<bool> Construct()
        {
            Utilities.Utils.Log("Constructing Properties -->");

            UserSettings = await FetchUserSettings();
            if (UserSettings.settingsProfiles == null) return false;

            ProfileListed = CheckIfList(UserSettings);
            Utilities.Utils.Log($"Multiple Profiles: {ProfileListed}");
            Stringsetting SavedProfiles;
            if (ProfileListed)
            {
                SavedProfiles = UserSettings.stringSettings.FirstOrDefault(setting => setting.settingEnum == "EAresStringSettingName::SavedCrosshairProfileData");
                FetchedProfiles = FetchProfiles(SavedProfiles.value, null, null ,null ,null);
            }
            else
            {
                Stringsetting Primary, PrimaryOutline, aDS, aDSOutline, sniperDot;

                try
                {
                    Primary = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
                    PrimaryOutline = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairOutlineColor");
                    aDS = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSColor");
                    aDSOutline = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSOutlineColor");
                    sniperDot = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairSniperCenterDotColor");
                }
                catch
                {
                    Utilities.Utils.Log("User is new account/Using White Color");
                    UserSettings.stringSettings.Add(new Stringsetting
                    {
                        settingEnum = "EAresStringSettingName::CrosshairColor",
                        value = "(R=0,G=0,B=0,A=255)"
                    });
                    UserSettings.stringSettings.Add(new Stringsetting
                    {
                        settingEnum = "EAresStringSettingName::CrosshairOutlineColor",
                        value = "(R=255,G=255,B=255,A=255)"
                    });
                    UserSettings.stringSettings.Add(new Stringsetting
                    {
                        settingEnum = "EAresStringSettingName::CrosshairADSColor",
                        value = "(R=0,G=0,B=0,A=255)"
                    });
                    UserSettings.stringSettings.Add(new Stringsetting
                    {
                        settingEnum = "EAresStringSettingName::CrosshairADSOutlineColor",
                        value = "(R=255,G=255,B=255,A=255)"
                    });
                    UserSettings.stringSettings.Add(new Stringsetting
                    {
                        settingEnum = "EAresStringSettingName::CrosshairSniperCenterDotColor",
                        value = "(R=254,G=254,B=254,A=255)"
                    });

                    Primary = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairColor");
                    PrimaryOutline = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairOutlineColor");
                    aDS = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSColor");
                    aDSOutline = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairADSOutlineColor");
                    sniperDot = UserSettings.stringSettings.First(setting => setting.settingEnum == "EAresStringSettingName::CrosshairSniperCenterDotColor");
                }
                FetchedProfiles = FetchProfiles(Primary.value, PrimaryOutline.value, aDS.value, aDSOutline.value, sniperDot.value);
            }

            ProfileNames = FetchProfileNames(FetchedProfiles);
            CurrentProfile = FetchedProfiles.CurrentProfile;

            isLoggedIn = true;
            Utilities.Utils.Log("<-- Constructing Properties");
            return true;
        }

        private bool CheckIfList(Data settings)
        {
            Utilities.Utils.Log("Checking if User has multiple profiles");
            return settings.stringSettings.Any(setting => setting.settingEnum == "EAresStringSettingName::SavedCrosshairProfileData");
        }

        private async Task<Data> FetchUserSettings()
        {
            Utilities.Utils.Log("Obtaining User Settings");
            RestRequest request = new RestRequest("/playerPref/v3/getPreference/Ares.PlayerSettings", Method.Get);
            request.AddHeaders(Utilities.Utils.ConstructHeaders(AuthResponse));
            string responseContent = (await client.ExecuteAsync(request)).Content;
            RestResponse resp = await client.ExecuteAsync(request);
            Dictionary<string, object> response = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
            Data settings = new Data();
            try
            {
                settings = Utilities.Utils.Decompress(Convert.ToString(response["data"]));
            }
            catch (KeyNotFoundException)
            {
                return settings;
            }

            return settings;
        }

        private async Task<bool> putUserSettings(Data newData)
        {
            Utilities.Utils.Log("Saving New Data: (BACKUP) " + JsonConvert.SerializeObject(newData));

            RestRequest request = new RestRequest("/playerPref/v3/savePreference", Method.Put);
            request.AddHeaders(Utilities.Utils.ConstructHeaders(AuthResponse));
            request.AddJsonBody(new { type = "Ares.PlayerSettings", data = Utilities.Utils.Compress(newData) });

            RestResponse response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                Utilities.Utils.Log("savePreference Unsuccessfull: " + response.Content.ToString());
                return false;
            }

            Dictionary<string, object> responseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
            if (responseDict.ContainsKey("data")) return true;
            Utilities.Utils.Log("savePreference Unsuccessfull: " + response.Content.ToString());
            return false;
        }

        private ProfileList FetchProfiles(string SettingValue, string? PrimOutlineColor, string? ADSColorValue, string? ADSOutlineColor, string? SniperCenterdotColor)
        {
            string DefaultUserSettings = JsonConvert.SerializeObject(UserSettings);
            Utilities.Utils.Log("Fetching/Creating Profile/s");
            Utilities.Utils.Log($"Setting Value: {DefaultUserSettings}");
            if (ProfileListed) return JsonConvert.DeserializeObject<ProfileList>(SettingValue);

            CrosshairColor PrimaryColor = Utilities.Utils.parseCrosshairColor(SettingValue);
            CrosshairColor PrimaryOutlineColor = Utilities.Utils.parseCrosshairColor(PrimOutlineColor);
            CrosshairColor ADSColor = Utilities.Utils.parseCrosshairColor(ADSColorValue);
            CrosshairColor aDSOutlineColor = Utilities.Utils.parseCrosshairColor(ADSOutlineColor);
            CrosshairColor sniperCenterdotColor = Utilities.Utils.parseCrosshairColor(SniperCenterdotColor);    
            string profileName = UserSettings.stringSettings.FirstOrDefault(setting => setting.settingEnum == "EAresStringSettingName::CrosshairProfileName").value;

            return new ProfileList
            {
                CurrentProfile = 0,
                Profiles = new List<CrosshairProfile>
                {
                    new CrosshairProfile
                    {
                        Primary = new ProfileSettings {Color = PrimaryColor, OutlineColor = PrimaryOutlineColor, InnerLines = new LineSettings(), OuterLines = new LineSettings(), bHasOutline = true, OutlineOpacity = 1, OutlineThickness = 1},
                        aDS = new ProfileSettings {Color = ADSColor, OutlineColor = aDSOutlineColor, InnerLines = new LineSettings(), OuterLines = new LineSettings(), bHasOutline = true, OutlineOpacity = 1, OutlineThickness = 1},
                        Sniper = new SniperSettings { CenterDotColor = sniperCenterdotColor, CenterDotSize = 1, CenterDotOpacity = 1, bDisplayCenterDot = true},
                        bUseAdvancedOptions = true,
                        ProfileName = profileName
                    }
                }
            };
        }

        private List<string> FetchProfileNames(ProfileList ProfileList)
        {
            Utilities.Utils.Log("Fetching Profile names");
            if (ProfileListed) return (from profile in ProfileList.Profiles
                                       select profile.ProfileName).ToList();

            string profileName = UserSettings.stringSettings.FirstOrDefault(setting => setting.settingEnum == "EAresStringSettingName::CrosshairProfileName").value;
            return new List<string> { profileName };
        }

        public CrosshairProfile ProfileFromIndex(int Index)
        {
            Utilities.Utils.Log($"Obtained Profile from index: {FetchedProfiles.Profiles[Index].ProfileName}");
            return FetchedProfiles.Profiles[Index];
        }

        private void SaveListedSettings(List<Color> Colors, int SelectedIndex)
        {
            if (SelectedIndex == FetchedProfiles.CurrentProfile)
            {
                try
                {
                    UserSettings = Modifier.ChangeActiveProfile(Colors, SelectedIndex, UserSettings, FetchedProfiles);
                }
                catch (Exception e)
                {
                    Utilities.Utils.Log($"Error occured: {e}");
                }

            }
            Utilities.Utils.Log("Modifying Selected Profile.");
            CrosshairProfile currentProfile = FetchedProfiles.Profiles[SelectedIndex];

            currentProfile.bUseAdvancedOptions = true;
            currentProfile.bUsePrimaryCrosshairForADS = false;
            currentProfile.Primary.Color = new CrosshairColor { R = Colors[0].R, G = Colors[0].G, B = Colors[0].B, A = 255 };
            currentProfile.Primary.OutlineColor = new CrosshairColor { R = Colors[1].R, G = Colors[1].G, B = Colors[1].B, A = 255 };
            currentProfile.aDS.Color = new CrosshairColor { R = Colors[2].R, G = Colors[2].G, B = Colors[2].B, A = 255 };
            currentProfile.aDS.OutlineColor = new CrosshairColor { R = Colors[3].R, G = Colors[3].G, B = Colors[3].B, A = 255 };

            if (currentProfile.Sniper == null)
            {
                currentProfile.Sniper = new SniperSettings
                {
                    bDisplayCenterDot = true,
                    CenterDotColor = new CrosshairColor { R = Colors[4].R, G = Colors[4].G, B = Colors[4].B, A = 255 },
                    CenterDotOpacity = 1,
                    CenterDotSize = 1
                };
            }
            else
                currentProfile.Sniper.CenterDotColor = new CrosshairColor { R = Colors[4].R, G = Colors[4].G, B = Colors[4].B, A = 255 };
        }
        public async Task<bool> SaveNewColor(List<Color> Colors, int SelectedIndex, string ProfileName)
        {
            Utilities.Utils.Log("Save button clicked. Saving...");

            if (ProfileListed)
            {
                Utilities.Utils.Log("Profile type: List");
                SaveListedSettings(Colors, SelectedIndex);

                for (int i = 0; i < FetchedProfiles.Profiles.Count; i++)
                {
                    var item = FetchedProfiles.Profiles[i];
                    for (int x = 0; x < DefaultColors.Length; x++)
                    {
                        var color = DefaultColors[x];
                        CheckDefaultColor(item.Primary.Color, color);
                        CheckDefaultColor(item.aDS.Color, color);
                        CheckDefaultColor(item.Sniper.CenterDotColor, color);
                    }
                }

                FetchedProfiles.Profiles[SelectedIndex].ProfileName = ProfileName;
                Stringsetting SavedProfiles = UserSettings.stringSettings.FirstOrDefault(setting => setting.settingEnum == "EAresStringSettingName::SavedCrosshairProfileData");
                SavedProfiles.value = JsonConvert.SerializeObject(FetchedProfiles);
            }
            else
            {
                Utilities.Utils.Log("Profile type: Enum");
                Stringsetting profileName = UserSettings.stringSettings.FirstOrDefault(setting => setting.settingEnum == "EAresStringSettingName::CrosshairProfileName");
                UserSettings = Modifier.ChangeActiveProfile(Colors, SelectedIndex, UserSettings, FetchedProfiles);
                profileName.value = ProfileName;

            }
            if (await putUserSettings(UserSettings))
            {
                Utilities.Utils.Log("Reconstructing Data");
                await Construct();
                return true;
            }
            return false;
        }

        private void CheckDefaultColor(CrosshairColor prfColor, Color color)
        {
            if (prfColor.R == color.R && prfColor.G == color.G && prfColor.B == color.B)
                if (prfColor.R != 0)
                    prfColor.R--;
                else
                    prfColor.G--;
        }
    }
}
