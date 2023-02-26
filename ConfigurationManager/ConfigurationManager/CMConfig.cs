using BepInEx.Configuration;

namespace ConfigurationManager
{
    public class CMConfig
    {
        /// <summary>
        /// When enabled do not show section name for plugins with only one section
        /// </summary>
        public static ConfigEntry<bool> HideSingleSection { get; private set; }

        /// <summary>
        /// When enabled show advanced configuration
        /// </summary>
        public static ConfigEntry<bool> _showAdvanced { get; private set; }

        /// <summary>
        /// Show KeyBind type settings
        /// </summary>
        public static ConfigEntry<bool> _showKeybinds { get; private set; }

        /// <summary>
        /// Show settings at all
        /// </summary>
        public static ConfigEntry<bool> _showSettings { get; private set; }

        /// <summary>
        /// Show plugins collapsed by default
        /// </summary>
        public static ConfigEntry<bool> _pluginConfigCollapsedDefault { get; private set; }

        public static void InitializeConfigs(ConfigFile config)
        {
            _showAdvanced = config.Bind("Filtering", "Show advanced", false);
            _showKeybinds = config.Bind("Filtering", "Show keybinds", true);
            _showSettings = config.Bind("Filtering", "Show settings", true);
            new ConfigDescription("The shortcut used to toggle the config manager window on and off.\n" +
                                  "The key can be overridden by a game-specific plugin if necessary, in that case this setting is ignored.");
            HideSingleSection = config.Bind("General", "Hide single sections", false, new ConfigDescription("Show section title for plugins with only one section"));
            _pluginConfigCollapsedDefault = config.Bind("General", "Plugin collapsed default", true, new ConfigDescription("If set to true plugins will be collapsed when opening the configuration manager window"));
        }
    }
}
