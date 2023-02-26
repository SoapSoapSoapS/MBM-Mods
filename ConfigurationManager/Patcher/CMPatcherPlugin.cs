using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HarmonyLib.Tools;
using UnityEngine;

namespace CMPatcher
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class CMPatcherPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(CMPatcherPlugin),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        public static ManualLogSource Logger;

        public CMPatcherPlugin()
        {
            Logger = Log;
        }

        //[HarmonyPatch(typeof(GUILayoutGroup), nameof(GUILayoutGroup.Add))]
        //[HarmonyPrefix]
        public static bool AddPrefix(GUILayoutEntry e, GUILayoutGroup __instance)
        {
            __instance.entries.Add(e);
            Logger.LogMessage("hello");
            return false;
        }

        public override void Load()
        {
            HarmonyFileLog.Enabled = true;
            var harmony = new Harmony(GUID);

            harmony.PatchAll(typeof(CMPatcherPlugin));
        }
    }
}
