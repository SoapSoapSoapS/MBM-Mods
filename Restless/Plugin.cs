using BepInEx;
using BepInEx.Unity.Mono;
using BepInEx.Configuration;

namespace Restless;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    /// <summary>
    /// Mod log instance for injected code.
    /// </summary>
    public static BepInEx.Logging.ManualLogSource? log;

    /// <summary>
    /// Rest time.
    /// </summary>
    public ConfigEntry<float> RestTime;
    
    /// <summary>
    /// Initialize logger.
    /// </summary>
    public Plugin()
    {
        log = Logger;
        RestTime = Config.Bind(new ConfigInfo<float>()
        {
            Section = "General",
            Name = "RestTime",
            Description = "The time that a unit will rest before starting a new activity",
            AcceptableValues = new AcceptableValueRange<float>(1f, 64f),
            DefaultValue = 5
        });
    }

    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
    
}