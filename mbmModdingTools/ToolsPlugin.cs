using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HarmonyLib.Tools;
using MBMScripts;

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
        private static readonly IDictionary<float, PeriodicActionGroup> PeriodicActionGroups = new Dictionary<float, PeriodicActionGroup>();

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
            foreach(var kvp in PeriodicActionGroups)
            {
                var pag = kvp.Value;
                pag.timeSinceRun += deltaTime;
                if(pag.timeSinceRun > pag.period)
                {
                    pag.Act();
                    pag.timeSinceRun = 0;
                }
            }
        }

        public ToolsPlugin()
        {
            log = Log;
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
                harmony.PatchAll(typeof(ToolsPlugin));

                Log.LogMessage("Harmony Patch Successful");
            }
            catch
            {
                Log.LogWarning("Harmony Patch Failed");
            }
        }

        /// <summary>
        /// Registers an action to run approximatley every "period" seconds.
        /// </summary>
        public static PeriodicAction RegisterPeriodicAction(float period, Action act)
        {
            var paction = new PeriodicAction(act);

            if(PeriodicActionGroups.TryGetValue(period, out var pag))
            {
                pag.actions.Add(paction);
            }
            else
            {
                PeriodicActionGroups.Add(period, new PeriodicActionGroup(period, paction));
            }

            return paction;
        }

        /// <summary>
        /// Get only females who the player owns.
        /// </summary>
        public static IEnumerable<Female> GetOwnedFemales()
        {
            if (PD == null)
                yield break;

            var units = PD.GetUnitList(ESector.Female);
            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                if (!Females.TryGetValue(unit.UnitId, out var female))
                    continue;

                yield return female;
            }
        }
    }

    public class PeriodicAction
    {
        public Guid id;
        public Action act;

        public PeriodicAction(Action act)
        {
            id = Guid.NewGuid();
            this.act = act;
        }
    }

    public class PeriodicActionGroup
    {
        public Guid id;
        public float timeSinceRun;
        public float period;
        public IList<PeriodicAction> actions = new List<PeriodicAction>();

        public PeriodicActionGroup(float period, PeriodicAction act)
        {
            timeSinceRun = 0;
            this.period = period;
            actions.Add(act);
        }

        public void Act()
        {
            foreach(var action in actions)
            {
                action.act();
            }
        }
    }
}
