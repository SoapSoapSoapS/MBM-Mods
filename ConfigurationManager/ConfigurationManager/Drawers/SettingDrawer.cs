using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using UnityEngine;
using ConfigurationManager.Utilities;
using ConfigurationManager.Models;

namespace ConfigurationManager.Drawers
{
    public static class SettingDrawer
    {
        public static IDictionary<Type, Action<SettingModel>> TypeDrawers = new Dictionary<Type, Action<SettingModel>>
        {
            {typeof(bool), DrawBoolField},
            {typeof(Color), DrawColor },
            {typeof(Vector2), DrawVector2 },
            {typeof(Vector3), DrawVector3 },
            {typeof(Vector4), DrawVector4 },
            {typeof(Quaternion), DrawQuaternion },
        };

        private static readonly Dictionary<ConfigDefinition, ComboBox> _comboBoxCache = new Dictionary<ConfigDefinition, ComboBox>();
        private static IDictionary<ConfigDefinition, Texture2D> _colorCache = new Dictionary<ConfigDefinition, Texture2D>();

        public static void ClearCache()
        {
            _comboBoxCache.Clear();

            foreach (var tex in _colorCache)
                UnityEngine.Object.Destroy(tex.Value);
            _colorCache.Clear();
        }

        public static void Draw(SettingModel setting)
        {
            if (setting.IsFiltered)
                return;

            GUILayout.BeginHorizontal();
            try
            {
                DrawSettingName(setting);
                DrawSettingValue(setting);
                DrawDefaultButton(setting);
            }
            catch (Exception ex)
            {
                var message = $"Failed to draw setting {setting.Name} - {ex}";
                CMPlugin.log?.LogError(message);
                GUILayout.Label("Failed to draw this field, check log for details.");
            }
        }

        private static void DrawSettingName(SettingModel setting)
        {
            GUILayout.Label(
                new GUIContent
                {
                    text = setting.Name,
                    tooltip = setting.Description
                },
                GUILayout.Width(setting.Window.LeftColumnWidth),
                GUILayout.MaxWidth(setting.Window.LeftColumnWidth));
        }

        private static void DrawDefaultButton(SettingModel setting)
        {
            bool DefaultButton()
            {
                GUILayout.Space(5);
                return GUILayout.Button("Reset", GUILayout.ExpandWidth(false));
            }

            if (setting.DefaultValue != null)
            {
                if (DefaultButton())
                    setting.Value = setting.DefaultValue;
            }
        }

        #region Setting type specific drawers

        public static void DrawSettingValue(SettingModel setting)
        {
            if (setting.DrawValueInput == null)
                throw new ArgumentException("Failed to generate field");

            setting.DrawValueInput(setting);
        }

        public static void DrawRangeField(SettingModel setting)
        {
            var value = setting.Value;
            if (setting.AcceptableValueRange == null)
                throw new ArgumentException("AcceptableValueRange is missing");
            var acceptableValueRange = setting.AcceptableValueRange.Value;

            var converted = (float)Convert.ToDouble(value, CultureInfo.InvariantCulture);
            var leftValue = (float)Convert.ToDouble(acceptableValueRange.lowerBoud, CultureInfo.InvariantCulture);
            var rightValue = (float)Convert.ToDouble(acceptableValueRange.upperBound, CultureInfo.InvariantCulture);

            var result = GUILayout.HorizontalSlider(converted, leftValue, rightValue, GUILayout.ExpandWidth(true));
            if (Math.Abs(result - converted) > Mathf.Abs(rightValue - leftValue) / 1000)
            {
                var newValue = Convert.ChangeType(result, setting.SettingType, CultureInfo.InvariantCulture);
                setting.Value = newValue;
            }

            if (setting.ShowRangeAsPercent == true)
            {
                DrawCenteredLabel(
                    Mathf.Round(100 * Mathf.Abs(result - leftValue) / Mathf.Abs(rightValue - leftValue)) + "%",
                    GUILayout.Width(50));
            }
            else
            {
                var strVal = value.ToString().AppendZeroIfFloat(setting.SettingType);
                var strResult = GUILayout.TextField(strVal, GUILayout.Width(50));
                if (strResult != strVal)
                {
                    try
                    {
                        var resultVal = (float)Convert.ToDouble(strResult, CultureInfo.InvariantCulture);
                        var clampedResultVal = Mathf.Clamp(resultVal, leftValue, rightValue);
                        setting.Value = Convert.ChangeType(clampedResultVal, setting.SettingType, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        // Ignore user typing in bad data
                    }
                }
            }
        }

        public static void DrawCenteredLabel(string text, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            GUILayout.FlexibleSpace();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static void DrawListField(SettingModel setting)
        {
            var acceptableValues = setting.AcceptableValues;
            if (acceptableValues == null)
                throw new ArgumentException("AcceptableValueListAttribute is missing");

            if (acceptableValues.Length == 0)
                throw new ArgumentException("AcceptableValueListAttribute returned an empty list of acceptable values. You need to supply at least 1 option.");

            if (!setting.SettingType.IsInstanceOfType(acceptableValues.FirstOrDefault(x => x != null)))
                throw new ArgumentException("AcceptableValueListAttribute returned a list with items of type other than the settng type itself.");

            DrawComboBoxField(setting, acceptableValues, setting.Window.SettingWindowRect.yMax);
        }

        public static void DrawFlagsField(SettingModel setting)
        {
            var currentValue = Convert.ToInt64(setting.Value);
            var enumValues = Enum.GetValues(setting.SettingType);

            var allValues = enumValues.Cast<Enum>().Select(x => new { name = x.ToString(), val = Convert.ToInt64(x) }).ToArray();

            // Vertically stack Horizontal groups of the options to deal with the options taking more width than is available in the window
            GUILayout.BeginVertical(GUILayout.MaxWidth(setting.Window.RightColumnWidth));
            {
                for (var index = 0; index < allValues.Length;)
                {
                    GUILayout.BeginHorizontal();
                    {
                        var currentWidth = 0;
                        for (; index < allValues.Length; index++)
                        {
                            var value = allValues[index];

                            // Skip the 0 / none enum value, just uncheck everything to get 0
                            if (value.val != 0)
                            {
                                // Make sure this horizontal group doesn't extend over window width, if it does then start a new horiz group below
                                var textDimension = (int)GUI.skin.toggle.CalcSize(new GUIContent(value.name)).x;
                                currentWidth += textDimension;
                                if (currentWidth > setting.Window.RightColumnWidth)
                                    break;

                                GUI.changed = false;
                                var newVal = GUILayout.Toggle((currentValue & value.val) == value.val, value.name,
                                    GUILayout.ExpandWidth(false));
                                if (GUI.changed)
                                {
                                    var newValue = newVal ? currentValue | value.val : currentValue & ~value.val;
                                    setting.Value = Enum.ToObject(setting.SettingType, newValue);
                                }
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUI.changed = false;
            }
            GUILayout.EndVertical();
            // Make sure the reset button is properly spaced
            GUILayout.FlexibleSpace();
        }

        public static void DrawEnumComboBoxField(SettingModel setting)
        {
            DrawComboBoxField(setting, Enum.GetValues(setting.SettingType).Cast<object>().ToList(), setting.Window.SettingWindowRect.yMax);
        }

        public static void DrawComboBoxField(SettingModel setting, IList<object> list, float windowYmax)
        {
            var buttonText = ObjectToGuiContent(setting.Value);
            var dispRect = GUILayoutUtility.GetRect(buttonText, GUI.skin.button, GUILayout.ExpandWidth(true));

            if (!_comboBoxCache.TryGetValue(setting.Entry.Definition, out var box))
            {
                box = new ComboBox(dispRect, buttonText, list.Select(ObjectToGuiContent).ToArray(), GUI.skin.button, windowYmax);
                _comboBoxCache[setting.Entry.Definition] = box;
            }
            else
            {
                box.Rect = dispRect;
                box.ButtonContent = buttonText;
            }

            box.Show(id =>
            {
                if (id >= 0 && id < list.Count)
                    setting.Value = list[id];
            });
        }

        private static GUIContent ObjectToGuiContent(object obj)
        {
            if (obj is Enum)
            {
                var enumType = obj.GetType();
                var enumMember = enumType.GetMember(obj.ToString()).FirstOrDefault();
                var attr = enumMember?.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().FirstOrDefault();
                if (attr != null)
                    return new GUIContent(attr.Description);
                return new GUIContent(obj.ToString().ToProperCase());
            }
            return new GUIContent(obj.ToString());
        }

        public static void DrawFieldByType(SettingModel setting)
        {
            if (setting.OtoS != null && setting.StoO != null)
            {
                var text = setting.OtoS(setting.Value).AppendZeroIfFloat(setting.SettingType);
                var result = GUILayout.TextField(text,
                    GUILayout.Width(setting.Window.RightColumnWidth),
                    GUILayout.MaxWidth(setting.Window.RightColumnWidth));

                if (result != text) setting.Value = setting.StoO(result);
            }

            // When using MaxWidth the width will always be less than full window size
            // Use this to fill this gap and push the Reset button to the right edge
            GUILayout.FlexibleSpace();
        }

        public static void DrawBoolField(SettingModel setting)
        {
            var boolVal = (bool)setting.Value;
            var result = GUILayout.Toggle(boolVal, boolVal ? "Enabled" : "Disabled", GUILayout.ExpandWidth(true));
            if (result != boolVal)
                setting.Value = result;
        }

        public static void DrawVector2(SettingModel setting)
        {
            var value = (Vector2)setting.Value;
            var copy = value;
            value.x = DrawSingleVectorSlider(value.x, "X");
            value.y = DrawSingleVectorSlider(value.y, "Y");
            if (value != copy) setting.Value = value;
        }

        public static void DrawVector3(SettingModel setting)
        {
            var value = (Vector3)setting.Value;
            var copy = value;
            value.x = DrawSingleVectorSlider(value.x, "X");
            value.y = DrawSingleVectorSlider(value.y, "Y");
            value.z = DrawSingleVectorSlider(value.z, "Z");
            if (value != copy) setting.Value = value;
        }

        public static void DrawVector4(SettingModel setting)
        {
            var value = (Vector4)setting.Value;
            var copy = value;
            value.x = DrawSingleVectorSlider(value.x, "X");
            value.y = DrawSingleVectorSlider(value.y, "Y");
            value.z = DrawSingleVectorSlider(value.z, "Z");
            value.w = DrawSingleVectorSlider(value.w, "W");
            if (value != copy) setting.Value = value;
        }

        public static void DrawQuaternion(SettingModel setting)
        {
            var value = (Quaternion)setting.Value;
            var copy = value;
            value.x = DrawSingleVectorSlider(value.x, "X");
            value.y = DrawSingleVectorSlider(value.y, "Y");
            value.z = DrawSingleVectorSlider(value.z, "Z");
            value.w = DrawSingleVectorSlider(value.w, "W");
            if (value != copy) setting.Value = value;
        }

        private static float DrawSingleVectorSlider(float setting, string label)
        {
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(setting.ToString("F", CultureInfo.InvariantCulture), GUILayout.ExpandWidth(true)), NumberStyles.Any, CultureInfo.InvariantCulture, out var x);
            return x;
        }

        public static void DrawColor(SettingModel setting)
        {
            var value = (Color)setting.Value;
            var copy = value;

            if (!_colorCache.TryGetValue(setting.Entry.Definition, out var texture))
            {
                texture = new Texture2D(40, 10, TextureFormat.ARGB32, false);
                texture.FillTexture(value);

                _colorCache[setting.Entry.Definition] = texture;
            }

            GUILayout.Label("R", GUILayout.ExpandWidth(false));
            value.r = GUILayout.HorizontalSlider(value.r, 0f, 1f, GUILayout.ExpandWidth(true));
            GUILayout.Label("G", GUILayout.ExpandWidth(false));
            value.g = GUILayout.HorizontalSlider(value.g, 0f, 1f, GUILayout.ExpandWidth(true));
            GUILayout.Label("B", GUILayout.ExpandWidth(false));
            value.b = GUILayout.HorizontalSlider(value.b, 0f, 1f, GUILayout.ExpandWidth(true));
            GUILayout.Label("A", GUILayout.ExpandWidth(false));
            value.a = GUILayout.HorizontalSlider(value.a, 0f, 1f, GUILayout.ExpandWidth(true));

            GUILayout.Space(4);

            if (value != copy)
            {
                setting.Value = value;
                texture.FillTexture(value);
            }

            GUILayout.Label(texture, GUILayout.ExpandWidth(false));
        }

        #endregion
    }
}
