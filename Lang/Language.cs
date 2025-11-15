using Metamod.Interface;
using Metamod.Wrapper.Engine;

namespace ChatEngine.Lang;
internal class Language
{
    internal enum PrintTarget
    {
        Server,
        ClientChat,
        ClientConcole
    }

    internal static void Print(string code, PrintTarget target, Edict? client = null)
    {
        string msg = code;
        switch (target)
        {
            case PrintTarget.Server: MetaMod.EngineFuncs.ServerPrint(msg); break;
            case PrintTarget.ClientChat: break;
            case PrintTarget.ClientConcole:
                {
                    if (client == null)
                        return;
                    MetaMod.EngineFuncs.ClientPrintf(client, Metamod.Enum.Common.PrintType.print_console, msg); break;
                }
        }
    }
}

