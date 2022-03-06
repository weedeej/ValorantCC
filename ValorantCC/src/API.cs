using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;
using ValorantCC.src.Community;

namespace ValorantCC
{
    public struct SetCallResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public struct FetchResponse
    {
        public bool success { get; set; }
        public List<ShareableProfile> data { get; set; }
        public string message { get; set; }
        public int offset { get; set; }
    }

    public struct ShareableProfile
    {
        public String shareCode { get; set; }
        public String settings { get; set; }
        public bool shareable { get; set; }
        public String displayName { get; set; }
    }

    public partial class Payload
    {
        public String settings { get; set; }
        public bool shareable { get; set; }
        public String sharecode { get; set; }
    }
    public class API
    {
        private static AuthTokens AuthTokens;
        private static RestClient client = new RestClient("https://vtools-next.vercel.app/api");

        public CrosshairProfile profile;
        public bool Shareable;
        public API(AuthTokens Tokens, CrosshairProfile TargetProfile, int ActionInt, bool isShareable)
        {
            AuthTokens = Tokens;
            profile = TargetProfile;
            Shareable = isShareable;
        }

        public async Task<FetchResponse> Fetch(String sharecode = null, int Offset = 0)
        {
            RestRequest request;
            if (sharecode != null)
                request = new RestRequest($"/sharecode/{sharecode}", Method.Get);
            else
                request = new RestRequest($"/profiles?offset={Offset}", Method.Get);

            RestResponse response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                Utils.Log(response.Content.ToString());
                return new FetchResponse() { success = false };
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new FetchResponse() { success = true, data = new List<ShareableProfile>(), offset = Offset };
            }

            return new FetchResponse()
            {
                success = true,
                data = JsonConvert.DeserializeObject<List<ShareableProfile>>(response.Content),
                offset = Offset
            };
        }

        public async Task<FetchResponse> ObtainSelfSaved()
        {
            RestRequest request = new RestRequest($"/profiles/{AuthTokens.Subject}/fetch", Method.Get);

            RestResponse response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                Utils.Log(response.Content.ToString());
                return new FetchResponse() { success = false, message = response.Content.ToString() };
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new FetchResponse() { success = true, message = response.Content.ToString() };

            return new FetchResponse()
            {
                success = true,
                data = new List<ShareableProfile>() { JsonConvert.DeserializeObject<ShareableProfile>(response.Content) }
            };
        }

        public async Task<SetCallResponse> Set()
        {
            Payload payload = new Payload()
            {
                settings = JsonConvert.SerializeObject(profile),
                shareable = Shareable,
                sharecode = SCGen.GenerateShareCode(AuthTokens.Subject)
            };
            RestRequest request = new RestRequest($"/profiles/{AuthTokens.Subject}/set", Method.Put) { RequestFormat = DataFormat.Json };
            request.AddJsonBody(payload);
            request.AddOrUpdateHeader("Authorization", $"Bearer {AuthTokens.AccessToken}"); // Pass to server so nobody can set somebody's saved profile.
            RestResponse response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                Utils.Log(response.Content.ToString());
                int retry = 0;
                while (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    await Task.Delay(500);
                    if (response.StatusCode != System.Net.HttpStatusCode.Conflict) break;
                    // Retry until 5 retries or removed the conflict.
                    payload.sharecode = SCGen.GenerateShareCode(AuthTokens.Subject);
                    request = new RestRequest($"/profiles/{AuthTokens.Subject}/set", Method.Put) { RequestFormat = DataFormat.Json };
                    request.AddJsonBody(payload);
                    request.AddOrUpdateHeader("Authorization", $"Bearer {AuthTokens.AccessToken}");
                    response = await client.ExecuteAsync(request);
                    retry++;
                    Utils.Log($"Conflict removal retry: {retry}: {response.Content}");
                    if (retry >= 5) break;
                }
                if (response.StatusCode != System.Net.HttpStatusCode.Conflict)
                {
                    Utils.Log(response.Content.ToString());
                    return new SetCallResponse() { success = false, message = response.Content.ToString() };
                }
                Utils.Log($"Failure to remove the conflict. Sharecode: {payload.sharecode} | {response.Content}");
                return new SetCallResponse() { success = false, message = response.Content.ToString() };
            }
            return new SetCallResponse() { success = true, message = payload.sharecode };
        }

    }
}
