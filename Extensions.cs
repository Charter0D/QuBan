using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuBan
{
    public static class Extensions
    {
        public static void SendMessage(this IRocketPlayer player, string key, params object[] para)
        {
            UnturnedChat.Say(player, QuBan.Instance.Translate(key, para));
        }
        
    }
}
