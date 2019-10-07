using static StolenAvatarChecker.Utils.ReflectionUtils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VRChatApi;
using VRChatApi.Classes;
using System.Reflection;
using System.Threading;
using System.Net;
using System.Net.Security;
using Newtonsoft.Json;

namespace StolenAvatarChecker {
    public partial class Form1 : Form {

        private VRChatApi.VRChatApi api;
        private bool IsLoggedIn = false;

        public Form1() {
            InitializeComponent();
            this.Text = "Stolen Avatar Checker";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            apiKeyTextField.Text = Global.ApiKey;

            loggingOnButton.Checked = true;

            logTextField.VisibleChanged += (sender, e) =>
            {
                if (logTextField.Visible) {
                    logTextField.SelectionStart = logTextField.TextLength;
                    logTextField.ScrollToCaret();
                    logTextField.Refresh();
                }
            };

            Global.ApiKey = System.IO.File.ReadAllText("APIKey.cfg");
            apiKeyTextField.Text = Global.ApiKey;
        }

        //Open Log Button
        private void button3_Click(object sender, EventArgs e) {
            System.IO.File.WriteAllText("Log.txt", logTextField.Text);
            //System.Diagnostics.Process.Start("notepad.exe", "Log.txt");
        }

        //Clear Log Button
        private void button2_Click(object sender, EventArgs e) {
            logTextField.Text = "";
        }

        //Login Button
        private void button1_Click(object sender, EventArgs e) {
            StartApi();
        }

        async Task StartApi() {
            if (IsLoggedIn) {
                return;
            }

            string n = Environment.NewLine;
            string userID = "";
            try
            {
                Global.ApiKey = apiKeyTextField.Text;
                try { System.IO.File.WriteAllLines("APIKey.cfg", new string[] { Global.ApiKey }); } catch (Exception e) { /*unhandled*/ }
                logTextField.Text += "Logging in to API as " + userTextField.Text + n;
                //logTextField.Text += "Creating API Instance" + n;
                api = new VRChatApi.VRChatApi(userTextField.Text, passwordTextField.Text);
                //logTextField.Text += "Getting config response" + n;
                ConfigResponse config = await api.RemoteConfig.Get();

                UserResponse user = await api.UserApi.Login();

                if (user == null)
                {
                    logTextField.Text += "Login Failed (Incorrect Username/Password OR APIKey)" + n;
                    IsLoggedIn = false;
                    return;
                }

                loginButton.Enabled = false;

                //logTextField.Text += user.DeepDumpAsString() + n;
                IsLoggedIn = true;

                userID = user.id;
                if (!user.friends.Contains(userID))
                {
                    await api.UserApi.SendFriendRequest(userID);
                    await api.FriendsApi.AcceptFriend(userID);
                }
            } catch (Exception e)
            {
                logTextField.Text += e.ToString() + Environment.NewLine;
            }

            new Thread(async () =>
            {
                while (true) 
                {
                    if (!IsLoggedIn)
                        break;

                    if (!loggingOnButton.Checked) {
                        Thread.Sleep(1000);
                        continue;
                    }

                    //usr_80a984c9-2b8e-4396-b893-2f31d8fd2ba3
                    UserBriefResponse userPing = await api.UserApi.GetById(userID);
                    //ThreadHelperClass.SetText(this, logTextField, logTextField.Text + "User Ping" + n + n + userPing.DeepDumpAsString() + n);

                    ThreadHelperClass.SetText(this, logTextField, logTextField.Text + "Running check for stolen avatars" + n);

                    IList<String> ids = await api.WorldApi.GetUsersInInstance(userPing.worldId + "/" + userPing.instanceId);
                    foreach (String s in ids) {
                        //ThreadHelperClass.SetText(this, logTextField, logTextField.Text + s + n);

                        UserBriefResponse userInRoom = await api.UserApi.GetById(s);
                        if (userInRoom == null)
                            continue;
                        IList<string> userPublicAvatars = await api.AvatarApi.GetAvatarList(userInRoom.id);

                        foreach (String ss in userPublicAvatars) {
                            //ThreadHelperClass.SetText(this, logTextField, logTextField.Text + "              " + ss + n);
                            try {
                                AvatarResponse avatar = await api.AvatarApi.GetById(ss);
                                CheckIfStolen(avatar.assetUrl, avatar.id, userInRoom.username, userInRoom.displayName, userInRoom.id);
                            } catch (Exception e) {
                                string response = await api.AvatarApi.GetUrl(ss);
                                string[] parse = response.Split('"');
                                string assetURL = "";
                                string avatarID = "";
                                for (int i = 0; i < parse.Length; i++) {
                                    if (parse[i].Contains("assetUrl")) {
                                        assetURL = parse[i + 2];
                                        break;
                                    }
                                }
                                foreach (String sss in parse) {
                                    if (sss.Contains("avtr_")) {
                                        avatarID = sss;
                                        break;
                                    }
                                }
                                CheckIfStolen(assetURL, avatarID, userInRoom.username, userInRoom.displayName, userInRoom.id);
                                //ThreadHelperClass.SetText(this, logTextField, logTextField.Text + "              " + "Manual Asset URL: " + assetURL + n);
                            }
                        }
                    }

                    ThreadHelperClass.SetText(this, logTextField, logTextField.Text + "Check finished. Waiting 60 seconds..." + n);
                    Thread.Sleep(60000);
                }
            }).Start();
        }

        async void CheckIfStolen(string assetUrl, String avatarID, string username, string displayName, string userID)
        {
            string fileName = string.Empty, author = string.Empty;
            using (WebClient web = new WebClient())
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                    string url = assetUrl.Substring(36).Split('/')[0];
                    string resp = web.DownloadString($"https://api.vrchat.cloud/api/1/file/" + url + "/");
                    var obj = JsonConvert.DeserializeObject<Dictionary<object, object>>(resp);
                    fileName = obj["name"].ToString();
                    author = obj["ownerId"].ToString();
                }
                catch (Exception e) { }
            }

            if (fileName == string.Empty || fileName.ToLower().Contains("asset"))
                return;
            string n = Environment.NewLine;
            string toPrint = "=========================Stolen Avatar Detected===================" + n;
            toPrint += "Time: " + DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + n;
            toPrint += "Username: " + username + n;
            toPrint += "Displayname: " + displayName + n;
            toPrint += "User ID: " + userID + n;
            toPrint += "Stolen Avatar ID: " + avatarID + n;
            toPrint += "===============================================================" + n + n;

            ThreadHelperClass.SetText(this, logTextField, logTextField.Text + toPrint + n);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            CheckAvatarIDIfStolen();
        }

        async Task CheckAvatarIDIfStolen()
        {
            string n = Environment.NewLine;
            string id = avatarIDTextField.Text;
            logTextField.Text += "Checking " + id + " to see if it is a stolen avatar";

            try
            {
                AvatarResponse avatar = await api.AvatarApi.GetById(id);
                UserBriefResponse avatarOwner = await api.UserApi.GetById(avatar.authorId);
                CheckIfStolen(avatar.assetUrl, avatar.id, avatarOwner.username, avatarOwner.displayName, avatarOwner.id);
            }
            catch (Exception e)
            {
                string response = await api.AvatarApi.GetUrl(id);
                string[] parse = response.Split('"');
                string assetURL = "";
                string avatarID = "";
                for (int i = 0; i < parse.Length; i++)
                {
                    if (parse[i].Contains("assetUrl"))
                    {
                        assetURL = parse[i + 2];
                        break;
                    }
                }
                foreach (String sss in parse)
                {
                    if (sss.Contains("avtr_"))
                    {
                        avatarID = sss;
                        break;
                    }
                }
                CheckIfStolen(assetURL, avatarID, "", "", "");
                //ThreadHelperClass.SetText(this, logTextField, logTextField.Text + "              " + "Manual Asset URL: " + assetURL + n);
            }
        }
    }

    static class ThreadHelperClass {
        delegate void SetTextCallback(Form f, TextBox ctrl, string text);
        /// <summary>
        /// Set text property of various controls
        /// </summary>
        /// <param name="form">The calling form</param>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        public static void SetText(Form form, TextBox ctrl, string text) {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (ctrl.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(SetText);
                form.Invoke(d, new object[] { form, ctrl, text });
            } else {
                ctrl.Text = text;
                ctrl.SelectionStart = ctrl.TextLength;
                ctrl.ScrollToCaret();
            }
        }
    }
}
