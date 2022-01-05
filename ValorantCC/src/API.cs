using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ValorantCC
{
    public struct VoidCallResponse
    {
        public bool success { get; set; }
        public Object data { get; set; }
    }

    public struct FetchResponse
    {
        public bool success { get; set; }
        public List<ShareableProfile> data { get; set; }
    }

    public struct ShareableProfile
    {
        public String shareCode { get; set; }
        public String settings { get; set; }
        public bool shareable { get; set; }
        public String displayName { get; set; }
    }

    public partial class PostPayload
    {
        public String subject { get; set; }
        public String settings { get; set; }
        public bool shareable { get; set; }
        public String sharecode { get; set; } = null;
        public int action { get; set; }
    }
    public class API
    {
        private static AuthTokens AuthTokens;
        private static RestClient client = new RestClient("https://valorantcc.000webhostapp.com/api.php");

        public CrosshairProfile profile;
        public int Action;
        public bool Shareable;
        public API(AuthTokens Tokens, CrosshairProfile TargetProfile ,int ActionInt, bool isShareable)
        {
            AuthTokens = Tokens;
            profile = TargetProfile;
            Action = ActionInt;
            Shareable = isShareable;
        }

        public FetchResponse Fetch(String sharecode = null)
        {
            PostPayload payload = new PostPayload()
            {
                subject = AuthTokens.Subject,
                settings = JsonConvert.SerializeObject(profile),
                shareable = Shareable,
                action = Action
            };
            if (sharecode != null)
            {
                payload.sharecode = sharecode;
                payload.action = Action;
            }
            RestRequest request = new RestRequest() { Method = Method.POST};
            request.AddJsonBody(payload);
            RestResponse response = (RestResponse)client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return new FetchResponse() { success = false };
            return JsonConvert.DeserializeObject<FetchResponse>(Regex.Unescape(response.Content));
        }

        public VoidCallResponse Set()
        {
            PostPayload payload = new PostPayload()
            {
                subject = AuthTokens.Subject,
                settings = JsonConvert.SerializeObject(profile),
                shareable = Shareable,
                action = Action
            };
            RestRequest request = new RestRequest() { Method = Method.POST };
            request.AddJsonBody(payload);
            request.AddHeader("Authorization", $"Bearer {AuthTokens.AccessToken}"); // Pass to server so nobody can set somebody's saved profile.
            RestResponse response = (RestResponse)client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return new VoidCallResponse() { success = false };
            return JsonConvert.DeserializeObject<VoidCallResponse>(Regex.Unescape(response.Content));
        }

    }
}
