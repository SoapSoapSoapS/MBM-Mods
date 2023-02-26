using System;
using HarmonyLib;
using HarmonyLib.Tools;
using MBMScripts;
using System.Collections.Generic;

namespace Tools;

public static class PeriodicActionRunner {
    /// <summary>
    /// List of actions to be run periodically
    /// </summary>
    private static readonly IDictionary<float, PeriodicActionGroup> PeriodicActionGroups = new Dictionary<float, PeriodicActionGroup>();

    /// <summary>
    /// Registers an action to run approximatley every "period" seconds.
    /// </summary>
    public static PeriodicAction RegisterPeriodicAction(float period, Action act)
    {
        var paction = new PeriodicAction(act);

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
}