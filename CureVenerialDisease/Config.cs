using BepInEx.Configuration;
using UnityEngine;
using System;

namespace CureVenerealDisease
{
    public class Config
    {
        public const string SECTION = "CvdConfig";

        public readonly ConfigEntry<bool> Enable;
        private readonly ConfigEntryInfo<bool> EnableInfo = new ConfigEntryInfo<bool>()
        {
            Section = SECTION,
            Name = nameof(Enable),
            DefaultValue = true,
            Description = new ConfigDescription("If true, enable mod")
        };

        public Config(ConfigFile config)
        {
            Enable = config.Bind(EnableInfo);
        }
    }

    public struct ConfigEntryInfo<T>
    {
        public string Section;
        public string Name;
        public T DefaultValue;
        public ConfigDescription Description;
    }

    public static class ConfigFileExtensions
    {
        public static ConfigEntry<T> Bind<T>(this ConfigFile config, ConfigEntryInfo<T> info)
        {
            return config.Bind(
                info.Section ?? throw new ArgumentNullException(nameof(info.Section)),
                info.Name ?? throw new ArgumentNullException(nameof(info.Name)),
                info.DefaultValue ?? throw new ArgumentNullException(nameof(info.DefaultValue)),
                info.Description);
        }
    }
}