using System;
using System.Linq;
using UnityEngine;

namespace SBH.ConfigurationManager.Utilities
{
    public static class Extensions
    {
        public static void FillTexture(this Texture2D tex, Color color)
        {
            var colorArray = Enumerable.Repeat(color, tex.width*tex.height).ToArray();
            tex.SetPixels(colorArray);

            tex.Apply(false);
        }

        public static string AppendZeroIfFloat(this string s, Type type)
        {
            return type == typeof(float) || type == typeof(double) || type == typeof(decimal) ? s.AppendZero() : s;
        }

        public static string AppendZero(this string s)
        {
            return !s.Contains(".") ? s + ".0" : s;
        }

        public static string ToProperCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            if (str.Length < 2) return str;

            // Start with the first character.
            string result = str.Substring(0, 1).ToUpper();

            // Add the remaining characters.
            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i])) result += " ";
                result += str[i];
            }

            return result;
        }
    }
}
