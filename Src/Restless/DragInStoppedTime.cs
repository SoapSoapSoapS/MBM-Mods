using BepInEx.Configuration;
using HarmonyLib;
using MBMScripts;
using Tools;

namespace Restless;

public static class DragInStoppedTime
{
    /// <summary>
    /// Rest time.
    /// </summary>
    public static ConfigEntry<bool>? Enable;

    public static void Initialize(ConfigFile config)
    {
        Enable = config.Bind(new ConfigInfo<bool>()
        {
            Section = nameof(DragInStoppedTime),
            Name = nameof(Enable),
            Description = "Allows dragging units in stopped time.",
            DefaultValue = false
        });
    }

    /// <summary>
    /// Trick drag event into thinking gamespeed is zero
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(InteractionUnit), nameof(InteractionUnit.Drag))]
    [HarmonyPrefix]
    public static void PreDrag(GameManager __instance)
    {
        GameManager.Instance.GameSpeedIsZero = false;
    }

    /// <summary>
    /// Undo Trick
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(InteractionUnit), nameof(InteractionUnit.Drag))]
    [HarmonyPostfix]
    public static void PostDrag(GameManager __instance)
    {
        if(Enable == null) return;
        if(!Enable.Value) return;

        GameManager.Instance.GameSpeedIsZero = true;
    }
}