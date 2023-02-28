using BepInEx;
using BepInEx.Unity.Mono;
using BepInEx.Configuration;
using System;
using MBMScripts;
using HarmonyLib;
using Tools;

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
    /// Rest time.
    /// </summary>
    public static ConfigEntry<float>? RestTime;
    
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

        Tools.PeriodicActionRunner.RegisterPeriodicAction(60, SetRestTime);

        RestTime.SettingChanged += OnRestTimeChanged;
    }

    private static void OnRestTimeChanged(object sender, EventArgs e)
    {
        SetRestTime();
    }

    private static void SetRestTime()
    {
        if(GameManager.ConfigData == null) return;
        if(RestTime == null) return;
        log?.LogMessage("SetRestTime");
        Traverse.Create(GameManager.ConfigData).Field("m_RestTime").SetValue(RestTime.Value);
    }

    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
    
}