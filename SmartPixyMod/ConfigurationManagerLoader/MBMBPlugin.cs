using BepInEx;
using BepInEx.IL2CPP;
using UnhollowerRuntimeLib;
using HarmonyLib;
using HarmonyLib.Tools;
using System;
using BepInEx.Logging;

namespace SBH.MBMBootstrapperPlugin
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class MBMBPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(MBMBootstrapperPlugin),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        public static ManualLogSource? log;
        public static bool once = false;

        public MBMBPlugin()
        {
            log = Log;
        }

        public override void Load()
        {
            log?.LogMessage("Registering ConfigurationManager in Il2Cpp");

            try
            {
                log?.LogMessage("Registering Il2Cpp Types!");
                // Register our custom Types in Il2Cpp
                ClassInjector.RegisterTypeInIl2Cpp<Bootstrapper>();
            }
            catch(Exception e)
            {
                log?.LogError("FAILED to Register Il2Cpp Type!");
                log?.LogError(e);
            }

            try
            {
                log?.LogMessage("Applying Hooks!");

                HarmonyFileLog.Enabled = true;
                var harmony = new Harmony(GUID);

                harmony.PatchAll(typeof(Bootstrapper));
            }
            catch { log?.LogError("FAILED to Apply Hooks!"); }

        }
    }
}
