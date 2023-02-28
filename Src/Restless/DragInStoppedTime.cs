using HarmonyLib;
using MBMScripts;

namespace Restless;

public static class DragInStoppedTime
{
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
        GameManager.Instance.GameSpeedIsZero = true;
    }
}