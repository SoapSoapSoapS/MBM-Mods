using ConfigurationManager.Models;
using UnityEngine;

namespace ConfigurationManager.Drawers
{
    public static class SectionDrawer
    {
        private static GUIStyle CategoryHeaderSkin = new GUIStyle(GUI.skin.label.Pointer)
        {
            alignment = TextAnchor.UpperCenter,
            wordWrap = true,
            stretchWidth = true,
            fontSize = 14
        };

        public static void DrawSection(SectionModel section)
        {
            if (section.IsFiltered)
                return;

            GUILayout.Label(section.SectionName, CategoryHeaderSkin);
            foreach (var settingView in section.Settings)
            {
                SettingDrawer.DrawSettingValue(settingView);
                GUILayout.Space(2);
            }
        }
    }
}
