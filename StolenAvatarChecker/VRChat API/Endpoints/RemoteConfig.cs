using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VRChatApi.Classes;

namespace VRChatApi.Endpoints {
    public class RemoteConfig {

        public async Task<ConfigResponse> Get() {
            Console.WriteLine("Getting remote config");
            HttpResponseMessage response = await Global.HttpClient.GetAsync("config");

            ConfigResponse res = null;

            if (response.IsSuccessStatusCode) {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON received: {json}");
                res = JsonConvert.DeserializeObject<ConfigResponse>(json);
                Global.ApiKey = res.clientApiKey;
                Console.WriteLine($"API key has been set to: {Global.ApiKey}");
            }

            return res;
        }
    }
}
