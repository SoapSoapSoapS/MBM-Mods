using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MbmModdingTools;
using MBMScripts;

namespace PixyPlus
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class PixyPlusPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(PixyPlus),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        /// <summary>
        /// Mod log instance
        /// </summary>
        public static BepInEx.Logging.ManualLogSource? log;

        /// <inheritdoc cref="ToolsPlugin.Females"/>
        public static IDictionary<int, Female> Females
        {
            get => ToolsPlugin.Females;
        }

        /// <inheritdoc cref="ToolsPlugin.GM"/>
        public static GameManager? GM
        {
            get => ToolsPlugin.GM;
        }

        /// <inheritdoc cref="ToolsPlugin.PD"/>
        public static PlayData? PD
        {
            get => ToolsPlugin.PD;
        }

        /// <summary>
        /// Stores return location for females sent to rest in cage (Override).
        /// </summary>
        public static IDictionary<int, (int roomid, int seat)> restingReturnOverrides = new Dictionary<int, (int roomid, int seat)>();

        /// <summary>
        /// Stores return location for females to block them from returning before the mod sees them (Blocking).
        /// </summary>
        public static IDictionary<int, (int roomid, int seat)> restingReturnBlocks = new Dictionary<int, (int roomid, int seat)>();

        /// <summary>
        /// State when return location should be blocked
        /// </summary>
        public static IList<EState> PreReadyToRestStates = new List<EState>
        {
            EState.Birth,
            EState.BirthDrained,
            EState.MilkStart
        };

        /// <summary>
        /// State when return location should be overriden.
        /// </summary>
        public static IList<EState> ReadyToRestStates = new List<EState>
        {
            EState.AfterBirth,
            EState.AfterBirthDrained,
            EState.MilkIdle
        };

        /// <summary>
        /// Placeholder to denote that a character was already processed and had no return location
        /// </summary>
        public static (int, int) NoReturnTuple => (-1, -1);

        /// <summary>
        /// Clear unit return state on drag start.
        /// </summary>
        [HarmonyPatch(typeof(Unit), nameof(Unit.IsDragging), MethodType.Setter)]
        [HarmonyPostfix]
        public static void OnDragUnit(Unit __instance)
        {
            if (__instance.IsDragging)
            {
                restingReturnBlocks.Remove(__instance.UnitId);
                restingReturnOverrides.Remove(__instance.UnitId);
            }
        }

        /// <summary>
        /// Clear all state on load.
        /// </summary>
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Load))]
        [HarmonyPrefix]
        public static void BeforeLoad()
        {
            restingReturnBlocks.Clear();
            restingReturnOverrides.Clear();
        }

        /// <summary>
        /// Undo override and block on save.
        /// </summary>
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Save))]
        [HarmonyPrefix]
        public static void BeforeSave()
        {
            foreach (var item in restingReturnOverrides)
            {
                if (Females.TryGetValue(item.Key, out var female))
                {
                    female.PreviousRoomIdStack.Clear();
                    female.PreviousSeatStack.Clear();

                    female.PreviousRoomIdStack.Push(item.Value.roomid);
                    female.PreviousSeatStack.Push(item.Value.seat);
                }
            }

            foreach (var item in restingReturnBlocks)
            {
                if (Females.TryGetValue(item.Key, out var female))
                {
                    female.PreviousRoomIdStack.Clear();
                    female.PreviousSeatStack.Clear();

                    female.PreviousRoomIdStack.Push(item.Value.roomid);
                    female.PreviousSeatStack.Push(item.Value.seat);
                }
            }

            restingReturnBlocks.Clear();
            restingReturnOverrides.Clear();
        }

        public override void Load()
        {
            throw new System.NotImplementedException();
        }
    }
}
