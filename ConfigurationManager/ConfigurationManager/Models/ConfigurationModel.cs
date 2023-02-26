using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using ConfigurationManager;

namespace ConfigurationManager.Models
{
    public class ConfigurationWindowModel
    {
        /// <summary>
        /// List of all plugins with configurations
        /// </summary>
        public readonly IList<PluginModel> Plugins;

        /// <summary>
        /// List of all plugins without configurations
        /// </summary>
        public readonly string PluginsWithoutSettings;

        /// <summary>
        /// Current search input
        /// </summary>
        public string SearchString
        {
            get => _searchString;
            set
            {
                if (value == null)
                    value = string.Empty;

                if (_searchString == value)
                    return;

                _searchString = value;

                Filter();
            }
        }
        private static string _searchString = string.Empty;

        /// <summary>
        /// When true, set focus to the searchbox
        /// </summary>
        public bool FocusSearchBox;

        /// <summary>
        /// Size of column with Sections/Names
        /// </summary>
        public int LeftColumnWidth;

        /// <summary>
        /// Size of column with Values/Inputs
        /// </summary>
        public int RightColumnWidth;

        /// <summary>
        /// Size of window
        /// </summary>
        public Rect SettingWindowRect;

        /// <summary>
        /// Size of screen
        /// </summary>
        public Rect ScreenRect;

        /// <summary>
        /// Location in window scroll
        /// </summary>
        public Vector2 SettingWindowScrollPos;

        /// <summary>
        /// Height of DrawTips() entry
        /// </summary>
        public int TipsHeight;

        /// <summary>
        /// Show more information for debugging
        /// </summary>
        public static bool IsDebug;

        public ConfigurationWindowModel()
        {
            CalculateWindowRect();

            Plugins = SettingSearcher.GetAllPluginsWithConfig()
                .Select(p => new PluginModel(p, this)).ToList();

            PluginsWithoutSettings = string.Join(", ", SettingSearcher.GetAllPluginsWithoutConfig().Select(p => p.Metadata.GUID));
        }

        public void Filter()
        {
            foreach (var plugin in Plugins)
            {
                plugin.Filter(_searchString);
            }
        }

        private void CalculateWindowRect()
        {
            var width = Mathf.Min(Screen.width, 650);
            var height = Screen.height < 560 ? Screen.height : Screen.height - 100;
            var offsetX = Mathf.RoundToInt((Screen.width - width) / 2f);
            var offsetY = Mathf.RoundToInt((Screen.height - height) / 2f);
            SettingWindowRect = new Rect(offsetX, offsetY, width, height);

            ScreenRect = new Rect(0, 0, Screen.width, Screen.height);

            LeftColumnWidth = Mathf.RoundToInt(SettingWindowRect.width / 2.5f);
            RightColumnWidth = (int)SettingWindowRect.width - LeftColumnWidth - 115;
        }
    }
}
