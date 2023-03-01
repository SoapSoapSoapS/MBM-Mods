using BepInEx.Configuration;
using HarmonyLib;
using MBMScripts;
using Tools;

namespace Restless;

public static class RestTime
{
    /// <summary>
    /// Rest time.
    /// </summary>
    public static ConfigEntry<bool>? EnableRestTime;

    /// <summary>
    /// Rest time.
    /// </summary>
    public static ConfigEntry<float>? Seconds;

    public static void Initialize(ConfigFile config)
    {
        Seconds = config.Bind(new ConfigInfo<float>()
        {
            Section = "RestTime",
            Name = "Seconds",
            Description = "The time in seconds that a unit will rest before starting a new activity",
            AcceptableValues = new AcceptableValueRange<float>(1f, 64f),
            DefaultValue = 15
        });

        EnableRestTime = config.Bind(new ConfigInfo<bool>()
        {
            Section = "RestTime",
            Name = "Enabled",
            Description = "Whether to override the default value",
            DefaultValue = false
        });
    }

    /// <summary>
    /// Undo Trick
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(ConfigData), nameof(ConfigData.RestTime), MethodType.Getter)]
    [HarmonyPostfix]
    public static void OverrideRestTime(ref float __result)
    {
        if(EnableRestTime == null || Seconds == null) return;

        if(EnableRestTime.Value)
        {
            __result = Seconds.Value;
        }
    }
}