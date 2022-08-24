using SBH.ConfigurationManager.Models;
using SBH.ConfigurationManager.Utilities;
using Il2CppSystem;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnhollowerBaseLib;

namespace SBH.ConfigurationManager.Drawers
{
    public static class ConfigurationDrawer
    {
        /// <summary>
        /// Background for Tooltip
        /// </summary>
        public static Texture2D? TooltipBg { get; private set; }

        /// <summary>
        /// Background for Config Window
        /// </summary>
        public static Texture2D? WindowBackground { get; private set; }

        /// <summary>
        /// Color for Advanced Settings
        /// </summary>
        public static readonly Color AdvancedSettingColor = new Color(1f, 0.95f, 0.67f, 1f);

        /// <summary>
        /// Window Id, used to identify Window GUI entity
        /// </summary>
        public const int WindowId = -68;

        /// <summary>
        /// Label for search box
        /// </summary>
        public const string SearchBoxName = "searchBox";

        public static void InitializeTextureConstants()
        {
            var background = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            background.SetPixel(0, 0, Color.black);
            background.Apply();
            TooltipBg = background;

            var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            windowBackground.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 1));
            windowBackground.Apply();
            WindowBackground = windowBackground;
        }

        public static void DrawConfiguration(ConfigurationWindowModel configuration)
        {
            DrawWindowHeader(configuration);

            configuration.SettingWindowScrollPos = GUILayout.BeginScrollView(configuration.SettingWindowScrollPos, false, true);

            var scrollPosition = configuration.SettingWindowScrollPos.y;
            var scrollHeight = configuration.SettingWindowRect.height;

            GUILayout.BeginVertical();
            
            if (string.IsNullOrEmpty(configuration.SearchString))
            {
                DrawTips(configuration);

                if (configuration.TipsHeight == 0 && Event.current.type == EventType.Repaint)
                    configuration.TipsHeight = (int)GUILayoutUtility.GetLastRect().height;
            }

            var currentHeight = configuration.TipsHeight;

            foreach (var plugin in configuration.Plugins)
            {
                var visible = plugin.Height == 0 || currentHeight + plugin.Height >= scrollPosition && currentHeight <= scrollPosition + scrollHeight;

                if (visible)
                {
                    try
                    {
                        PluginDrawer.DrawPlugin(plugin, string.IsNullOrEmpty(configuration.SearchString));
                    }
                    catch (System.Exception)
                    {
                        // Needed to avoid GUILayout: Mismatched LayoutGroup.Repaint crashes on large lists
                    }

                    if (plugin.Height == 0 && Event.current.type == EventType.Repaint)
                        plugin.Height = (int)GUILayoutUtility.GetLastRect().height;
                }
                else
                {
                    try
                    {
                        GUILayout.Space(plugin.Height);
                    }
                    catch (System.ArgumentException)
                    {
                        // Needed to avoid GUILayout: Mismatched LayoutGroup.Repaint crashes on large lists
                    }
                }

                currentHeight += plugin.Height;
            }

            if (ConfigurationWindowModel.IsDebug)
            {
                GUILayout.Space(10);
                GUILayout.Label("Plugins with no options available: " + configuration.PluginsWithoutSettings);
            }
            else
            {
                // Always leave some space in case there's a dropdown box at the very bottom of the list
                GUILayout.Space(70);
            }
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            if (!DrawCurrentDropdown())
                DrawTooltip(configuration.SettingWindowRect);
        }

        private static void DrawWindowHeader(ConfigurationWindowModel configurationView)
        {
            DrawFilterLeft(configurationView);
            DrawFilterRight(configurationView);
        }

        private static void DrawFilterLeft(ConfigurationWindowModel configuration)
        {
            CMPlugin.log?.LogMessage(Event.current.type);

            var style = GUI.skin.box;
            var options = new GUILayoutOption[0];
            var layoutType = Il2CppType.Of<GUILayoutGroup>();

            EventType type = Event.current.type;
            EventType eventType = type;

            GUILayoutGroup guilayoutGroup;
            if(eventType != EventType.Layout && eventType != EventType.Used)
            {
                guilayoutGroup = GUILayoutUtility.current.topLevel.GetNext().TryCast<GUILayoutGroup>();
                if (guilayoutGroup == null)
                {
                    CMPlugin.log?.LogMessage("exception");
                    throw new System.Exception();
                }
                guilayoutGroup.ResetCursor();
            }
            else
            {
                guilayoutGroup = GUILayoutUtility.CreateGUILayoutGroupInstanceOfType(layoutType);
                guilayoutGroup.style = style;
                if (options != null)
                {
                    guilayoutGroup.ApplyOptions(options);
                }
                GUILayoutUtility.current.topLevel.Add(guilayoutGroup);
            }
            GUILayoutUtility.current.layoutGroups.Push(guilayoutGroup);
            GUILayoutUtility.current.topLevel = guilayoutGroup;

            //GUILayout.BeginHorizontal(GUI.skin.box);
            
            GUILayout.Label("Show: ", GUILayout.ExpandWidth(false));

            GUI.enabled = configuration.SearchString == string.Empty;

            var newVal = GUILayout.Toggle(CMConfig._showSettings.Value, "Normal settings");
            if (CMConfig._showSettings.Value != newVal)
            {
                CMConfig._showSettings.Value = newVal;
                configuration.Filter();
            }

            newVal = GUILayout.Toggle(CMConfig._showKeybinds.Value, "Keyboard shortcuts");
            if (CMConfig._showKeybinds.Value != newVal)
            {
                CMConfig._showKeybinds.Value = newVal;
                configuration.Filter();
            }

            var origColor = GUI.color;
            GUI.color = AdvancedSettingColor;
            newVal = GUILayout.Toggle(CMConfig._showAdvanced.Value, "Advanced settings");
            if (CMConfig._showAdvanced.Value != newVal)
            {
                CMConfig._showAdvanced.Value = newVal;
                configuration.Filter();
            }
            GUI.color = origColor;

            GUI.enabled = true;

            newVal = GUILayout.Toggle(ConfigurationWindowModel.IsDebug, "Debug mode");
            if (ConfigurationWindowModel.IsDebug != newVal)
            {
                ConfigurationWindowModel.IsDebug = newVal;
                configuration.Filter();
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawFilterRight(ConfigurationWindowModel configuration)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            GUILayout.Label("Search settings: ", GUILayout.ExpandWidth(false));

            GUI.SetNextControlName(SearchBoxName);
            configuration.SearchString = GUILayout.TextField(configuration.SearchString, GUILayout.ExpandWidth(true));

            if (configuration.FocusSearchBox)
            {
                GUI.FocusWindow(WindowId);
                GUI.FocusControl(SearchBoxName);
                configuration.FocusSearchBox = false;
            }

            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                configuration.SearchString = string.Empty;

            GUILayout.EndHorizontal();
        }

        private static void DrawTips(ConfigurationWindowModel configuration)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Tip: Click plugin names to expand. Click setting and group names to see their descriptions.");

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(CMConfig._pluginConfigCollapsedDefault.Value ? "Expand" : "Collapse", GUILayout.ExpandWidth(false)))
                {
                    var newValue = !CMConfig._pluginConfigCollapsedDefault.Value;
                    CMConfig._pluginConfigCollapsedDefault.Value = newValue;
                    foreach (var plugin in configuration.Plugins)
                        plugin.IsCollapsed = newValue;
                }
            }
            GUILayout.EndHorizontal();
        }

        private static bool DrawCurrentDropdown()
        {
            if (ComboBox.CurrentDropdownDrawer != null)
            {
                ComboBox.CurrentDropdownDrawer.Invoke();
                ComboBox.CurrentDropdownDrawer = null;
                return true;
            }
            return false;
        }

        private static void DrawTooltip(Rect area)
        {
            if (!string.IsNullOrEmpty(GUI.tooltip))
            {
                var currentEvent = Event.current;

                var style = new GUIStyle
                {
                    normal = new GUIStyleState { textColor = Color.white, background = TooltipBg },
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };

                const int width = 400;
                var height = style.CalcHeight(new GUIContent(GUI.tooltip), 400) + 10;

                var x = currentEvent.mousePosition.x + width > area.width
                    ? area.width - width
                    : currentEvent.mousePosition.x;

                var y = currentEvent.mousePosition.y + 25 + height > area.height
                    ? currentEvent.mousePosition.y - height
                    : currentEvent.mousePosition.y + 25;

                GUI.Box(new Rect(x, y, width, height), GUI.tooltip, style);
            }
        }
    }
}
