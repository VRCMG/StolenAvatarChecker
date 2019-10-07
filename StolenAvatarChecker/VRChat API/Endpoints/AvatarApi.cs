using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VRChatApi.Classes;

namespace VRChatApi.Endpoints {
    public class AvatarApi {

        public async Task<AvatarResponse> GetById(string id) {
            Console.WriteLine($"Getting avatar details using ID: {id}");
            HttpResponseMessage response = await Global.HttpClient.GetAsync($"avatars/{id}?apiKey={Global.ApiKey}");

            AvatarResponse res = null;

            if (response.IsSuccessStatusCode) {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON received: {json}");

                res = JsonConvert.DeserializeObject<AvatarResponse>(json);
            }

            return res;
        }

        public async Task<string> GetUrl(string id) {
            Console.WriteLine($"Getting avatar details using ID: {id}");
            HttpResponseMessage response = await Global.HttpClient.GetAsync($"avatars/{id}?apiKey={Global.ApiKey}");

            string res = null;

            if (response.IsSuccessStatusCode) {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON received: {json}");

                res = await response.Content.ReadAsStringAsync();
            }

            return res;
        }

        public async Task<IList<string>> GetAvatarList(string id) {
            IList<string> ids = new List<String>();
            HttpResponseMessage response = await Global.HttpClient.GetAsync($"avatars?userId={id}");
            string res = await response.Content.ReadAsStringAsync();
            string[] resSplit = res.Split('"');

            foreach (String s in resSplit) {
                if (s.StartsWith("avtr")) {
                    ids.Add(s);
                }
            }

            return ids;
        }
    }
}
