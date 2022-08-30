using BepInEx.Configuration;

namespace PixyPlus
{
    public class Config
    {
        public readonly ConfigEntry<bool> AutoRestFromBreedingRoomEnabled;
        public readonly ConfigEntry<bool> AutoRestEnabled;

        public Config(ConfigFile file)
        {
            var setting1 = new ConfigInfo<bool>()
            {
                Section = "AutoRestFromBreedingRoom",
                Name = "Enabled",
                Description = "If enabled, will force low health event to trick pixy into moving the unit to a cage",
                DefaultValue = true
            };
            AutoRestFromBreedingRoomEnabled = setting1.Bind(file);

            var setting2 = new ConfigInfo<bool>()
            {
                Section = "AutoRest",
                Name = "Enabled",
                Description = "If enabled, will move units to cage after milking and birth",
                DefaultValue = true
            };
            AutoRestEnabled = setting2.Bind(file);
        }
    }

    public struct ConfigInfo<T>
    {
        public string Section;
        public string Name;
        public string Description;
        public T DefaultValue;
        public AcceptableValueBase? AcceptableValues;

        public ConfigEntry<T> Bind(ConfigFile file)
        {
            if(AcceptableValues == null)
            {
                return file.Bind(new ConfigDefinition(Section, Name), DefaultValue, new ConfigDescription(Description));
            }
            return file.Bind(new ConfigDefinition(Section, Name), DefaultValue, new ConfigDescription(Description, AcceptableValues));
        }
    }
}
