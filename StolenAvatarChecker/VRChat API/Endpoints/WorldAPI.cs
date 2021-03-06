using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRChatApi.Classes;


namespace VRChatApi.Endpoints {
    public class WorldApi {

        public async Task<IList<String>> GetUsersInInstance(string instance) {
            Console.WriteLine("Fetching friends from instance: " + instance);
            HttpResponseMessage response = await Global.HttpClient.GetAsync($"worlds/{instance}");

            string res = await response.Content.ReadAsStringAsync();
            Console.WriteLine(res);
            string[] resSplit = res.Split('"');
            IList<String> resIDs = new List<String>();
            foreach (String s in resSplit) {
                if (s.Contains("usr_"))
                    resIDs.Add(s);
            }

            return resIDs;
        }

        public async Task<WorldResponse> Get(string id) {
            Console.WriteLine($"Getting world info with ID: {id}");

            HttpResponseMessage response = await Global.HttpClient.GetAsync($"worlds/{id}?apiKey={Global.ApiKey}");

            WorldResponse res = null;

            if (response.IsSuccessStatusCode) {
                string json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON received: {json}");
                res = JsonConvert.DeserializeObject<WorldResponse>(json);

                // Parse instances.
                res.instances = res._instances.Select(data => new WorldInstance() {
                    id = (string)data[0],
                    occupants = (int)data[1]
                }).ToList();
            }

            return res;
        }

        public async Task<List<WorldBriefResponse>> Search(WorldGroups? endpoint = null, bool? featured = null,
            SortOptions? sort = null, UserOptions? user = null,
            string userId = null, string keyword = null, string tags = null, string excludeTags = null,
            ReleaseStatus? releaseStatus = null, int offset = 0, int count = 20) {
            Console.WriteLine( "Getting world list");
            var param = new StringBuilder();
            param.Append($"&n={count}");
            param.Append($"&offset={offset}");

            if (featured.HasValue) {
                param.Append($"&featured={featured.Value}");

                if (featured.Value && sort.HasValue == false) {
                    param.Append("&sort=order");
                }
            }

            if (sort.HasValue) {
                param.Append($"&sort={sort.Value.ToString().ToLowerInvariant()}");

                if (sort.Value == SortOptions.Popularity && featured.HasValue == false) {
                    param.Append("&featured=false");
                }
            }

            if (user.HasValue)
                param.Append($"&user={user.Value.ToString().ToLowerInvariant()}");
            if (!string.IsNullOrEmpty(userId))
                param.Append($"&userId={userId}");
            if (!string.IsNullOrEmpty(keyword))
                param.Append($"&search={keyword}");
            if (!string.IsNullOrEmpty(tags))
                param.Append($"&tag={tags}");
            if (!string.IsNullOrEmpty(excludeTags))
                param.Append($"&notag={excludeTags}");
            if (releaseStatus.HasValue)
                param.Append($"&releaseStatus={releaseStatus.Value.ToString().ToLowerInvariant()}");

            string baseUrl = "worlds";
            if (endpoint.HasValue) {
                switch (endpoint.Value) {
                    case WorldGroups.Active:
                        baseUrl = "worlds/active";
                        break;
                    case WorldGroups.Recent:
                        baseUrl = "worlds/recent";
                        break;
                    case WorldGroups.Favorite:
                        baseUrl = "worlds/favorites";
                        break;
                }
            }

            HttpResponseMessage response = await Global.HttpClient.GetAsync($"{baseUrl}?apiKey={Global.ApiKey}{param.ToString()}");

            List<WorldBriefResponse> res = null;

            if (response.IsSuccessStatusCode) {
                string json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON received: {json}");
                res = JsonConvert.DeserializeObject<List<WorldBriefResponse>>(json);
            }

            return res;
        }

        public async Task<WorldMetadataResponse> GetMetadata(string id) {
            Console.WriteLine($"Getting world metadata with ID: {id}");
            HttpResponseMessage response = await Global.HttpClient.GetAsync($"worlds/{id}/metadata?apiKey={Global.ApiKey}");

            WorldMetadataResponse res = null;

            if (response.IsSuccessStatusCode) {
                string json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON received: {json}");
                res = JsonConvert.DeserializeObject<WorldMetadataResponse>(json);
            }

            return res;
        }

        public async Task<WorldInstanceResponse> GetInstance(string worldId, string instanceId) {
            Console.WriteLine($"Getting world instance with world ID: {worldId} and instance ID {instanceId}");
            HttpResponseMessage response = await Global.HttpClient.GetAsync($"worlds/{worldId}/{instanceId}?apiKey={Global.ApiKey}");

            WorldInstanceResponse res = null;

            if (response.IsSuccessStatusCode) {
                string text = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON received: {text}");
                var json = JObject.Parse(text);

                res = new WorldInstanceResponse {
                    id = json["id"].ToString(),
                    name = json["name"].ToString(),
                    privateUsers = (json["private"] is JArray)
                        ? json["private"].Select(tk => tk.ToObject<WorldInstanceUserResponse>()).ToList() : null,
                    friends = (json["friends"] is JArray)
                        ? json["friends"].Select(tk => tk.ToObject<WorldInstanceUserResponse>()).ToList() : null,
                    users = (json["users"] is JArray)
                        ? json["users"].Select(tk => tk.ToObject<WorldInstanceUserResponse>()).ToList() : null,
                    hidden = (json["hidden"] == null || json["hidden"].Type == JTokenType.Null) ? null : json["hidden"].ToString(),
                    nonce = (json["nonce"] == null) ? null : json["nonce"].ToString(),
                };
            }

            return res;
        }
    }
}
