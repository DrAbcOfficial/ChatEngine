using ChatEngine.Lang;
using ChatEngine.Storage;
using NuggetMod.Wrapper.Engine;

namespace ChatEngine.Commands;

internal class CommandBan(string n, string d, List<ArgumentsDescriptor>? arguments) : BaseMetaModCommand(n, d, arguments, Enum.Admin.Admin)
{
    protected override bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget, Edict? player = null)
    {
        if (arguments.Count < 2)
            return false;
        string steamid = arguments[0];
        int bantime = arguments[1];
        string reason = string.Empty;
        if (arguments.Count > 2)
            reason = arguments[2];
        if (bantime < 0)
            bantime = int.MaxValue;

        var info = PlayerInfo.PlayerStorage.TryGetValue(steamid, out var cachedInfo)
            ? cachedInfo
            : Plugin.SQLStorage.GetPlayerInfo(steamid);

        if (info == null)
            return false;
        string operatorId = "Server";
        if (player != null)
            operatorId = PlayerInfo.GetPlayerSteamID(player);
        PlayerInfo.Ban(info, operatorId, bantime, (string.IsNullOrEmpty(reason) ?
            string.Format(Language.GetTranlation("player.banned"), bantime) : reason));
        return true;
    }
}
