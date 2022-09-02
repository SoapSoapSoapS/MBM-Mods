using BepInEx.Configuration;
using MbmModdingTools;

namespace HealthPixyPlus
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
            AutoRestFromBreedingRoomEnabled = file.Bind(setting1);

            var setting2 = new ConfigInfo<bool>()
            {
                Section = "AutoRest",
                Name = "Enabled",
                Description = "If enabled, will move units to cage after milking and birth",
                DefaultValue = true
            };
            AutoRestEnabled = file.Bind(setting2);
        }
    }
}
