using ChatEngine.Storage;
using NuggetMod.Interface;

namespace ChatEngine.Censor;

internal static class StringChecker
{
    private static DFANode? RootDFA;

    internal static void BuildDFA(string gamePath)
    {
        (RootDFA, uint count, uint terminals) = DFANode.CreateFromFile(Path.Combine(gamePath, ConfigManager.Instance.Config.Censor.CensorWordsFilePath));
        MetaMod.EngineFuncs.ServerPrint($"ChatEngine Loaded {count} DFA nodes, {terminals} DFA terminals\n");
    }
    internal static DFANode.CheckResult[] Check(string str)
    {
        if (RootDFA == null)
            throw new NullReferenceException("Root DFA is NULL");
        return RootDFA.CheckString(str);
    }
}
