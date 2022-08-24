using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx;
using System.Linq;

namespace SBH.ConfigurationManager
{
    public static class SettingSearcher
    {
        /// <summary>
        /// Get entries for all core BepInEx settings
        /// </summary>
        public static List<PluginInfo> GetAllPluginsWithConfig()
        {
            var result = new Dictionary<BasePlugin, ConfigEntryBase[]>();
            return IL2CPPChainloader.Instance.Plugins
                .Where(kvp => kvp.Value.Instance is BasePlugin p && p.Config.Any())
                .Where(kvp => !(kvp.Value.Instance is CMPlugin))
                .Select(kvp => kvp.Value)
                .ToList();
        }

        /// <summary>
        /// Get entries for all core BepInEx settings
        /// </summary>
        public static List<PluginInfo> GetAllPluginsWithoutConfig()
        {
            var result = new Dictionary<BasePlugin, ConfigEntryBase[]>();
            return IL2CPPChainloader.Instance.Plugins
                .Where(kvp => kvp.Value.Instance is BasePlugin p && !p.Config.Any())
                .Where(kvp => !(kvp.Value.Instance is CMPlugin))
                .Select(kvp => kvp.Value)
                .ToList();
        }
    }
}
