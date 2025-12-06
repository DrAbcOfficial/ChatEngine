using ChatEngine.Commands;
using ChatEngine.Lang;
using ChatEngine.Storage;
using NuggetMod.Enum.Common;
using NuggetMod.Enum.Engine;
using NuggetMod.Enum.Metamod;
using NuggetMod.Helper;
using NuggetMod.Interface;
using NuggetMod.Interface.Events;
using NuggetMod.Wrapper.Metamod;
using SQLitePCL;
using System.Runtime.InteropServices;

namespace ChatEngine;

/// <summary>
/// Plugin entry point: the name must be Plugin and must inherit from the IPlugin interface.
/// </summary>
public class Plugin : IPlugin
{
    internal const string CMD_PREFIX = "cte_";
    internal static SQL SQLStorage = new();
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
        Loadable = PluginLoadTime.Anytime,
        Unloadable = PluginLoadTime.Anytime
    };

    public MetaPluginInfo GetPluginInfo()
    {
        return _pluginInfo;
    }

    public void MetaInit()
    {

    }

    public bool MetaQuery(InterfaceVersion interfaceVersion, MetaUtilFunctions pMetaUtilFuncs)
    {
        if (interfaceVersion != _pluginInfo.InterfaceVersion)
            return false;
        static string GetNativeLibraryName()
        {
            if (OperatingSystem.IsWindows())
                return "e_sqlite3.dll";
            else if (OperatingSystem.IsLinux())
                return "libe_sqlite3.so";
            else if (OperatingSystem.IsMacOS())
                return "libe_sqlite3.dylib";
            else
                throw new PlatformNotSupportedException();
        }

        string gamePath = pMetaUtilFuncs.GetGameInfo(GetGameInfoType.GameDirectory);
        try
        {
            string libPath = Path.Combine(gamePath, "addons/chatengine/dlls/", GetNativeLibraryName());
            NativeLibrary.Load(libPath);
            Batteries.Init();
        }
        catch (Exception)
        {
            pMetaUtilFuncs.LogError("failed to load e_sqlite3.dll");
            return false;
        }

        //Force load config
        var config = ConfigManager.Instance;
        SQLStorage.Initialize(config.Config.SQLStoragePath);
        return true;
    }

    public bool MetaAttach(PluginLoadTime now, MetaGlobals pMGlobals, MetaGameDLLFunctions pGamedllFuncs)
    {
        BaseMetaModCommand.RegisterCommands();
        DLLEvents dLLEvents = new();
        dLLEvents.ClientCommand += player =>
        {
            int argc = MetaMod.EngineFuncs.Cmd_Argc();
            if (argc <= 0)
                return MetaResult.Ignored;

            List<string> args = [];
            for (int i = 0; i < argc; i++)
            {
                args.Add(MetaMod.EngineFuncs.Cmd_Argv(i));
            }
            bool from_chat = false;
            string msg = MetaMod.EngineFuncs.Cmd_Args();
            bool is_teamchat = false;
            if (args[0] == "say" || args[0] == "say_team")
            {
                is_teamchat = args[0] == "say_team";
                PlayerInfo.InsterNewChatLog(player, msg, is_teamchat);
                from_chat = true;
                args = [.. args.Skip(1)];
            }
            if (args.Count > 0)
            {
                string name = args[0].Trim();
                if (from_chat)
                {
                    string trigger = name[..1];
                    if (ConfigManager.Instance.Config.ChatTrigger.Contains(trigger))
                        name = name[1..];
                    else
                    {
                        string trimmed = msg.Trim().Trim('"');
                        string content = $"{(player.EntVars.DeadFlag != DeadFlag.No ? Language.GetTranlation("chat.tag.dead") : "")}" +
                            $"{(is_teamchat ? Language.GetTranlation("chat.tag.team") : "")} {player.EntVars.NetName}: {trimmed}";
                        //重新打印一次，防止傻逼非asiic检查
                        if (!is_teamchat)
                            Language.SayText(content, MessageDestination.Broadcast, player, null);
                        else
                        {
                            Utility.GetAllPlayers().ForEach(p =>
                            {
                                if (p.EntVars.Team == player.EntVars.Team)
                                    Language.SayText(content, MessageDestination.One, player, p);
                            });
                        }
                        return MetaResult.SuperCEDE;
                    }
                }
                if (name.StartsWith(CMD_PREFIX))
                {
                    name = name[(CMD_PREFIX.Length)..];
                    if (BaseMetaModCommand.Commands.TryGetValue(name, out BaseMetaModCommand? instance))
                    {
                        args = [.. args.Skip(1)];
                        instance.ClientPreExcute([.. args], player, from_chat);
                        return MetaResult.SuperCEDE;
                    }
                }
            }
            return MetaResult.Ignored;
        };
        dLLEvents.ClientConnect += (player, pszName, pszAddress, ref szRejectReason) =>
        {
            var result = PlayerInfo.PlayerConnected(player, pszName);
            if (!result.Item1)
            {
                string datetimestr = new DateTimeOffset(result.Item2.BannedUntil!.Value).ToString("o");
                szRejectReason = string.Format(Language.GetTranlation("player.banned.connect"), datetimestr);
                return (MetaResult.SuperCEDE, false);
            }
            return (MetaResult.Handled, true);
        };
        dLLEvents.ClientDisconnect += player =>
        {
            PlayerInfo.PlayerDisconnected(player);
            return MetaResult.Handled;
        };
        EngineEvents engineEvents = new();
        engineEvents.RegUserMsg += (pszName, iSize) =>
        {
            switch (pszName)
            {
                case "TextMsg":
                    {
                        Language.MessageTextMsg = MetaMod.MetaGlobals.GetOriginalReturn<int>();
                        return (MetaResult.Handled, 0);
                    }
                case "SayText":
                    {
                        Language.MessageSayText = MetaMod.MetaGlobals.GetOriginalReturn<int>();
                        return (MetaResult.Handled, 0);
                    }
            }
            return (MetaResult.Ignored, 0);
        };

        MetaMod.RegisterEvents(entityApi: dLLEvents, engineFunctionsPost: engineEvents);
        return true;
    }

    public bool MetaDetach(PluginLoadTime now, PluginUnloadReason reason)
    {
        return true;
    }
}
