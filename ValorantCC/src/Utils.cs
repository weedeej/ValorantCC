using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using ValorantCC;

namespace Utilities
{
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
            MemoryStream output = new MemoryStream();
            //Create stream from bytes of the json data and copy it to deflateStream as it only has write access
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(byteArray))
                {
                    DeflateStream deflateStream = new DeflateStream(output, CompressionMode.Compress);
                    memoryStream.CopyTo(deflateStream);
                    deflateStream.Close();
                }
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
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
            headers.Add("Authorization", $"Bearer {auth.AuthTokens.AccessToken}");
            headers.Add("X-Riot-Entitlements-JWT", auth.AuthTokens.Token);
            headers.Add("X-Riot-ClientVersion", auth.Version);
            headers.Add("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
            return headers;
        }

        public static void Log(string Text)
        {
            StringBuilder.Append($"{DateTimeOffset.Now} | {Text}\n");
            File.AppendAllText(Environment.GetEnvironmentVariable("LocalAppData") + "\\VTools\\Logs\\logs.txt", StringBuilder.ToString());
            StringBuilder.Clear();
        }
        public static string LoginResponse(Processor processor)
        {
            if (processor.ProfileListed) return "Logged In! You may now close Valorant.";
            return "Logged In! You may now close Valorant. NOTE: You only have 1 Profile. To use the other features, Please create an extra profile.";
        }
    }
}
