using ChatEngine.Censor;
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
        //Dictionary Initialize
        StringChecker.BuildDFA(gamePath);
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
                var printTarget = from_chat ? Language.PrintTarget.ClientChat : Language.PrintTarget.ClientConsole;
                if (from_chat)
                {
                    string trigger = name[..1];
                    if (ConfigManager.Instance.Config.ChatTrigger.Contains(trigger))
                        name = name[1..];
                    else
                    {
                        PlayerInfo? info = PlayerInfo.GetPlayerInfo(player);
                        if (info == null)
                        {
                            Language.PrintWithLang("player.info.lost", printTarget);
                            return MetaResult.SuperCEDE;
                        }
                        if (info.GaggedUntil.HasValue && DateTime.UtcNow <= info.GaggedUntil.Value)
                        {
                            Language.PrintWithLang("player.gagged", printTarget, player, (info.GaggedUntil.Value - DateTime.UtcNow).TotalMinutes);
                            return MetaResult.SuperCEDE;
                        }
                        string trimmed = msg.Trim().Trim('"');
                        //词语检查
                        var checkResult = StringChecker.Check(trimmed);
                        if (checkResult != null && checkResult.Length > 0)
                        {
                            string detected = string.Empty;
                            foreach (var arg in checkResult)
                            {
                                detected += $"{arg.Matched}/";
                            }
                            PlayerInfo.IncreseCensorCount(player, trimmed, detected);
                            var nicewd = ConfigManager.Instance.Config.Censor.NiceWords;
                            trimmed = nicewd[MetaMod.EngineFuncs.RandomLong(0, nicewd.Count - 1)];
                        }
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
                if (name.StartsWith(ConfigManager.Instance.Config.CommandPrefix))
                {
                    name = name[(ConfigManager.Instance.Config.CommandPrefix.Length)..];
                    if (BaseMetaModCommand.Commands.TryGetValue(name, out BaseMetaModCommand? instance))
                    {
                        args = [.. args.Skip(1)];
                        bool result = instance.ClientPreExcute([.. args], player, from_chat);
                        Language.PrintWithLang(result ? "command.exec.success" : "command.exec.failed",
                            printTarget,
                            player, instance.Name);
                        if (!result)
                        {
                            string desc = $"{ConfigManager.Instance.Config.CommandPrefix}{instance.Name} ";
                            foreach (var arg in instance.Arguments)
                            {
                                desc += (arg.Optional ? $"[{arg.Name}] " : $"<{arg.Name}> ");
                            }
                            Language.Print(desc, printTarget, player);
                        }
                        return MetaResult.SuperCEDE;
                    }
                }
            }
            return MetaResult.Ignored;
        };
        dLLEvents.ClientConnect += (player, pszName, pszAddress, ref szRejectReason) =>
        {
            string steamid = PlayerInfo.GetPlayerSteamID(player);
            //检查用户名
            var check = StringChecker.Check(pszName);
            if (check.Length > 0)
            {
                szRejectReason = Language.GetTranlation("player.shitname");
                string detected = string.Empty;
                foreach (var arg in check)
                {
                    detected += $"{arg.Matched}/";
                }
                Task.Factory.StartNew(() =>
                {
                    SQLStorage.LogDetected(steamid, 1, pszName, $"{detected}");
                });
                return (MetaResult.SuperCEDE, false);
            }
            var result = PlayerInfo.PlayerConnected(player, pszName, pszAddress);
            if (!result.Item1)
            {
                string datetimestr = result.Item2.BannedUntil!.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                szRejectReason = string.Format(Language.GetTranlation("player.banned.connect"), $"{datetimestr}(UTC)");
                return (MetaResult.SuperCEDE, false);
            }
            return (MetaResult.Handled, true);
        };
        dLLEvents.ClientDisconnect += player =>
        {
            PlayerInfo.PlayerDisconnected(player);
            return MetaResult.Handled;
        };
        dLLEvents.ServerActivate += (pEdictList, edictCount, clientMax) =>
        {
            PlayerInfo.ClearAllDetected();
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
