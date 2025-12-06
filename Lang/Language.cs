using ChatEngine.Storage;
using NuggetMod.Enum.Engine;
using NuggetMod.Helper;
using NuggetMod.Interface;
using NuggetMod.Wrapper.Engine;

namespace ChatEngine.Lang;

internal class Language
{
    internal static int MessageTextMsg = 0;
    internal enum PrintTarget
    {
        Server = 0,
        ClientNotify = 1,
        ClientConsole = 2,
        ClientChat = 3,
        ClientCenter = 4
    }

    internal static void DoClientPrintf(Edict client, MessageDestination msg_dest, PrintTarget target, string message)
    {
        new NetworkMessage(msg_dest, MessageTextMsg, client)
            .WriteByte((byte)target)
            .WriteString($"{message}\n")
            .Send();
    }

    internal static string GetTranlation(string code)
    {
        if (ConfigManager.Instance.Config.Language.Lang.TryGetValue(code, out var lang))
            return lang;
        return code;
    }
    internal static void Print(string code, PrintTarget target, Edict? client = null)
    {
        string msg = GetTranlation(code);
        switch (target)
        {
            case PrintTarget.Server: MetaMod.EngineFuncs.ServerPrint(msg); break;
            default:
                {
                    if (client == null)
                        return;
                    DoClientPrintf(client, MessageDestination.One, target, msg);
                    break;
                }
        }
    }
}

