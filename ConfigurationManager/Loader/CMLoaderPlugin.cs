using BepInEx.Unity.IL2CPP;
using BepInEx;
using Il2CppInterop.Runtime.Injection;
using System;
using HarmonyLib;
using HarmonyLib.Tools;

namespace CMLoader
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class CMLoaderPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(CMLoader),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        public static BepInEx.Logging.ManualLogSource log;

        public CMLoaderPlugin()
        {
            log = Log;
        }

        public override void Load()
        {
            try
            {
                Log.LogMessage("Registering Il2Cpp Types!");
                // Register our custom Types in Il2Cpp
                ClassInjector.RegisterTypeInIl2Cpp<Bootstrapper>();
            }
            catch (Exception e)
            {
                Log.LogError("FAILED to Register Il2Cpp Type!");
                Log.LogError(e);
            }

            try
            {
                Log.LogMessage("Applying Hooks!");

                HarmonyFileLog.Enabled = true;
                var harmony = new Harmony(GUID);

                harmony.PatchAll(typeof(Bootstrapper));
            }
            catch (Exception e)
            {
                Log.LogError("FAILED to Apply Hooks!");
                Log.LogError(e);
            }

        }
    }
}
