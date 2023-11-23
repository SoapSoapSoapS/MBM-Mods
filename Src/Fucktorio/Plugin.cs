using BepInEx;
using BepInEx.Unity.Mono;

namespace Boilerplate;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    /// <summary>
    /// Mod log instance
    /// </summary>
    public static BepInEx.Logging.ManualLogSource? log;

    /// <summary>
    /// Initialize logger.
    /// </summary>
    public Plugin()
    {
        log = Logger;
    }

    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
}
