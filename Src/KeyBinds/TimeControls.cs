using System;
using BepInEx.Configuration;
using MBMScripts;
using Tools;
using UnityEngine;

namespace KeyBinds;

public static class TimeControls
{
    /// <summary>
    /// Speed 1.5x
    /// </summary>
    public static ConfigEntry<KeyCode>? Speed1_5;

    /// <summary>
    /// Speed 1.5x
    /// </summary>
    public static ConfigEntry<KeyCode>? Speed2;

    /// <summary>
    /// Speed 1.5x
    /// </summary>
    public static ConfigEntry<KeyCode>? Speed3;

    /// <summary>
    /// Speed 1.5x
    /// </summary>
    public static ConfigEntry<KeyCode>? Speed4;

    /// <summary>
    /// Speed 1.5x
    /// </summary>
    public static ConfigEntry<KeyCode>? Speed5;

    public static void Initialize(ConfigFile config)
    {
        Speed1_5 = config.Bind(
            new ConfigInfo<KeyCode>()
            {
                Section = nameof(TimeControls),
                Name = nameof(Speed1_5),
                Description = "Keybind for 1.5x speed",
                DefaultValue = KeyCode.None
            }
        );

        Speed2 = config.Bind(
            new ConfigInfo<KeyCode>()
            {
                Section = nameof(TimeControls),
                Name = nameof(Speed2),
                Description = "Keybind for 2x speed",
                DefaultValue = KeyCode.None
            }
        );

        Speed3 = config.Bind(
            new ConfigInfo<KeyCode>()
            {
                Section = nameof(TimeControls),
                Name = nameof(Speed3),
                Description = "Keybind for 3x speed",
                DefaultValue = KeyCode.None
            }
        );

        Speed4 = config.Bind(
            new ConfigInfo<KeyCode>()
            {
                Section = nameof(TimeControls),
                Name = nameof(Speed4),
                Description = "Keybind for 4x speed",
                DefaultValue = KeyCode.None
            }
        );

        Speed5 = config.Bind(
            new ConfigInfo<KeyCode>()
            {
                Section = nameof(TimeControls),
                Name = nameof(Speed5),
                Description = "Keybind for 5x speed",
                DefaultValue = KeyCode.None
            }
        );

        var id1_5 = Keybindings.RegisterKeybinding(Speed1_5.Value, SetSpeed(1.5f));
        var id2 = Keybindings.RegisterKeybinding(Speed2.Value, SetSpeed(2f));
        var id3 = Keybindings.RegisterKeybinding(Speed3.Value, SetSpeed(3f));
        var id4 = Keybindings.RegisterKeybinding(Speed4.Value, SetSpeed(4f));
        var id5 = Keybindings.RegisterKeybinding(Speed5.Value, SetSpeed(5f));

        Speed1_5.SettingChanged += OnUpdateSetting(id1_5, 1.5f);
        Speed2.SettingChanged += OnUpdateSetting(id2, 2f);
        Speed3.SettingChanged += OnUpdateSetting(id3, 3f);
        Speed4.SettingChanged += OnUpdateSetting(id4, 4f);
        Speed5.SettingChanged += OnUpdateSetting(id5, 5f);
    }

    private static Action SetSpeed(float f)
    {
        return () =>
        {
            GameManager.Instance.GameSpeed = f;
        };
    }

    private static EventHandler OnUpdateSetting(Guid guid, float f)
    {
        return (object sender, EventArgs e) =>
        {
            var newKey = (KeyCode)((ConfigEntryBase)sender).BoxedValue;
            Keybindings.RegisterKeybinding(guid, newKey, SetSpeed(f));
        };
    }
}
