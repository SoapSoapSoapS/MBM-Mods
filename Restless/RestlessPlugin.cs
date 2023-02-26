using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib.Tools;
using HarmonyLib;
using MBMScripts;
using MbmModdingTools;
using BepInEx.Configuration;

namespace Restless
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency(ToolsPlugin.GUID)]
    public class RestlessPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(Restless),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        /// <summary>
        /// Mod log instance
        /// </summary>
        public static BepInEx.Logging.ManualLogSource? log;

        /// <summary>
        /// Rest time.
        /// </summary>
        public ConfigEntry<float> RestTime;

        /// <summary>
        /// Initialize logger.
        /// </summary>
        public RestlessPlugin()
        {
            log = Log;
            RestTime = Config.Bind(new ConfigInfo<float>()
            {
                Section = "General",
                Name = "RestTime",
                Description = "The time that a unit will rest before starting a new activity",
                AcceptableValues = new AcceptableValueRange<float>(1f, 64f),
                DefaultValue = 5
            });
        }

        /// <summary>
        /// Patch and start plugin.
        /// </summary>
        public override void Load()
        {
            try
            {
                Log.LogMessage("Starting Harmony Patch");
                HarmonyFileLog.Enabled = true;
                var harmony = new Harmony(GUID);
                harmony.PatchAll(typeof(RestlessPlugin));

                Log.LogMessage("Harmony Patch Successful");
            }
            catch
            {
                Log.LogWarning("Harmony Patch Failed");
            }

            ToolsPlugin.RegisterPeriodicAction(1, Run);
        }

        public void Run()
        {
            if(GameManager.ConfigData != null)
            {
                GameManager.ConfigData.m_RestTime = RestTime.Value;
            }
        }
    }
}
