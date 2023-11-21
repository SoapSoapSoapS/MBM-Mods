using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using HarmonyLib.Tools;

namespace PunnettRebalance;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Tools.MyPluginInfo.PLUGIN_GUID, Tools.MyPluginInfo.PLUGIN_VERSION)]
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

        PunnettInheritance.Initialize(Config);
    }

    private void Awake()
    {
        try
        {
            Logger.LogMessage("Starting Harmony Patch");
            HarmonyFileLog.Enabled = true;
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(PunnettInheritance));

            Logger.LogMessage("Harmony Patch Successful");
        }
        catch
        {
            Logger.LogWarning("Harmony Patch Failed");
        }
    }
}
