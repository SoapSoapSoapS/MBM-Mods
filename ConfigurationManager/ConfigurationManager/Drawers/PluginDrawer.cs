using ConfigurationManager;
using ConfigurationManager.Models;
using UnityEngine;

namespace ConfigurationManager.Drawers
{
    public static class PluginDrawer
    {
        private static GUIStyle _pluginHeaderSkin = new GUIStyle(GUI.skin.label.Pointer)
        {
            alignment = TextAnchor.UpperCenter,
            wordWrap = true,
            stretchWidth = true,
            fontSize = 15
        };

        public static void DrawPlugin(PluginModel plugin, bool isSearching)
        {
            if (plugin.IsFiltered)
                return;

            GUILayout.BeginVertical(GUI.skin.box);

            var categoryHeader = new GUIContent($"{plugin.Name} {plugin.Version}");

            if (ConfigurationWindowModel.IsDebug)
                categoryHeader.tooltip = "GUID: " + plugin.GUID;

            if (DrawPluginHeader(categoryHeader, plugin.IsCollapsed && !isSearching) && !isSearching)
                plugin.IsCollapsed = !plugin.IsCollapsed;

            if (isSearching || !plugin.IsCollapsed)
            {
                foreach (var section in plugin.Sections)
                {
                    if (!string.IsNullOrEmpty(section.SectionName))
                    {
                        if (plugin.Sections.Length > 1 || !CMConfig.HideSingleSection.Value)
                            SectionDrawer.DrawSection(section);
                    }
                }
            }

            GUILayout.EndVertical();
        }

        public static bool DrawPluginHeader(GUIContent content, bool isCollapsed)
        {
            if (isCollapsed)
                content.text += "\n...";

            return GUILayout.Button(content, _pluginHeaderSkin, GUILayout.ExpandWidth(true));
        }
    }
}
