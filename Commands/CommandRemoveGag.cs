using ChatEngine.Lang;
using ChatEngine.Storage;
using NuggetMod.Wrapper.Engine;

namespace ChatEngine.Commands;

internal class CommandRemoveGag(string n, string d, List<ArgumentsDescriptor>? arguments) : BaseMetaModCommand(n, d, arguments, true)
{
    protected override bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget, Edict? player = null)
    {
        if (arguments.Count <= 0)
            return false;
        string steamid = arguments[0];
        var info = PlayerInfo.PlayerStorage.TryGetValue(steamid, out var cachedInfo)
            ? cachedInfo
            : Plugin.SQLStorage.GetPlayerInfo(steamid);
        
        if (info == null)
            return false;
        info.GaggedUntil = null;
        Task.Factory.StartNew(() =>
        {
            Plugin.SQLStorage.UpsertPlayerInfo(info);
        });
        return true;
    }
}
