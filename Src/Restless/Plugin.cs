using BepInEx;
using BepInEx.Unity.Mono;
using BepInEx.Configuration;
using System;
using MBMScripts;
using HarmonyLib;
using Tools;
using HarmonyLib.Tools;

namespace Restless;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Tools.MyPluginInfo.PLUGIN_GUID, Tools.MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    /// <summary>
    /// Mod log instance for injected code.
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
        try
        {
            Logger.LogMessage("Starting Harmony Patch");
            HarmonyFileLog.Enabled = true;
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(DragInStoppedTime));
            harmony.PatchAll(typeof(RestTime));

            Logger.LogMessage("Harmony Patch Successful");
        }
        catch
        {
            Logger.LogWarning("Harmony Patch Failed");
        }
    }
    
}