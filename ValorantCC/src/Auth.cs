using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
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
        public AuthResponse StartAuth(bool wait = false)
        {
            string LockfilePath = Environment.GetEnvironmentVariable("LocalAppData") + "\\Riot Games\\Riot Client\\Config\\lockfile";

            if (wait)
            {
                Utils.Log("Waiting for Lockfile");

                bool LockfileExists = CheckLockFile(LockfilePath);
                while (!LockfileExists)
                {
                    LockfileExists = CheckLockFile(LockfilePath);
                }
            }

            LockfileData LocalCredentials = ObtainLockfileData(LockfilePath);
            if (!LocalCredentials.Success) return new AuthResponse() { Success = false, Response = "Please login to Riot Client or Start Valorant." };

            AuthTokens Tokens = ObtainAuthTokens(LocalCredentials.Basic, LocalCredentials.Port);
            if (!Tokens.Success) return new AuthResponse() { Success = false, Response = "Please login to Riot Client or Start Valorant." };
            return new AuthResponse() { Success = true, AuthTokens = Tokens, LockfileData = LocalCredentials, Version = GetVersion() };

        }

        private static bool CheckLockFile(string LockfilePath)
        {
            return File.Exists(LockfilePath);
        }

        private static LockfileData ObtainLockfileData(string LockfilePath)
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

        private AuthTokens ObtainAuthTokens(string Basic, int port)
        {
            Utils.Log("Creating Auth Request");

            HttpWebRequest request = HttpWebRequest.CreateHttp($"https://127.0.0.1:{port}/entitlements/v1/token");
            request.Method = "GET";
            request.Headers.Add("Authorization", $"Basic {Basic}");
            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            HttpWebResponse resp;
            try
            {
                Utils.Log("Sending Auth Request");
                resp = (HttpWebResponse)request.GetResponse();
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

        private static string GetVersion()
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp("https://valorant-api.com/v1/version");
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            VersionResponse RespData = JsonConvert.DeserializeObject<VersionResponse>(sr.ReadToEnd());
            return RespData.Data.RiotClientVersion;
        }
    }
}
