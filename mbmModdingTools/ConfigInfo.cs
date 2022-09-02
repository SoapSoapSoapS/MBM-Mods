using BepInEx.Configuration;

namespace MbmModdingTools
{
    public struct ConfigInfo<T>
    {
        public string Section;
        public string Name;
        public string Description;
        public T DefaultValue;
        public AcceptableValueBase? AcceptableValues;
    }

    public static class ConfigExtensions
    {
        public static ConfigEntry<T> Bind<T>(this ConfigFile file, ConfigInfo<T> info)
        {
            return file.Bind(new ConfigDefinition(info.Section, info.Name), info.DefaultValue, new ConfigDescription(info.Description, info.AcceptableValues));
        }
    }

}
