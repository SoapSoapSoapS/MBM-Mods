using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using HarmonyLib.Tools;
using System;

namespace SBH.TestPlugin
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class TestPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(TestPlugin),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";
        
        public static BepInEx.Logging.ManualLogSource log;
        public static bool once = false;

        public TestPlugin()
        {
            log = Log;
        }

        [HarmonyPatch(typeof(MBMScripts.PlayerController), nameof(MBMScripts.PlayerController.Start))]
        [HarmonyPostfix]
        public static void PostFix(MBMScripts.MovePixy __instance)
        {
            foreach (var an in AppDomain.CurrentDomain.GetAssemblies())
            {
                log.LogMessage(an.FullName);
            }
        }

        public override void Load()
        {
            try
            {
                log.LogMessage("Starting Harmony Patch");

                HarmonyFileLog.Enabled = true;
                var harmony = new Harmony(GUID);
                harmony.PatchAll(typeof(TestPlugin));

                log.LogMessage("Harmony Patch Successful");
            }
            catch
            {
                log.LogMessage("Harmony Patch Failed");
            }
        }
    }
}
