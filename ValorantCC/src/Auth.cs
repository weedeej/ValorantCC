using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
namespace ValorantCC
{
    public partial class AuthTokens
    {
        public string AccessToken { get; set; }
        public string Subject { get; set; }
        public string Token { get; set; }
        public bool Success { get; set; } = false;
    }

    public partial class LockfileData
    {
        public string Client { get; set; }
        public int PID { get; set; }
        public int Port { get; set; }
        public string Key { get; set; }
        public string Protocol { get; set; }
        public string Basic { get; set; }
        public bool Success { get; set; } = false;
    }

    public partial class AuthResponse
    {
        public bool Success { get; set; }
        public string Response { get; set; }
        public string Version { get; set; }
        public LockfileData LockfileData { get; set; }
        public AuthTokens AuthTokens { get; set; }
    }

    public partial class VersionResponse
    {
        public int Status { get; set; }
        public string riotClientVersion { get; set; }

    }

    public class AuthObj
    {
        LockfileData LocalCredentials;
        static RestClient client = new RestClient(new RestClientOptions() { RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true });
        public async Task<AuthResponse> StartAuth()
        {
            string LockfilePath = Environment.GetEnvironmentVariable("LocalAppData") + "\\Riot Games\\Riot Client\\Config\\lockfile";

            LocalCredentials = ObtainLockfileData(LockfilePath);
            if (!LocalCredentials.Success) return new AuthResponse() { Success = false, Response = "Please login to Riot Client or Start Valorant." };

            AuthTokens Tokens = await ObtainAuthTokens();
            if (!Tokens.Success) return new AuthResponse() { Success = false, Response = "Please login to Riot Client or Start Valorant." };
            return new AuthResponse() { Success = true, AuthTokens = Tokens, LockfileData = LocalCredentials, Version = await GetVersion() };

        }

        public static bool CheckLockFile(string LockfilePath)
        {
            return File.Exists(LockfilePath);
        }

        public static LockfileData ObtainLockfileData(string LockfilePath)
        {
            string LockfileRaw;
            try
            {
                Utilities.Utils.Log("Trying to open Lockfile");
                using (FileStream File = new FileStream(LockfilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader Reader = new StreamReader(File, Encoding.UTF8))
                    {
                        LockfileRaw = (String)Reader.ReadToEnd().Clone();
                        File.Close();
                        Reader.Close();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Utilities.Utils.Log("Lockfile not found");
                return new LockfileData();
            }

            object[] LockfileData = LockfileRaw.Split(":");
            return new LockfileData()
            {
                Client = (string)LockfileData[0],
                PID = Int32.Parse((string)LockfileData[1]),
                Port = Int32.Parse((string)LockfileData[2]),
                Key = (string)LockfileData[3],
                Protocol = (string)LockfileData[4],
                Basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{(string)LockfileData[3]}")),
                Success = true
            };
        }

        private async Task<AuthTokens> ObtainAuthTokens()
        {
            Utilities.Utils.Log("Creating Auth Request");
            RestRequest request = new RestRequest($"https://127.0.0.1:{LocalCredentials.Port}/entitlements/v1/token", Method.Get);
            request.AddHeader("Authorization", $"Basic {LocalCredentials.Basic}");

            RestResponse response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) return new AuthTokens();

            AuthTokens tokens = JsonConvert.DeserializeObject<AuthTokens>(response.Content.ToString());
            tokens.Success = true;
            return tokens;
        }

        private async static Task<String> GetVersion()
        {
            string ver = "release-04.04-shipping-16-679250";
            Utilities.Utils.Log("Obtaining Client Version info");
            RestRequest request = new RestRequest("https://vtools-next.vercel.app/api/skinslist/version", Method.Get);

            RestResponse response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) return ver;
            VersionResponse RespData;
            try
            {
                RespData = JsonConvert.DeserializeObject<VersionResponse>(response.Content.ToString());
            } catch (JsonReaderException)
            {
                return ver;
            }
            RespData.Status = ((int)response.StatusCode);
            return RespData.riotClientVersion;
        }
    }
}
