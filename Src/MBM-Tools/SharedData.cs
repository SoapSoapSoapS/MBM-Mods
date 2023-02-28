using HarmonyLib;
using MBMScripts;

namespace Tools;

public static class SharedData {
    /// <summary>
    /// The GameManager instance
    /// </summary>
    public static GameManager? GM = null;

    /// <summary>
    /// Collect GameManager instance on new game
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Play))]
    [HarmonyPostfix]
    public static void OnPlay(GameManager __instance)
    {
        SharedData.GM ??= __instance;
    }

    /// <summary>
    /// Collect GameManager instance on load
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Load))]
    [HarmonyPostfix]
    public static void OnLoad(GameManager __instance)
    {
        SharedData.GM ??= __instance;
    }
}