using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MBMScripts;
using UnityEngine;

namespace KeyBinds;

public static class Keybindings {
    private static IDictionary<Guid, (KeyCode key, Action act)> bindings = new Dictionary<Guid, (KeyCode, Action)>();

    private static EGameWindow[] gameWindows = (EGameWindow[]) Enum.GetValues(typeof(EGameWindow));

    private static EGameWindow[] expectedGameWindows = new EGameWindow[]
    {
        EGameWindow.Top,
        EGameWindow.Clock,
        EGameWindow.Bottom,
        EGameWindow.PayList
    };

    public static Guid RegisterKeybinding(KeyCode key, Action act)
    {
        var guid = Guid.NewGuid();
        RegisterKeybinding(guid, key, act);
        return guid;
    }

    public static void RegisterKeybinding(Guid guid, KeyCode key, Action act)
    {
        bindings[guid] = (key, act);
    }

    public static void DeregisterKeybinding(Guid guid)
    {
        bindings.Remove(guid);
    }

    [HarmonyPatch(typeof(PlayData), nameof(PlayData.Update))]
    [HarmonyPostfix]
    public static void OnUpdate()
    {
        var areMenusOpen = gameWindows.Where(w => !expectedGameWindows.Contains(w)).Any(w => GameManager.Instance.GetWindowState(w));
        if(areMenusOpen) return;

        foreach(var kvp in bindings)
        {
            if(Input.GetKeyUp(kvp.Value.key))
            {
                Plugin.log?.LogMessage("KeyPress");
                kvp.Value.act();
            }
        }
    }
}