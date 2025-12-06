using ChatEngine.Lang;
using NuggetMod.Wrapper.Engine;

namespace ChatEngine.Commands;

internal class CommandHelp(string n, string d) : BaseMetaModCommand(n, d, null)
{
    private static string ProcessField(string input, int targetLength)
    {
        if (string.IsNullOrEmpty(input))
            return new string(' ', targetLength);
        string truncated = input.Length > targetLength ? input[..targetLength] : input;
        return truncated.PadRight(targetLength);
    }

    protected override bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget, Edict? player = null)
    {
        Language.Print("command.help", printTarget, player);
        string a = ProcessField(Language.GetTranlation("command.help.command"), 12);
        string b = ProcessField(Language.GetTranlation("command.help.description"), 18);
        string c = ProcessField(Language.GetTranlation("command.help.arguments"), 24);
        string d = ProcessField(Language.GetTranlation("command.help.admin"), 3);
        Language.Print($"{a}{b}{c}{d}", printTarget, player);
        foreach (var cmds in Commands)
        {
            string namePart = ProcessField(cmds.Value.Name, 12);
            string desc1Part = ProcessField(cmds.Value.Description, 18);
            string desc2Part = string.Empty;
            foreach (var arg in cmds.Value.Arguments)
            {
                desc2Part += $"<{arg.Name}:{arg.Type}>";
            }
            desc2Part = ProcessField(desc2Part, 24);
            string desc3Part = ProcessField(Language.GetTranlation(cmds.Value.Admin ? "yes" : "no"), 3);
            Language.Print($"{namePart}{desc1Part}{desc2Part}{desc3Part}", printTarget, player);
        }
        return true;
    }
}
