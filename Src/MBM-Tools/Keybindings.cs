using System;
using System.Collections.Generic;
using System.Linq;
using MBMScripts;
using UnityEngine;

namespace Tools;

public static class Keybindings {
    private static IDictionary<Guid, (KeyCode key, Action act)> bindings = new Dictionary<Guid, (KeyCode, Action)>();

    private static EGameWindow[] menuWindows = new EGameWindow[]
    {
        EGameWindow.Title,
        EGameWindow.Menu,
        EGameWindow.MenuSave,
        EGameWindow.MenuLoad,
        EGameWindow.MenuSound,
        EGameWindow.MenuHotKey,
        EGameWindow.MenuCode,
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

    public static void OnUpdate()
    {
        var areMenusOpen = menuWindows.Any(w => GameManager.Instance.GetWindowState(w));
        if(areMenusOpen) return;

        foreach(var kvp in bindings)
        {
            if(Input.GetKeyUp(kvp.Value.key))
            {
                kvp.Value.act();
            }
        }
    }
}