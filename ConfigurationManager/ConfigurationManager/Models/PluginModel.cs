using BepInEx;
using BepInEx.Unity.IL2CPP;
using System.Linq;
using System;
using Version = SemanticVersioning.Version;

namespace ConfigurationManager.Models
{
    public class PluginModel
    {
        // Underlying value of setting described by an instance of the view
        public readonly PluginInfo PluginInfo;
        public readonly SectionModel[] Sections;

        // Display information of the view
        public string Name => PluginInfo.Metadata.Name;
        public string GUID => PluginInfo.Metadata.GUID;
        public Version Version => PluginInfo.Metadata.Version;

        // Display details of the view
        public int Height;

        /// <summary>
        /// When true, show this plugin minimized
        /// </summary>
        public bool IsCollapsed
        {
            get => _collapsed;
            set
            {
                _collapsed = value;
                Height = 0;
            }
        }
        private bool _collapsed;

        /// <summary>
        /// When true, do not show this plugin
        /// </summary>
        public bool IsFiltered;

        public PluginModel(PluginInfo info, ConfigurationWindowModel window)
        {
            PluginInfo = info;

            if (info.Instance is BasePlugin p)
            {
                Sections = p.Config
                    .GroupBy(kvp => kvp.Key.Section, kvp => kvp.Value)
                    .Select(g => new SectionModel(g, g.Key, window))
                    .ToArray();
            }
            else
            {
                Sections = Array.Empty<SectionModel>();
            }
        }

        public void Filter(string searchString)
        {
            var parentString = string.Join(" ", Name, GUID);
            IsFiltered = Sections.Select(s => s.Filter(searchString, parentString))
                .All(f => f);
        }
    }
}
