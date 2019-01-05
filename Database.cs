using MySql.Data.MySqlClient;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuBan
{
    public class Database
    {
        public Database()
        {
            Logger.Log("Loading MySQL for QuBan...", ConsoleColor.Yellow);
            SetupConnection();
            CreateTablesIfNotExists();
            Logger.Log("MySQL is loaded!", ConsoleColor.Green);
        }

        private MySqlConnection Connection { get; set; }
        #region Database data
        private string Host => QuBan.Instance.Configuration.Instance.DatabaseHost;
        private string User => QuBan.Instance.Configuration.Instance.DatabaseUser;
        private string Password => QuBan.Instance.Configuration.Instance.DatabasePassword;
        private string BaseName => QuBan.Instance.Configuration.Instance.DatabaseName;
        private ushort Port => QuBan.Instance.Configuration.Instance.DatabasePort;
        private string TableBans => QuBan.Instance.Configuration.Instance.DatabaseTableBans;
        private string TableIPBans => QuBan.Instance.Configuration.Instance.DatabaseTableIPBans;
        private string ConnectionString => string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", Host, BaseName, User, Password, Port);
        #endregion Database data

        private void SetupConnection()
        {
            Connection = new MySqlConnection(ConnectionString);
        }

        private void OpenConnection()
        {
            try { Connection.Open(); }
            catch (Exception ex) { Logger.LogError("Error while opening connection: " + ex.Message); }
        }

        private void CloseConnection()
        {
            try { Connection.Close(); }
            catch (Exception ex) { Logger.LogError("Error while closing connection: " + ex.Message); }
        }

        private void ExecuteCommand(string command)
        {
            OpenConnection();
            new MySqlCommand(command, Connection).ExecuteNonQuery();
            CloseConnection();
        }

        private MySqlDataReader ExecuteCommandReader(string command)
        {
            OpenConnection();
            var reader = new MySqlCommand(command, Connection).ExecuteReader();
            CloseConnection();
            return reader;
        }

        private void CreateTablesIfNotExists()
        {
            ExecuteCommand($"CREATE TABLE IF NOT EXISTS {TableBans} (id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, ViolatorID VARCHAR(40), Violator VARCHAR(150), AdminID VARCHAR(40), Admin VARCHAR(150), Reason VARCHAR(200), Duration int, Cancelled VARCHAR(6), Time DATETIME DEFAULT NOW())");
            ExecuteCommand($"CREATE TABLE IF NOT EXISTS {TableIPBans} (id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, ViolatorID VARCHAR(40), ViolatorIP VARCHAR(16), Violator VARCHAR(150), AdminID VARCHAR(40), Admin VARCHAR(150), Reason VARCHAR(200), Duration int, Cancelled VARCHAR(6), Time DATETIME DEFAULT NOW())");
        }

        public bool Still(DateTime ta, int dura)
        {
            return (DateTime.Now - ta).TotalSeconds < dura;
        }

        public bool IsBanned(UnturnedPlayer player, out string reason)
        {
            bool res = false;
            reason = "";
            var reader = ExecuteCommandReader($"SELECT * FROM {TableBans} WHERE ViolatorID LIKE {player.CSteamID.m_SteamID}");
            while (reader.NextResult())
            {
                if (Still(DateTime.Parse(reader["Time"].ToString()), int.Parse(reader["Duration"].ToString())) && !bool.Parse(reader["Cancelled"].ToString()))
                {
                    res = true;
                    reason = reader["Reason"].ToString();
                }
            }
            return res;
        }

        public bool IsIpBanned(UnturnedPlayer player, out string reason)
        {
            bool res = false;
            reason = "";
            var reader = ExecuteCommandReader($"SELECT * FROM {TableBans} WHERE ViolatorID LIKE {player.CSteamID.m_SteamID} OR ViolatorIP LIKE {player.IP}");
            while (reader.NextResult())
            {
                if (Still(DateTime.Parse(reader["Time"].ToString()), int.Parse(reader["Duration"].ToString())) && !bool.Parse(reader["Cancelled"].ToString()))
                {
                    res = true;
                    reason = reader["Reason"].ToString();
                }
            }
            return res;
        }


        public void Ban(UnturnedPlayer player, IRocketPlayer caller, string reason, int duration = 0)
        {
            ExecuteCommand($"INSERT IGNORE INTO {TableBans} (ViolatorID, Violator, AdminID, Admin, Reason, Duration, Time) VALUES ({player.CSteamID.m_SteamID.ToString()}, {player.CharacterName}, {caller.Id}, {caller.DisplayName}, {reason}, {duration.ToString()}, NOW())");
        }

        public void IPBan(UnturnedPlayer player, IRocketPlayer caller, string reason, int duration = 0)
        {
            ExecuteCommand($"INSERT IGNORE INTO {TableIPBans} (ViolatorID, ViolatorIP, Violator, AdminID, Admin, Reason, Duration, Time) VALUES ({player.CSteamID.m_SteamID.ToString()}, {player.IP}, {player.CharacterName}, {caller.Id}, {caller.DisplayName}, {reason}, {duration.ToString()}, NOW())");
        }

        public void UNIPBan(string name)
        {
            ExecuteCommand($"UPDATE {TableIPBans} SET Cancelled='true' WHERE ViolatorID LIKE '{name}' OR ViolatorIP LIKE '{name}' OR Violator LIKE {name}");
        }

        public void UNBan(string nama)
        {
            ExecuteCommand($"UPDATE {TableBans} SET Cancelled='true' WHERE ViolatorID LIKE '{nama}' OR Violator LIKE {nama}");
        }
    }
}
