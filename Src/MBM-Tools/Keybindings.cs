using System;
using System.Collections.Generic;
using System.Linq;
using MBMScripts;
using UnityEngine;

namespace Tools;

public static class Keybindings
{
    private static IDictionary<Guid, (KeyCode key, Action act)> registeredBindings =
        new Dictionary<Guid, (KeyCode, Action)>();
    private static IDictionary<KeyCode, Action> bindings = new Dictionary<KeyCode, Action>();
    private static bool keyActivityDetected;

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
        if (key == KeyCode.None)
        {
            registeredBindings.Remove(guid);
            bindings.Remove(key);
            return;
        }

        if (registeredBindings.Any(b => b.Value.key == key))
        {
            foreach (var e in registeredBindings.Where(b => b.Value.key == key))
            {
                registeredBindings.Remove(e.Key);
                bindings.Remove(key);
            }
        }

        registeredBindings[guid] = (key, act);
        bindings[key] = act;
    }

    public static void DeregisterKeybinding(Guid guid)
    {
        if (registeredBindings.TryGetValue(guid, out var v))
        {
            bindings.Remove(v.key);
            registeredBindings.Remove(guid);
        }
    }

    public static void OnUpdate()
    {
        if (Input.anyKey)
        {
            keyActivityDetected = true;
        }

        if (!keyActivityDetected)
            return;

        var areMenusOpen = menuWindows.Any(GameManager.Instance.GetWindowState);
        if (areMenusOpen)
            return;

        foreach (var key in bindings.Keys)
        {
            if (Input.GetKeyUp(key))
            {
                bindings[key]();
            }
        }

        if (!Input.anyKey)
        {
            keyActivityDetected = false;
        }
    }
}
