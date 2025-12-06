using ChatEngine.Commands.Enum;
using ChatEngine.Lang;
using NuggetMod.Interface;
using NuggetMod.Wrapper.Engine;
using System.Collections.Concurrent;

namespace ChatEngine.Storage
{
    internal partial class PlayerInfo
    {
        internal string SteamID = "Invalid";
        internal string NickName = "Unknown";
        internal DateTime? BannedUntil;
        internal DateTime? GaggedUntil;
        internal long TalkedCount;
        internal Admin Admin;
        internal string IP = "";
        internal string[] Flags = [];

        internal static ConcurrentDictionary<string, PlayerInfo> PlayerStorage = [];

        internal static string GetPlayerSteamID(Edict player)
        {
            return MetaMod.EngineFuncs.GetPlayerAuthId(player);
        }
        internal static (bool, PlayerInfo) PlayerConnected(Edict player, string name, string ip)
        {
            string steamid = GetPlayerSteamID(player);
            PlayerInfo info = Plugin.SQLStorage.GetOrCreatePlayerInfo(steamid, name, ip);
            DateTime now = DateTime.UtcNow;
            if (info.BannedUntil.HasValue && info.BannedUntil.Value > now)
                return (false, info);
            if (info.BannedUntil.HasValue)
                info.BannedUntil = null;
            if (info.GaggedUntil.HasValue && info.GaggedUntil.Value <= now)
                info.GaggedUntil = null;
            PlayerStorage[steamid] = info;
            return (true, info);
        }
        internal static Task PlayerDisconnected(Edict player)
        {
            string steamid = GetPlayerSteamID(player);
            var info = GetPlayerInfo(player);
            if (info != null)
            {
                if (ConfigManager.Instance.Config.ShowPlayerInfoOnDisconnect > 0)
                {
                    string str = Language.GetTranlation("player.leaved");
                    str = string.Format(str, player.EntVars.NetName,
                        ConfigManager.Instance.Config.ShowPlayerInfoOnDisconnect > 1 ? info.IP : "***",
                        steamid);
                    Language.PrintAll(str, Language.PrintTarget.ClientConsole);
                }
            }
            return Task.Factory.StartNew(() =>
            {
                if (PlayerStorage.TryRemove(steamid, out var info))
                {
                    Plugin.SQLStorage.UpsertPlayerInfo(info);
                }
            });
        }

        internal static PlayerInfo? GetPlayerInfo(Edict player)
        {
            var steamid = GetPlayerSteamID(player);
            return PlayerStorage.TryGetValue(steamid, out var info) ? info : null;
        }

        internal static void InsterNewChatLog(Edict player, string msg, bool say_team)
        {
            PlayerInfo? info = GetPlayerInfo(player);
            if (info != null)
            {
                Interlocked.Increment(ref info.TalkedCount);
                Task.Factory.StartNew(() =>
                {
                    Plugin.SQLStorage.LogChat(info.SteamID, msg, say_team);
                    Plugin.SQLStorage.UpsertPlayerInfo(info);
                });
            }
        }
    }
}
