using BepInEx.Configuration;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using ConfigurationManager.Utilities;
using ConfigurationManager.Drawers;

namespace ConfigurationManager.Models
{
    public class SettingModel
    {
        // Underlying value of setting described by an instance of the view
        public readonly ConfigEntryBase Entry;

        // Window instance called for layout information
        public readonly ConfigurationWindowModel Window;

        // Display information of the view
        public readonly string Name;
        public readonly string Description;
        public readonly Type SettingType;
        public readonly object DefaultValue;
        public object Value
        {
            get => Entry.BoxedValue;
            set => Entry.BoxedValue = value;
        }

        // Type conversion information of the view
        /// <summary>
        /// Function to convert object of type '<see cref="SettingType"/>' to string
        /// </summary>
        public readonly Func<object, string>? OtoS;

        /// <summary>
        /// Function to convert string to object of type '<see cref="SettingType"/>'
        /// </summary>
        public readonly Func<string, object>? StoO;

        // Display details of the view
        public readonly bool FailedToProcess;
        public readonly bool ShowRangeAsPercent;
        public readonly (object lowerBoud, object upperBound)? AcceptableValueRange;
        public readonly object[]? AcceptableValues;

        /// <summary>
        /// When true, do not show this setting
        /// </summary>
        public bool IsFiltered { get; private set; }

        // How to draw this view
        public readonly Action<SettingModel>? DrawValueInput;

        public SettingModel(ConfigEntryBase entry, ConfigurationWindowModel window)
        {
            Window = window;
            Entry = entry;
            Name = entry.Definition.Key.TrimStart('!');
            Description = entry.Description.Description;
            SettingType = entry.SettingType;
            DefaultValue = entry.DefaultValue;

            GetAcceptableValues(
                entry.Description.AcceptableValues,
                ref AcceptableValues,
                ref AcceptableValueRange,
                ref ShowRangeAsPercent,
                ref FailedToProcess);

            var converter = TomlTypeConverter.GetConverter(SettingType);
            if (converter != null)
            {
                OtoS = o => converter.ConvertToString(o, SettingType);
                StoO = s => converter.ConvertToObject(s, SettingType);
            }

            DrawValueInput = GetFieldDrawer(ref FailedToProcess);
        }

        public bool Filter(string searchString, string parentString)
        {
            var searchStrings = searchString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (searchStrings.Any())
            {
                var stringsToSearch = string.Join(" ", Name, Description, parentString);

                IsFiltered = searchStrings.Any(s => !stringsToSearch.Contains(s, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                IsFiltered = false;
            }

            return IsFiltered;
        }

        /// <summary>
        /// Initialize acceptableValue properties
        /// </summary>
        private void GetAcceptableValues(AcceptableValueBase valueBase,
            ref object[]? acceptableValues,
            ref (object, object)? acceptableValuerange,
            ref bool showRangeAsPercent,
            ref bool failedToProcess)
        {
            var t = valueBase.GetType();
            var listProp = t.GetProperty(nameof(AcceptableValueList<bool>.AcceptableValues), BindingFlags.Instance | BindingFlags.Public);
            if (listProp != null)
            {
                acceptableValues = ((IEnumerable)listProp.GetValue(valueBase, null)).Cast<object>().ToArray();
            }
            else
            {
                var minProp = t.GetProperty(nameof(AcceptableValueRange<bool>.MinValue), BindingFlags.Instance | BindingFlags.Public);
                if (minProp == null)
                {
                    var maxProp = t.GetProperty(nameof(AcceptableValueRange<bool>.MaxValue), BindingFlags.Instance | BindingFlags.Public);
                    if (maxProp != null)
                    {
                        acceptableValuerange = (minProp.GetValue(valueBase, null), maxProp.GetValue(valueBase, null));
                        showRangeAsPercent = (acceptableValuerange.Value.Item1.Equals(0) || acceptableValuerange.Value.Item1.Equals(1)) && acceptableValuerange.Value.Item2.Equals(100) ||
                                             acceptableValuerange.Value.Item1.Equals(0f) && acceptableValuerange.Value.Item2.Equals(1f);
                    }
                    else
                    {
                        failedToProcess = true;
                    }
                }
            }
        }

        /// <summary>
        /// Select the type of Setting UI that corresponds to the setting
        /// </summary>
        private Action<SettingModel>? GetFieldDrawer(ref bool failedToProcess)
        {
            if (AcceptableValueRange != null)
            {
                return SettingDrawer.DrawRangeField;
            }
            if (AcceptableValues != null)
            {
                return SettingDrawer.DrawListField;
            }
            if (SettingType.IsEnum)
            {
                if (SettingTypeHelper.IsFlagsEnum(SettingType))
                {
                    return SettingDrawer.DrawFlagsField;
                }

                return SettingDrawer.DrawEnumComboBoxField;
            }

            if (SettingDrawer.TypeDrawers.TryGetValue(SettingType, out var drawer))
            {
                return drawer;
            }

            if (StoO != null && OtoS != null)
            {
                return SettingDrawer.DrawFieldByType;
            }

            failedToProcess = true;
            return null;
        }
    }
}
