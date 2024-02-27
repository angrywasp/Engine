using Engine;
using Engine.UI.Controls;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Engine.Configuration;
using Engine.Multiplayer;

namespace GameDemo.UI.Online
{
    public class OnlineMenu
    {
        protected Button btnBack;

        protected ListBox lbxServers;
        protected Button btnConnect;
        protected Button btnHostGame;

        public string FormFile => "Engine/UI/Default.form";

        //public OnlineMenu(EngineCore engine) : base(engine) { }

        /*protected override void Build()
        {
            Vector2 buttonSize = new Vector2(0.2f, 0.05f);
            
            UiControl ctrlBg = AddControl(Vector2.Zero, Vector2.One, "Background");
            ctrlBg.ApplySkinElement("menuBackground");

            TextBox txtHeading = AddTextBox(Vector2.Zero, new Vector2(0.5f, 0.15f), "Heading", ctrlBg);
            txtHeading.Text = "Online";
            txtHeading.ApplySkinElement("headingText96");

            btnConnect = AddButton(new Vector2(0.05f, 0.15f), buttonSize, "Connect", ctrlBg);
            btnHostGame = AddButton(new Vector2(0.05f, 0.25f), buttonSize, "Host", ctrlBg);

            btnBack = AddButton(new Vector2(0.05f, 0.9f), buttonSize, "Back", ctrlBg);

            lbxServers = AddListBox(new Vector2(0.35f, 0.15f), new Vector2(0.55f, 0.25f), "Servers", ctrlBg);
            
            Dictionary<string, object> lbxData = new Dictionary<string, object>();

            foreach (var s in Engine.Configuration.Settings.Engine.SavedServers)
                lbxData.Add(FormatServerName(s.Name, lbxData), s);

            lbxServers.DataBind(lbxData);

            lbxServers.SelectedIndex = 0;

            btnConnect.MouseClick += btnConnect_MouseClick;
            btnHostGame.MouseClick += btnHostGame_MouseClick;
            btnBack.MouseClick += btnBack_MouseClick;
        }*/

        public void Update(GameTime gameTime)
        {
            /*if (engine.NetworkClient != null && engine.NetworkClient.ThisUser != null)
            {
                string status = engine.NetworkClient.NetworkObject.ConnectionStatus.ToString();

                if (btnConnect.TextField.Text != status)
                    btnConnect.TextField.Text = status;
            }*/
        }

        private string FormatServerName(string n, Dictionary<string, object> ld)
        {
            return null;
            /*if (!ld.ContainsKey(n))
                return n;

            int i = 0;
            string s = n + i.ToString();

            while (ld.ContainsKey(s))
            {
                ++i;
                s = n + i.ToString();
            }

            return s;*/
        }

        void btnConnect_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            /*if (engine.NetworkConnected)
                engine.NetworkClient.NetworkObject.Disconnect("Goodbye"); //todo: need a setting for the goodbye message
            else
            {
                if (lbxServers.SelectedIndex == -1)
                    return;

                NetworkServerConnectionSettingsItem x = (NetworkServerConnectionSettingsItem)lbxServers.Items[lbxServers.SelectedIndex].Tag;

                engine.CreateNetworkClient(Engine.Configuration.Settings.Engine.ServerHost.Port, x.PrivateKey);
                engine.NetworkClient.Connected += Network_UserConnected;
                engine.NetworkClient.NetworkObject.Connect(x.Address, x.Port);
            }*/
        }

        void btnHostGame_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            /*int port = Engine.Configuration.Settings.Engine.ServerHost.Port;
            string privateKey = Engine.Configuration.Settings.Engine.ServerHost.PrivateKey;
            int maxConnections = Engine.Configuration.Settings.Engine.ServerHost.MaxConnections;
            string map = Engine.Configuration.Settings.Engine.ServerHost.Map;

            engine.CreateNetworkServer(port, privateKey, maxConnections, map);

            engine.NetworkServer.Connected += NetworkServer_Connected;
            engine.NetworkServer.Disconnected += NetworkServer_Disconnected;
            BasicGame.Instance.GameHelper.LoadMap();*/
        }

        void btnBack_MouseClick(UiControl sender, UiControlMouseEventArgs e)
        {
            //BasicGame.Instance.LoadMainMenu();
        }

        void Network_UserConnected(ClientConnectedPeer sender)
        {
            //message received when any client is connected
            //even you
            /*if (sender.Id == engine.NetworkClient.ThisUser.Id)
                engine.Interface.ScreenMessages.Write("You are connected", Color.LimeGreen);
            else
                engine.Interface.ScreenMessages.Write($"{sender.UserName} connected", Color.LimeGreen);

            BasicGame.Instance.GameHelper.LoadMap();*/
        }

        void NetworkServer_Connected(ServerConnectedPeer sender)
        {
            //engine.Interface.ScreenMessages.Write($"{sender.UserName} connected", Color.LimeGreen);
        }

        void NetworkServer_Disconnected(ServerConnectedPeer sender)
        {
            //engine.Interface.ScreenMessages.Write($"{sender.UserName} disconnected", Color.LimeGreen);
        }
    }
}