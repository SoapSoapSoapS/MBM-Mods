// Rewrite of https://github.com/BepInEx/BepInEx.ConfigurationManager for Bepinex 6 Il2Cpp

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using System;

namespace ConfigurationManager
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class CMPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(ConfigurationManager),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        public static ManualLogSource? log;
        public static ConfigFile? config;
        public static bool once = false;

        public CMPlugin()
        {
            log = Log;
            config = Config;
            CMConfig.InitializeConfigs(config);
        }

        public override void Load()
        {
            try
            {
                // Register our custom Types in Il2Cpp
                ClassInjector.RegisterTypeInIl2Cpp<ConfigurationWindowManager>();
            }
            catch (Exception e)
            {
                log?.LogError("FAILED to Register Il2Cpp Type!");
                log?.LogError(e);
            }
        }
    }
}
