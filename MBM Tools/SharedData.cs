using System.Collections.Generic;
using System;
using HarmonyLib;
using MBMScripts;

namespace Tools;

public static class SharedData {
    /// <summary>
    /// The GameManager instance
    /// </summary>
    public static GameManager? GM = null;

    /// <summary>
    /// Actions to run against the GameManager instance on load
    /// </summary>
    private static IList<CustomAction<GameManager>> OnLoadActions = new List<CustomAction<GameManager>>();

    /// <summary>
    /// Add a new action to run against the GameManager instance on load
    /// </summary>
    public static CustomAction<GameManager> RegisterOnLoadAction(Action<GameManager> action)
    {
        var customAction = new CustomAction<GameManager>(action);
        OnLoadActions.Add(customAction);
        return customAction;
    }

    /// <summary>
    /// Remove a registered onLoad action
    /// </summary>
    public static bool DeregisterOnLoadAction(CustomAction<GameManager> action)
    {
        return OnLoadActions.Remove(action);
    }

    /// <summary>
    /// Collect GameManager instance
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Load))]
    [HarmonyPostfix]
    public static void GetManager(GameManager __instance)
    {
        GM = __instance;

        foreach (var action in OnLoadActions)
        {
            action.act(__instance);
        }
    }
}