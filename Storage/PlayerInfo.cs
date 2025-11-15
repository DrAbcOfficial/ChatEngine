using Metamod.Interface;
using Metamod.Wrapper.Engine;

namespace ChatEngine.Storage
{
    internal class PlayerInfo
    {
        internal bool Admin;

        internal bool Banned;
        internal DateTime BannedUntil;

        internal bool Garged;
        internal DateTime GargedUntil;

        internal string Language = string.Empty;

        internal static Dictionary<string, PlayerInfo> PlayerStorage = [];

        internal static string GetPlayerSteamID(Edict player)
        {
            string steamid = MetaMod.EngineFuncs.GetPhysicsKeyValue(player, "*sid");
            if (string.IsNullOrWhiteSpace(steamid))
                steamid = MetaMod.EngineFuncs.GetPlayerAuthId(player);
            return steamid;
        }
        internal static void PlayerConnected(Edict player)
        {
            string steamid = GetPlayerSteamID(player);
            PlayerStorage.Add(steamid, new());
        }
        internal static void PlayerDisconnected(Edict player)
        {
            string playerid = GetPlayerSteamID(player);
            PlayerStorage.Remove(playerid);
        }

        internal static PlayerInfo? GetPlayerInfo(Edict player)
        {
            string steamid = GetPlayerSteamID(player);
            if (PlayerStorage.TryGetValue(steamid, out PlayerInfo? info))
                return info;
            else
                return null;
        }
    }
}
