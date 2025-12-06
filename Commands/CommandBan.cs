using ChatEngine.Lang;
using ChatEngine.Storage;
using NuggetMod.Interface;
using NuggetMod.Wrapper.Engine;

namespace ChatEngine.Commands;

internal class CommandBan(string n, string d, List<ArgumentsDescriptor>? arguments) : BaseMetaModCommand(n, d, arguments, true)
{
    protected override bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget, Edict? player = null)
    {
        if (arguments.Count < 2)
            return false;
        string steamid = arguments[0];
        int bantime = arguments[1];
        string reason =string.Empty;
        if(arguments.Count > 2)
            reason = arguments[2];
        if(bantime < 0)
            bantime = int.MaxValue;
        var info = Plugin.SQLStorage.GetPlayerInfo(steamid);
        if (info == null)
            return false;
        MetaMod.EngineFuncs.ServerCommand($"kick #{steamid} \"{(string.IsNullOrEmpty(reason) ?
            string.Format(Language.GetTranlation("player.banned"), bantime) : reason)}\"\n");
        MetaMod.EngineFuncs.ServerExecute();
        DateTime banuntil = DateTime.UtcNow.AddMinutes(bantime);
        info.BannedUntil = banuntil;
        string operatorId = "Server";
        if (player != null)
            operatorId = PlayerInfo.GetPlayerSteamID(player);
        Task.Factory.StartNew(() =>
        {
            Plugin.SQLStorage.UpsertPlayerInfo(info);
            Plugin.SQLStorage.LogBan(steamid, operatorId, banuntil);
        });
        return true;
    }
}
