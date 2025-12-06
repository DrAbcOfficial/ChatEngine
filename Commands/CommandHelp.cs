using ChatEngine.Lang;
using NuggetMod.Wrapper.Engine;
using System.Text;

namespace ChatEngine.Commands;

internal class CommandHelp(string n, string d) : BaseMetaModCommand(n, d, null)
{
    private static int GetDisplayWidth(string text)
    {
        int width = 0;
        foreach (char c in text)
        {
            if (c >= 0x4E00 && c <= 0x9FFF ||
                c >= 0x3000 && c <= 0x303F ||
                c >= 0xFF00 && c <= 0xFFEF)
            {
                width += 2;
            }
            else
            {
                width += 1;
            }
        }
        return width;
    }
    private static string ProcessField(string input, int targetWidth)
    {
        if (string.IsNullOrEmpty(input))
            return new string(' ', targetWidth);

        int currentWidth = GetDisplayWidth(input);
        if (currentWidth > targetWidth)
        {
            StringBuilder sb = new();
            int width = 0;
            foreach (char c in input)
            {
                int charWidth = (c >= 0x4E00 && c <= 0x9FFF || 
                                c >= 0x3000 && c <= 0x303F || 
                                c >= 0xFF00 && c <= 0xFFEF) ? 2 : 1;
                if (width + charWidth > targetWidth)
                    break;
                sb.Append(c);
                width += charWidth;
            }
            input = sb.ToString();
            currentWidth = width;
        }
        int paddingNeeded = targetWidth - currentWidth;
        return input + new string(' ', paddingNeeded);
    }

    protected override bool Excute(List<MetaModArgument> arguments, Language.PrintTarget printTarget, Edict? player = null)
    {
        Language.PrintWithLang("command.help", printTarget, player);
        
        // 表头
        string header = ProcessField(Language.GetTranlation("command.help.command"), 16) +
                       ProcessField(Language.GetTranlation("command.help.description"), 24) +
                       ProcessField(Language.GetTranlation("command.help.arguments"), 40) +
                       ProcessField(Language.GetTranlation("command.help.admin"), 8);
        Language.Print(header, printTarget, player);
        
        // 分隔线
        Language.Print(new string('-', 88), printTarget, player);
        
        // 命令列表
        foreach (var cmds in Commands)
        {
            string namePart = ProcessField(cmds.Value.Name, 16);
            string descPart = ProcessField(Language.GetTranlation(cmds.Value.Description), 24);
            
            // 构建参数列表
            StringBuilder argBuilder = new();
            foreach (var arg in cmds.Value.Arguments)
            {
                if (arg.Optional)
                    argBuilder.Append($"[{arg.Name}] ");
                else
                    argBuilder.Append($"<{arg.Name}> ");
            }
            string argPart = ProcessField(argBuilder.ToString().TrimEnd(), 40);
            
            string adminPart = ProcessField(Language.GetTranlation(cmds.Value.Admin ? "yes" : "no"), 8);
            
            Language.Print($"{namePart}{descPart}{argPart}{adminPart}", printTarget, player);
        }
        
        return true;
    }
}
