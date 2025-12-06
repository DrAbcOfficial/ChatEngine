using ChatEngine.Commands;
using ChatEngine.Lang;
using ChatEngine.Storage;
using NuggetMod.Enum.Metamod;
using NuggetMod.Interface;
using NuggetMod.Interface.Events;
using NuggetMod.Wrapper.Engine;
using NuggetMod.Wrapper.Metamod;
using System.Runtime.InteropServices;

namespace ChatEngine;

/// <summary>
/// Plugin entry point: the name must be Plugin and must inherit from the IPlugin interface.
/// </summary>
public class Plugin : IPlugin
{
    internal const string CMD_PREFIX = "cte_";
    /// <summary>
    /// Plugin information: it is recommended to set it as static to maintain memory availability.
    /// </summary>
    private readonly static MetaPluginInfo _pluginInfo = new()
    {
        InterfaceVersion = InterfaceVersion.V5_16,
        Name = "ChatEngine",
        Version = "1.0",
        Author = "Dr.Abc",
        Date = "2025/11/11",
        LogTag = "CTE",
        Url = "github.com",
        Loadable = PluginLoadTime.PT_ANYTIME,
        Unloadable = PluginLoadTime.PT_ANYTIME
    };

    public MetaPluginInfo GetPluginInfo()
    {
        return _pluginInfo;
    }

    public void Meta_Init()
    {

    }

    public bool Meta_Query(InterfaceVersion interfaceVersion, MetaUtilFunctions pMetaUtilFuncs)
    {
        if (interfaceVersion != _pluginInfo.InterfaceVersion)
            return false;
        return true;
    }

    public bool Meta_Attach(PluginLoadTime now, MetaGlobals pMGlobals, MetaGameDLLFunctions pGamedllFuncs)
    {
        BaseMetaModCommand.RegisterCommands();
        DLLEvents dLLEvents = new();
        dLLEvents.ClientCommand += player =>
        {
            int argc = MetaMod.EngineFuncs.Cmd_Argc();
            if (argc <= 0)
                return MetaResult.MRES_IGNORED;

            List<string> args = [];
            for (int i = 0; i < argc; i++)
            {
                args.Add(MetaMod.EngineFuncs.Cmd_Argv(i));
            }
            bool from_chat = false;
            if (args[0] == "say" || args[0] == "say_team")
            {
                from_chat = true;
                args = [.. args.Skip(1)];
            }
            if (args.Count > 0)
            {
                string name = args[0].Trim();
                if(from_chat)
                {
                    string trigger = name[..1];
                    if (ConfigManager.Instance.Config.ChatTrigger.Contains(trigger))
                        name = name[1..];
                    else
                        return MetaResult.MRES_IGNORED;
                }
                if (name.StartsWith(CMD_PREFIX))
                {
                    name = name[(CMD_PREFIX.Length)..];
                    if (BaseMetaModCommand.Commands.TryGetValue(name, out BaseMetaModCommand? instance))
                    {
                        args = [.. args.Skip(1)];
                        instance.ClientPreExcute([.. args], player, from_chat);
                        return MetaResult.MRES_SUPERCEDE;
                    }
                }
            }
            return MetaResult.MRES_IGNORED;
        };
        dLLEvents.ClientConnect += (player, pszName, pszAddress, ref szRejectReason) =>
        {
            PlayerInfo.PlayerConnected(player);
            return (MetaResult.MRES_HANDLED, true);
        };
        dLLEvents.ClientDisconnect += player =>
        {
            PlayerInfo.PlayerDisconnected(player);
            return MetaResult.MRES_HANDLED;
        };
        MetaMod.RegisterEvents(entityApi: dLLEvents);

        EngineEvents engineEvents = new();
        engineEvents.RegUserMsg += (pszName, iSize) =>
        {
            if(pszName == "TextMsg")
            {
                Language.MessageTextMsg = MetaMod.MetaGlobals.GetOriginalReturn<int>();
                return (MetaResult.MRES_HANDLED, 0);
            }
            return (MetaResult.MRES_IGNORED, 0);
        };
        MetaMod.RegisterEvents(engineFunctionsPost: engineEvents);
        return true;
    }

    public bool Meta_Detach(PluginLoadTime now, PluginUnloadReason reason)
    {
        return true;
    }
}
