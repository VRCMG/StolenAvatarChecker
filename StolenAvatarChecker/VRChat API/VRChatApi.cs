using System;
using System.Net.Http;
using System.Text;
using VRChatApi.Endpoints;

namespace VRChatApi {
    public class VRChatApi {

        public RemoteConfig RemoteConfig { get; set; }
        public UserApi UserApi { get; set; }
        public FriendsApi FriendsApi { get; set; }
        public WorldApi WorldApi { get; set; }
        public ModerationsApi ModerationsApi { get; set; }
        public AvatarApi AvatarApi { get; set; }

        public VRChatApi(string username, string password) {
            Console.WriteLine($"Entering {nameof(VRChatApi)} constructor");
            Console.WriteLine($"Using username {username}");

            // initialize endpoint classes
            RemoteConfig = new RemoteConfig();
            UserApi = new UserApi(username, password);
            FriendsApi = new FriendsApi();
            WorldApi = new WorldApi();
            ModerationsApi = new ModerationsApi();
            AvatarApi = new AvatarApi();

            // initialize http client
            // TODO: use the auth cookie
            if (Global.HttpClient == null) {
                Console.WriteLine($"Instantiating {nameof(HttpClient)}");
                Global.HttpClient = new HttpClient();
                Global.HttpClient.BaseAddress = new Uri("https://api.vrchat.cloud/api/1/");
                Console.WriteLine($"VRChat API base address set to {Global.HttpClient.BaseAddress}");
            }

            string authEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserApi.Username}:{UserApi.Password}"));

            var header = Global.HttpClient.DefaultRequestHeaders;
            if (header.Contains("Authorization")) {
                Console.WriteLine("Removing existing Authorization header");
                header.Remove("Authorization");
            }
            header.Add("Authorization", $"Basic {authEncoded}");
            Console.WriteLine($"Added new Authorization header");
        }
    }
}
