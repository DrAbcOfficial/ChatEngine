using ChatEngine.Lang;
using ChatEngine.Storage;
using NuggetMod.Interface;
using NuggetMod.Wrapper.Engine;

namespace ChatEngine.Commands;

internal class CommandGag(string n, string d, List<ArgumentsDescriptor>? arguments) : BaseMetaModCommand(n, d, arguments, true)
{
    protected override bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget, Edict? player = null)
    {
        if (arguments.Count < 2)
            return false;
        string steamid = arguments[0];
        int gagtime = arguments[1];
        string reason = string.Empty;
        if (arguments.Count > 2)
            reason = arguments[2];
        if (gagtime < 0)
            gagtime = int.MaxValue;
        var info = PlayerInfo.PlayerStorage.TryGetValue(steamid, out var cachedInfo)
            ? cachedInfo
            : Plugin.SQLStorage.GetPlayerInfo(steamid);
        if (info == null)
            return false;
        
        DateTime gaguntil = DateTime.UtcNow.AddMinutes(gagtime);
        info.GaggedUntil = gaguntil;
        string operatorId = "Server";
        if (player != null)
            operatorId = PlayerInfo.GetPlayerSteamID(player);
        Task.Factory.StartNew(() =>
        {
            Plugin.SQLStorage.UpsertPlayerInfo(info);
            Plugin.SQLStorage.LogGag(steamid, operatorId, gaguntil);
        });
        
        return true;
    }
}
