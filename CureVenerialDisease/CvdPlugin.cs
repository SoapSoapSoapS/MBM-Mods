using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using MbmModdingTools;
using MBMScripts;

namespace CureVenerialDisease
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency(ToolsPlugin.GUID)]
    public class CvdPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(CureVenerealDisease),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "2.0.0.0";

        /// <summary>
        /// Mod log instance
        /// </summary>
        public static BepInEx.Logging.ManualLogSource? log;

        /// <inheritdoc cref="ToolsPlugin.Females"/>
        public static IDictionary<int, Female> Females
        {
            get => ToolsPlugin.Females;
        }

        /// <inheritdoc cref="ToolsPlugin.GM"/>
        public static GameManager? GM
        {
            get => ToolsPlugin.GM;
        }

        /// <inheritdoc cref="ToolsPlugin.PD"/>
        public static PlayData? PD
        {
            get => ToolsPlugin.PD;
        }

        public ConfigEntry<string> ExcludePhrase;

        public CvdPlugin()
        {
            log = Log;
            ExcludePhrase = Config.Bind(
                new ConfigDefinition("General", "ExcludePhrase"),
                "cvdexclude",
                new ConfigDescription("If this exact phrase is in a female's name then that unit will not be cured."));
        }

        /// <summary>
        /// Register plugin to run.
        /// </summary>
        public override void Load()
        {
            Log.LogMessage("Registering");
            ToolsPlugin.RegisterPeriodicAction(1, Run);
        }

        /// <summary>
        /// Runs plugin on all owned females.
        /// </summary>
        public void Run()
        {
            var ownedFemales = ToolsPlugin.GetOwnedFemales();
            foreach (var female in ownedFemales)
            {
                CVD(female);
            }
        }

        /// <summary>
        /// Cure the target of venereal disease.
        /// </summary>
        public void CVD(Female female)
        {
            if (female.VenerealDisease && !female.DisplayName.Contains(ExcludePhrase.Value))
            {
                Log.LogDebug("Curing Venereal disease");
                female.VenerealDisease = false;
            }
        }
    }
}
