// Rewrite of https://github.com/BepInEx/BepInEx.ConfigurationManager for Bepinex 6 Il2Cpp

using BepInEx;
using BepInEx.IL2CPP;
using UnhollowerRuntimeLib;
using System;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace SBH.ConfigurationManager
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class CMPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(SBH.ConfigurationManager),
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
            log?.LogMessage("Registering ConfigurationManager in Il2Cpp");

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
