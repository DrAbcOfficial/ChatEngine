using ChatEngine.Storage;
using NuggetMod.Interface;

namespace ChatEngine.Censor;

internal static class StringChecker
{
    private static DFANode? RootDFA;

    internal static void BuildDFA(string gamePath)
    {
        (RootDFA, uint count, uint terminals) = DFANode.CreateFromFile(Path.Combine(gamePath, ConfigManager.Instance.Config.Censor.CensorWordsFilePath));
        MetaMod.EngineFuncs.ServerPrint($"ChatEngine Loaded {count} DFA nodes, {terminals} DFA terminals");
    }
    internal static DFANode.CheckResult[] Check(string str)
    {
        if (RootDFA == null)
            throw new NullReferenceException("Root DFA is NULL");
        return RootDFA.CheckString(str);
    }
    internal static (bool, DFANode.CheckResult[]?) IsIlegalString(string str)
    {
        if (RootDFA == null)
            throw new NullReferenceException("Root DFA is NULL");
        var result = RootDFA.CheckString(str);
        if (result.Length > ConfigManager.Instance.Config.Censor.MaxLimitPerChat)
            return (false, result);
        return (true, null);
    }
}
