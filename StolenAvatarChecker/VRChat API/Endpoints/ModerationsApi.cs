using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VRChatApi.Classes;

namespace VRChatApi.Endpoints {
    public class ModerationsApi {

        public async Task<List<PlayerModeratedResponse>> GetPlayerModerations() {
            Console.WriteLine("Get list of moderations made by current user");
            HttpResponseMessage response = await Global.HttpClient.GetAsync("auth/user/playermoderations");

            List<PlayerModeratedResponse> res = null;

            if (response.IsSuccessStatusCode) {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON received: {json}");
                res = JsonConvert.DeserializeObject<List<PlayerModeratedResponse>>(json);
            }

            return res;
        }

        public async Task<List<PlayerModeratedResponse>> GetPlayerModerated() {
            Console.WriteLine("Get list of moderations made against current user");
            HttpResponseMessage response = await Global.HttpClient.GetAsync("auth/user/playermoderated");

            List<PlayerModeratedResponse> res = null;

            if (response.IsSuccessStatusCode) {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON received: {json}");
                res = JsonConvert.DeserializeObject<List<PlayerModeratedResponse>>(json);
            }

            return res;
        }
    }
}
