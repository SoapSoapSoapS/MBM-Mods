using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HarmonyLib.Tools;
using MBMScripts;
using UnityEngine;
using Action = System.Action;

namespace MbmModdingTools
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class ToolsPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(MbmModdingTools),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        /// <summary>
        /// Mod log instance
        /// </summary>
        public static BepInEx.Logging.ManualLogSource? log;

        /// <summary>
        /// List of all female objects (includes store and npc)
        /// </summary>
        public static IDictionary<int, Female> Females = new Dictionary<int, Female>();

        /// <summary>
        /// The GameManager instance
        /// </summary>
        public static GameManager? GM = null;

        /// <summary>
        /// The Playdata Instance
        /// </summary>
        public static PlayData? PD
        {
            get => GM?.m_PlayData;
        }

        /// <summary>
        /// List of actions to be run periodically
        /// </summary>
        private static readonly IList<PeriodicAction> PeriodicActions = new List<PeriodicAction>();

        public ToolsPlugin()
        {
            log = Log;
        }

        /// <summary>
        /// Collect instances of female object
        /// </summary>
        [HarmonyPatch(typeof(Female), nameof(Female.UpdatePixy))]
        [HarmonyPostfix]
        public static void FemaleCollector(Female __instance)
        {
            Females.TryAdd(__instance.UnitId, __instance);
        }

        /// <summary>
        /// Collect GameManager instance
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.UpdateCursor))]
        [HarmonyPostfix]
        public static void GetManager(GameManager __instance)
        {
            GM ??= __instance;
        }

        /// <summary>
        /// Clear list of females when new save is loaded.
        /// </summary>
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Load))]
        [HarmonyPrefix]
        public static void BeforeLoad()
        {
            Females.Clear();
        }

        /// <summary>
        /// Run Periodic Actions
        /// </summary>
        /// <param name="deltaTime"></param>
        [HarmonyPatch(typeof(PlayData), nameof(PlayData.Update))]
        [HarmonyPostfix]
        public static void OnUpdate(float deltaTime)
        {
            foreach(var action in PeriodicActions)
            {
                action.timeSinceRun += deltaTime;
                if(action.timeSinceRun > action.period)
                {
                    action.act();
                }
            }
        }

        /// <summary>
        /// Patch and start plugin.
        /// </summary>
        public override void Load()
        {
            try
            {
                log?.LogMessage("Starting Harmony Patch");
                HarmonyFileLog.Enabled = true;
                var harmony = new Harmony(GUID);
                harmony.PatchAll(typeof(ToolsPlugin));

                log?.LogMessage("Harmony Patch Successful");
            }
            catch
            {
                log?.LogMessage("Harmony Patch Failed");
            }
        }

        /// <summary>
        /// Registers an action to run approximatley every "period" seconds.
        /// </summary>
        /// <param name="period">dfd</param>
        /// <param name="act"><sdf/param>
        /// <returns></returns>
        public static PeriodicAction RegisterPeriodicAction(float period, Action act)
        {
            var pact = new PeriodicAction
            {
                id = Guid.NewGuid(),
                timeSinceRun = 0,
                period = period,
                act = act
            };

            PeriodicActions.Add(pact);

            return pact;
        }
    }

    public class PeriodicAction
    {
        public Guid id;
        public float timeSinceRun;
        public float period;
        public Action act;
    }
}
