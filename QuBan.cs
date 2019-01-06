using Newtonsoft.Json.Linq;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Commands;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace QuBan
{
    public class QuBan : RocketPlugin<Config>
    {
        public static QuBan Instance { get; set; }
        public Database Database { get; private set; }

        protected override void Load()
        {
            Instance = this;
            Database = new Database();
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            string IP = player.IP;
            if (Configuration.Instance.UseIPHUB)
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("X-Key: " + Configuration.Instance.IPHUBAPIKey);
                    try
                    {
                        var response = webClient.DownloadString("http://v2.api.iphub.info/ip/" + IP);
                        JObject parsed = JObject.Parse(response);
                        int Block = parsed.GetValue("block").Value<int>();
                        switch (Block)
                        {
                            case 0:
                                Logger.Log(player.CharacterName + " has safe IP.", ConsoleColor.Green);
                                break;
                            case 1:
                            case 2:
                                Logger.Log(player.CharacterName + " has Proxy/VPN. Terminating connection...", ConsoleColor.Red);
                                player.Kick("Using VPN.");
                                return;
                        }
                    }
                    catch (WebException ex)
                    {
                        Logger.LogError("Error while using IPHUB: " + ex.Message);
                    }
                }
            }
            if (Database.IsBanned(player, out string reason))
            {
                Logger.Log(player.CharacterName + " is banned. Terminating connection...");
                player.Kick("General ban: " + reason);
            }
            else if (Database.IsIpBanned(player, out string reasona))
            {
                Logger.Log(player.CharacterName + " is IP banned. Terminating connection...");
                player.Kick("General IPban: " + reasona);
            }
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            {"muted", "You are muted!" },
            {"invalid", "Invalid arguments!" },
            {"success", "Successfully!" }
        };










        #region Commands

        [RocketCommand("ban", "")]
        public void Ban(IRocketPlayer caller, string[] args)
        {
            switch (args.Length)
            {
                case 1:
                    var p = UnturnedPlayer.FromName(args[0]);
                    if (p != null)
                    {
                        Database.Ban(p, caller, Configuration.Instance.DefaultBanReason);
                        caller.SendMessage("success");
                        p.Kick(Configuration.Instance.DefaultBanReason);
                    }
                    else
                        caller.SendMessage("invalid");
                    break;
                case 2:
                    var pa = UnturnedPlayer.FromName(args[0]);
                    if (pa != null)
                    {
                        Database.Ban(pa, caller, args[1]);
                        pa.Kick(args[1]);
                        caller.SendMessage("success");
                    }
                    else
                        caller.SendMessage("invalid");
                    break;
                case 3:
                    var paa = UnturnedPlayer.FromName(args[0]);
                    if (int.TryParse(args[2], out int dura))
                    {
                        if (paa != null)
                        {
                            Database.Ban(paa, caller, args[1], dura);
                            paa.Kick(args[1]);
                            caller.SendMessage("success");
                        }
                        else
                            caller.SendMessage("invalid");
                    }
                    else
                        caller.SendMessage("invalid");
                    break;

                default:
                    caller.SendMessage("invalid");
                    break;
            }
        }



        [RocketCommand("ipban", "")]
        public void ipBan(IRocketPlayer caller, string[] args)
        {
            switch (args.Length)
            {
                case 1:
                    var p = UnturnedPlayer.FromName(args[0]);
                    if (p != null)
                    {
                        Database.IPBan(p, caller, Configuration.Instance.DefaultBanReason);
                        p.Kick(Configuration.Instance.DefaultBanReason);
                        caller.SendMessage("success");
                    }
                    else
                        caller.SendMessage("invalid");
                    break;
                case 2:
                    var pa = UnturnedPlayer.FromName(args[0]);
                    if (pa != null)
                    {
                        Database.IPBan(pa, caller, args[1]);
                        pa.Kick(args[1]);
                        caller.SendMessage("success");
                    }
                    else
                        caller.SendMessage("invalid");
                    break;
                case 3:
                    var paa = UnturnedPlayer.FromName(args[0]);
                    if (int.TryParse(args[2], out int dura))
                    {
                        if (paa != null)
                        {
                            Database.IPBan(paa, caller, args[1], dura);
                            paa.Kick(args[1]);
                            caller.SendMessage("success");
                        }
                        else
                            caller.SendMessage("invalid");
                    }
                    else
                        caller.SendMessage("invalid");
                    break;

                default:
                    caller.SendMessage("invalid");
                    break;
            }
        }

        [RocketCommand("unban", "")]
        public void unBan(IRocketPlayer caller, string[] args)
        {
            switch (args.Length)
            {
                case 1:
                    Database.UNBan(args[0]);
                    caller.SendMessage("success");
                    break;
                default:
                    caller.SendMessage("invalid");
                    break;
            }
        }

        [RocketCommand("unipban", "")]
        public void unipBan(IRocketPlayer caller, string[] args)
        {
            switch (args.Length)
            {
                case 1:
                    Database.UNIPBan(args[0]);
                    caller.SendMessage("success");
                    break;
                default:
                    caller.SendMessage("invalid");
                    break;
            }
        }


        #endregion Commands
    }
}
