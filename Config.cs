using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuBan
{
    public class Config : IRocketPluginConfiguration
    {
        public bool UseIPHUB;
        public string IPHUBAPIKey;
        public string DatabaseHost;
        public string DatabaseUser;
        public string DatabasePassword;
        public string DatabaseName;
        public ushort DatabasePort;
        public string DatabaseTableBans;
        public string DatabaseTableIPBans;
        public string DefaultBanReason;
        public void LoadDefaults()
        {
            UseIPHUB = false;
            IPHUBAPIKey = "MyKey";
            DatabaseHost = "localhost";
            DatabaseUser = "admin";
            DatabasePassword = "admin";
            DatabasePort = 3306;
            DatabaseName = "database";
            DatabaseTableBans = "Bans";
            DatabaseTableIPBans = "IpBans";
            DefaultBanReason = "Violating the rules!";
        }
    }
}
