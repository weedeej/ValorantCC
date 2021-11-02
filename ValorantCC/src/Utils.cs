using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using ValorantCC;
using System.Reflection;

namespace Utilities
{
    struct GithubResponse
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("assets")]
        public List<Asset> Assets { get; set; }
    }
    struct Asset
    {
        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    class Utils
    {
        private static StringBuilder StringBuilder = new StringBuilder();
        private static WebClient client = new WebClient();
        public static Data Decompress(string value)
        {
            Log("Decompressing Response Data");
            byte[] byteArray = Convert.FromBase64String(value);
            var outputStream = new MemoryStream();
            DeflateStream deflateStream = new DeflateStream(new MemoryStream(byteArray), CompressionMode.Decompress);
            deflateStream.CopyTo(outputStream);

            return JsonConvert.DeserializeObject<Data>(Encoding.UTF8.GetString(outputStream.ToArray()));
        }

        public static string Compress(Object data)
        {
            Log("Compressing New Data");
            string jsonString = JsonConvert.SerializeObject(data);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            //Create stream from bytes of the json data and copy it to deflateStream as it only has write access
            MemoryStream memoryStream = new MemoryStream(byteArray);
            MemoryStream output = new MemoryStream();
            DeflateStream deflateStream = new DeflateStream(output, CompressionMode.Compress);
            memoryStream.CopyTo(deflateStream);
            deflateStream.Close();
            return Convert.ToBase64String(output.ToArray());
        }

        public static CrosshairColor parseCrosshairColor(string input)
        {
            Log("Parsing `CrosshairColor` to String");
            Regex rx = new Regex(@"(?=)\d+");
            List<Match> rgba = rx.Matches(input).ToList();

            return new CrosshairColor
            {
                R = (byte)Int32.Parse(rgba[0].Value),
                G = (byte)Int32.Parse(rgba[1].Value),
                B = (byte)Int32.Parse(rgba[2].Value),
                A = (byte)Int32.Parse(rgba[3].Value)
            };
        }

        public static string ColorToString(Color Color)
        {
            Log("`CrosshairColor` Reverse");
            return $"(R={Color.R},G={Color.G},B={Color.B},A={Color.A})";
        }

        public static Dictionary<string, string> ConstructHeaders(AuthResponse auth)
        {
            Log("Constructing Headers");
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Bearer " + auth.AuthTokens.AccessToken);
            headers.Add("X-Riot-Entitlements-JWT", auth.AuthTokens.Token);
            headers.Add("X-Riot-ClientVersion", auth.Version);
            headers.Add("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
            return headers;
        }

        public static void Log(string Text)
        {
            StringBuilder.Append($"{DateTimeOffset.UtcNow} | {Text}\n");
            File.AppendAllText(Directory.GetCurrentDirectory() + "/logs.txt", StringBuilder.ToString());
            StringBuilder.Clear();
        }

        public static bool CheckLatest()
        {
            Log("Checking Github releases");
            IEnumerable<string> Executables = GetAllFiles(".", "*.exe");
            Trace.WriteLine(Executables.First());
            foreach(string Executable in Executables)
            {
                Log("Executable Detected: " + Executable);
                if (Executable != AppDomain.CurrentDomain.FriendlyName)
                {
                    try
                    {
                        File.Delete(Directory.GetCurrentDirectory() + "/" + Executable);
                        Log("Deleted Executable: " + Executable);
                    }catch(Exception e)
                    {
                        Log("Error occured: " + e.StackTrace);
                        continue;
                    }
                }
            }

            client.Headers = new WebHeaderCollection(){ {"User-Agent","ValorantCC UserAgent"} };
            string ContentString = client.DownloadString("https://api.github.com/repos/weedeej/ValorantCC/releases/latest");
            GithubResponse ResponseObj = JsonConvert.DeserializeObject<GithubResponse>(ContentString);
            Log("Latest Data: " + ResponseObj.Name + " | URL: " + ResponseObj.Assets[0].BrowserDownloadUrl);
            if (new Version(ResponseObj.TagName) > Assembly.GetExecutingAssembly().GetName().Version)
            {
                DownloadRelease(ResponseObj.Assets[0].BrowserDownloadUrl, ResponseObj.TagName);
                return true;
            }
            return false;
        }

        private static void DownloadRelease(string url, string tag)
        {
            client.DownloadFile(url, $"ValorantCC-{tag}.exe");
            Log("Latest Release Downloaded!");
        }

        private static IEnumerable<string> GetAllFiles(string path, string searchPattern)
        {
            Log("Obtaining Executable names.");
            return Directory.EnumerateFiles(path, searchPattern).Union(
            Directory.EnumerateDirectories(path).SelectMany(d =>
            {
                try
                {
                    return GetAllFiles(d, searchPattern);
                }
                catch (Exception e)
                {
                    return Enumerable.Empty<string>();
                }
            }));
            }
        }
    }
