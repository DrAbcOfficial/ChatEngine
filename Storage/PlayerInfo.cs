using NuggetMod.Interface;
using NuggetMod.Wrapper.Engine;
using System.Collections.Concurrent;

namespace ChatEngine.Storage
{
    internal class PlayerInfo
    {
        internal long SteamID;
        internal string NickName = "Unknown";
        internal DateTime? BannedUntil;
        internal DateTime? GargedUntil;
        internal long TalkedCount;
        internal bool Admin;
        internal string[] Flags = [];

        internal static ConcurrentDictionary<long, PlayerInfo> PlayerStorage = [];

        internal static long GetPlayerSteamID(Edict player)
        {
            string steamid = MetaMod.EngineFuncs.GetPhysicsKeyValue(player, "*sid");
            if (string.IsNullOrWhiteSpace(steamid))
                return 0;
            if (long.TryParse(steamid, out long id))
                return id;
            return 0;
        }
        internal static (bool, PlayerInfo) PlayerConnected(Edict player, string name)
        {
            long steamid = GetPlayerSteamID(player);
            PlayerInfo info = Plugin.SQLStorage.GetOrCreatePlayerInfo(steamid, name);
            DateTime now = DateTime.UtcNow;
            if (info.BannedUntil.HasValue && info.BannedUntil.Value > now)
                return (false, info);
            PlayerStorage[steamid] = info;
            return (true, info);
        }
        internal static Task PlayerDisconnected(Edict player)
        {
            long steamid = GetPlayerSteamID(player);
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
