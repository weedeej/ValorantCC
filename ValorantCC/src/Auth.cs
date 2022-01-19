using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
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
        public int Status;
        public VersionData Data;

    }
    public partial class VersionData
    {
        public string RiotClientVersion { get; set; }
        public string ManifestID { get; set; }
        public string Version { get; set; }
        public string BuildVersion { get; set; }
        public string Branch { get; set; }
        public string BuildDate { get; set; }
    }
    public class AuthObj
    {
        LockfileData LocalCredentials;
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
                Utils.Log("Trying to open Lockfile");
                FileStream File = new FileStream(LockfilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader Reader = new StreamReader(File, Encoding.UTF8);
                LockfileRaw = Reader.ReadToEnd();
                File.Close();
                Reader.Close();
            }
            catch (FileNotFoundException)
            {
                Utils.Log("Lockfile not found");
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
            await Task.Delay(1);
            Utils.Log("Creating Auth Request");

            HttpWebRequest request = HttpWebRequest.CreateHttp($"https://127.0.0.1:{LocalCredentials.Port}/entitlements/v1/token");
            request.Method = "GET";
            request.Headers.Add("Authorization", $"Basic {LocalCredentials.Basic}");
            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            WebResponse resp;
            try
            {
                Utils.Log("Sending Auth Request");
                Task<WebResponse> taskResp = Task.Factory.FromAsync(
                    request.BeginGetResponse, request.EndGetResponse,
                    (object)null);
                resp = await taskResp;
            }
            catch (WebException ex)
            {
                Utils.Log($"Auth Error {ex}");
                return new AuthTokens();
            }

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            AuthTokens tokens = JsonConvert.DeserializeObject<AuthTokens>(sr.ReadToEnd());
            tokens.Success = true;
            return tokens;
        }

        private async static Task<String> GetVersion()
        {
            await Task.Delay(1);
            HttpWebRequest request = HttpWebRequest.CreateHttp("https://valorant-api.com/v1/version");
            request.Method = "GET";
            Task<WebResponse> taskResp = Task.Factory.FromAsync(
                    request.BeginGetResponse,
                    asyncResult => request.EndGetResponse(asyncResult),
                    (object)null);
            VersionResponse RespData = await taskResp.ContinueWith(r =>
            {   
                WebResponse response = (WebResponse)r.Result;
                StreamReader sr = new StreamReader(response.GetResponseStream());
                return JsonConvert.DeserializeObject<VersionResponse>(sr.ReadToEnd());
            });
            return RespData.Data.RiotClientVersion;
        }
    }
}
