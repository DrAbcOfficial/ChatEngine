using NuggetMod.Interface;
using NuggetMod.Wrapper.Engine;

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

        internal static Dictionary<long, PlayerInfo> PlayerStorage = [];

        internal static long GetPlayerSteamID(Edict player)
        {
            string steamid = MetaMod.EngineFuncs.GetPhysicsKeyValue(player, "*sid");
            if (string.IsNullOrWhiteSpace(steamid))
                return 0;
            if(long.TryParse(steamid, out long id))
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
            PlayerStorage.Add(steamid, info);
            return (true, info);
        }
        internal static void PlayerDisconnected(Edict player)
        {
            long steamid = GetPlayerSteamID(player);
            if(PlayerStorage.TryGetValue(steamid, out PlayerInfo? info))
            {
                Plugin.SQLStorage.UpsertPlayerInfo(info);
                PlayerStorage.Remove(steamid);
            }
        }

        internal static PlayerInfo? GetPlayerInfo(Edict player)
        {
            var steamid = GetPlayerSteamID(player);
            if (PlayerStorage.TryGetValue(steamid, out PlayerInfo? info))
                return info;
            else
                return null;
        }
    }
}
