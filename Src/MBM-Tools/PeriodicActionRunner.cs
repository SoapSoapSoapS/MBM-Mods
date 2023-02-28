using System;
using HarmonyLib;
using MBMScripts;
using System.Collections.Generic;

namespace Tools;

public static class PeriodicActionRunner {
    /// <summary>
    /// List of actions to be run periodically
    /// </summary>
    private static readonly IDictionary<float, PeriodicActionGroup> PeriodicActionGroups = new Dictionary<float, PeriodicActionGroup>();

    /// <summary>
    /// Actions to run against the GameManager instance on load
    /// </summary>
    private static IList<CustomAction> OnLoadActions = new List<CustomAction>();

    /// <summary>
    /// Registers an action to run approximatley every "period" seconds.
    /// </summary>
    public static CustomAction RegisterPeriodicAction(float period, Action act)
    {
        var paction = new CustomAction(act);

        if(PeriodicActionGroups.TryGetValue(period, out var pag))
        {
            pag.actions.Add(paction);
        }
        else
        {
            PeriodicActionGroups.Add(period, new PeriodicActionGroup(period, paction));
        }

        return paction;
    }

    /// <summary>
    /// Add a new action to run against the GameManager instance on load
    /// </summary>
    public static CustomAction RegisterOnLoadAction(Action action)
    {
        var customAction = new CustomAction(action);
        OnLoadActions.Add(customAction);
        return customAction;
    }

    /// <summary>
    /// Remove a registered onLoad action
    /// </summary>
    public static bool DeregisterOnLoadAction(CustomAction action)
    {
        return OnLoadActions.Remove(action);
    }

    /// <summary>
    /// Run Periodic Actions
    /// </summary>
    [HarmonyPatch(typeof(PlayData), nameof(PlayData.Update))]
    [HarmonyPostfix]
    public static void OnUpdate(float deltaTime)
    {
        foreach(var kvp in PeriodicActionGroups)
        {
            var pag = kvp.Value;
            pag.timeSinceRun += deltaTime;
            if(pag.timeSinceRun > pag.period)
            {
                try
                {
                    pag.Act();
                }
                catch
                {
                    Plugin.log?.LogError("Error in registered plugin");
                }
                pag.timeSinceRun = 0;
            }
        }
    }

    /// <summary>
    /// Collect GameManager instance on load
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Load))]
    [HarmonyPostfix]
    public static void OnLoad(GameManager __instance)
    {
        foreach (var action in OnLoadActions)
        {
            action.act();
        }
    }
}