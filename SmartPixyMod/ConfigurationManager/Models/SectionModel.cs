using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace SBH.ConfigurationManager.Models
{
    public class SectionModel
    {
        // Underlying value of setting described by an instance of the view
        public readonly SettingModel[] Settings;
        public readonly string SectionName;

        /// <summary>
        /// When true, do not show this section
        /// </summary>
        public bool IsFiltered { get; private set; }

        public SectionModel(IEnumerable<ConfigEntryBase> entries, string sectionName, ConfigurationWindowModel window)
        {
            SectionName = sectionName;
            Settings = entries.Select(e => new SettingModel(e, window)).ToArray();
        }

        public bool Filter(string searchString, string parentString)
        {
            parentString = string.Join(" ", SectionName, parentString);
            IsFiltered = Settings.Select(s => s.Filter(searchString, parentString))
                .All(f => f);
            return IsFiltered;
        }
    }
}
