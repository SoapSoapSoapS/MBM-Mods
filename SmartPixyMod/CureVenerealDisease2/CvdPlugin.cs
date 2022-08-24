using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using HarmonyLib.Tools;

namespace SBH.CureVenerealDisease
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class CvdPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(CureVenerealDisease),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";
        
        public static BepInEx.Logging.ManualLogSource log;

        public CvdPlugin()
        {
            log = Log;
        }

        [HarmonyPatch(typeof(MBMScripts.Character), nameof(MBMScripts.Character.UpdateState))]
        [HarmonyPostfix]
        public static void PostFix(MBMScripts.Character __instance)
        {
            if (__instance.VenerealDisease)
            {
                log.LogMessage("Curing venereal disease");

                __instance.VenerealDisease = false;
            }
        }

        public override void Load()
        {
            try
            {
                log.LogMessage("Starting Harmony Patch");

                HarmonyFileLog.Enabled = true;
                var harmony = new Harmony(GUID);
                harmony.PatchAll(typeof(CvdPlugin));

                log.LogMessage("Harmony Patch Successful");
            }
            catch
            {
                log.LogMessage("Harmony Patch Failed");
            }
        }
    }
}
