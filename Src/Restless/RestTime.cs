using BepInEx.Configuration;
using HarmonyLib;
using MBMScripts;
using Tools;

namespace Restless;

public static class RestTime
{
    /// <summary>
    /// Rest time in seconds.
    /// </summary>
    public static ConfigEntry<float>? Seconds;

    /// <summary>
    /// If true, override default rest time.
    /// </summary>
    public static ConfigEntry<bool>? Enable;

    /// <summary>
    /// Initialize settings from config.
    /// </summary>
    public static void Initialize(ConfigFile config)
    {
        Seconds = config.Bind(new ConfigInfo<float>()
        {
            Section = nameof(RestTime),
            Name = nameof(Seconds),
            Description = "The time in seconds that a unit will rest before starting a new activity.",
            AcceptableValues = new AcceptableValueRange<float>(1f, 64f),
            DefaultValue = 15
        });

        Enable = config.Bind(new ConfigInfo<bool>()
        {
            Section = nameof(RestTime),
            Name = nameof(Enable),
            Description = "Allow custom RestTime.",
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
        if(Enable == null || Seconds == null) return;

        if(Enable.Value)
        {
            __result = Seconds.Value;
        }
    }
}