using System;
using System.Linq;

namespace ConfigurationManager.Utilities
{
    public static class SettingTypeHelper
    {
        public static bool IsFlagsEnum(Type t)
        {
            return t.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
        }
    }
}
