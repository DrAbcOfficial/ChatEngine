using ChatEngine.Commands;
using Metamod.Enum.Metamod;
using Metamod.Interface;
using Metamod.Wrapper.Metamod;

namespace ChatEngine;

/// <summary>
/// Plugin entry point: the name must be Plugin and must inherit from the IPlugin interface.
/// </summary>
public class Plugin : IPlugin
{
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
        return true;
    }

    public bool Meta_Detach(PluginLoadTime now, PluginUnloadReason reason)
    {
        return true;
    }
}
