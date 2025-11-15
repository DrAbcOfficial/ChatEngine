using ChatEngine.Lang;

namespace ChatEngine.Commands;

internal class CommandHelp(string n, string d) : BaseMetaModCommand(n, d, null)
{
    private static string ProcessField(string input, int targetLength)
    {
        if (string.IsNullOrEmpty(input))
            return new string(' ', targetLength);
        string truncated = input.Length > targetLength
            ? input.Substring(0, targetLength)
            : input;
        return truncated.PadRight(targetLength);
    }

    protected override bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget)
    {
        foreach (var cmds in Commands)
        {
            string namePart = ProcessField(cmds.Value.Name, 12);
            string desc1Part = ProcessField(cmds.Value.Description, 24);

            string desc2Part = string.Empty;
            foreach (var arg in cmds.Value.Arguments)
            {
                desc2Part += $"<{arg.Name}:{arg.Type}>";
            }
            desc2Part = ProcessField(desc2Part, 32);
            string desc3Part = ProcessField(cmds.Value.Admin ? "Yes" : "No", 3);
            Language.Print($"{namePart}{desc1Part}{desc2Part}{desc3Part}", printTarget);
        }
        return true;
    }
}
