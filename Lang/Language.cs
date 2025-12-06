using ChatEngine.Storage;
using NuggetMod.Enum.Engine;
using NuggetMod.Helper;
using NuggetMod.Interface;
using NuggetMod.Wrapper.Engine;

namespace ChatEngine.Lang;

internal class Language
{
    internal static int MessageTextMsg = 0;
    internal static int MessageSayText = 0;
    internal enum PrintTarget
    {
        Server = 0,
        ClientNotify = 1,
        ClientConsole = 2,
        ClientChat = 3,
        ClientCenter = 4
    }

    internal static void ClientPrintf(Edict? client, MessageDestination msg_dest, PrintTarget target, string message)
    {
        var msg = client == null ? new NetworkMessage(msg_dest, MessageTextMsg) : new NetworkMessage(msg_dest, MessageTextMsg, client);
            msg.WriteByte((byte)target)
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
                    ClientPrintf(client, MessageDestination.One, target, msg);
                    break;
                }
        }
    }
    internal static void PrintAll(string code, PrintTarget target)
    {
        string msg = GetTranlation(code);
        switch (target)
        {
            case PrintTarget.Server: MetaMod.EngineFuncs.ServerPrint(msg); break;
            default:
                {
                    ClientPrintf(null, MessageDestination.Broadcast, target, msg);
                    break;
                }
        }
    }

    internal static void SayText(string message, MessageDestination msg_dest, Edict player, Edict? client)
    {
        var msg = client == null ? new NetworkMessage(msg_dest, MessageSayText) : new NetworkMessage(msg_dest, MessageSayText, client);
        msg.WriteByte((byte)MetaMod.EngineFuncs.IndexOfEdict(player))
            .WriteByte(2)
            .WriteString(message)
            .Send();
    }
}

