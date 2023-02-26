using HarmonyLib;
using HarmonyLib.Tools;
using MBMScripts;

namespace Tools;

public static class SharedData {
    /// <summary>
    /// The GameManager instance
    /// </summary>
    public static GameManager? GM = null;

    /// <summary>
    /// Collect GameManager instance
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Load))]
    [HarmonyPostfix]
    public static void GetManager(GameManager __instance)
    {
        GM ??= __instance;
    }
}