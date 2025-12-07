using ChatEngine.Lang;
using ChatEngine.Storage;
using NuggetMod.Wrapper.Engine;

namespace ChatEngine.Commands;

internal class CommandKick(string n, string d, List<ArgumentsDescriptor>? arguments) : BaseMetaModCommand(n, d, arguments, Enum.Admin.Admin)
{
    protected override bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget, Edict? player = null)
    {
        if (arguments.Count < 1)
            return false;
        string steamid = arguments[0];
        string reason = string.Empty;
        if (arguments.Count > 1)
            reason = arguments[1];
        PlayerInfo.Kick(steamid, (string.IsNullOrEmpty(reason) ? Language.GetTranlation("player.kicked") : reason));
        return true;
    }
}
