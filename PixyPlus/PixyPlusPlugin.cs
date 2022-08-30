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

        public static void PreRestIfAvailable(Female female)
        {
            if (restingReturnBlocks.ContainsKey(female.UnitId))
                return;

            if (!PreReadyToRestStates.Contains(female.State))
                return;

            if (female.PreviousRoomIdStack.Count == 0 || female.PreviousSeatStack.Count == 0)
            {
                restingReturnBlocks.Add(female.UnitId, NoReturnTuple);
                log?.LogDebug("Block return (with no return target)");
                return;
            }

            var prevRoom = female.PreviousRoomIdStack.Pop();
            var prevSeat = female.PreviousSeatStack.Pop();
            restingReturnBlocks.Add(female.UnitId, (prevRoom, prevSeat));
            log?.LogDebug("Block return");
        }

        public static void RestIfAvailable(Female female)
        {
            if (PD == null)
            {
                return;
            }

            if (female.DisplayName.Contains("look"))
            {
                if (restingReturnOverrides.ContainsKey(female.UnitId))
                    return;

                var isRestingInBreedingRoomNotPregnant = female.Room.RoomType == ERoomType.BreedingRoom && female.State == EState.Rest && !female.IsPregnant;
                var isReadyToRestState = ReadyToRestStates.Contains(female.State);
                var isRestAvailable = isReadyToRestState || isRestingInBreedingRoomNotPregnant;

                if (isRestAvailable && TryGetSeatInRoomByType(ERoomType.SlaveCage, female, PD, out var target))
                {
                    if (restingReturnBlocks.TryGetValue(female.UnitId, out var prevRoom))
                    {
                        restingReturnOverrides.Add(female.UnitId, prevRoom);
                        restingReturnBlocks.Remove(female.UnitId);
                        female.PreviousRoomIdStack.Push(target.roomid);
                        female.PreviousSeatStack.Push(target.seat);
                        log?.LogDebug("Resting (from block return)");
                        log?.LogDebug(prevRoom);
                        return;
                    }

                    if (female.PreviousRoomIdStack.Count == 0 || female.PreviousSeatStack.Count == 0)
                    {
                        restingReturnOverrides.Add(female.UnitId, NoReturnTuple);
                        female.PreviousRoomIdStack.Push(target.roomid);
                        female.PreviousSeatStack.Push(target.seat);
                        log?.LogDebug("Resting (from no return)");
                        return;
                    }

                    var prevRoomId = female.PreviousRoomIdStack.Pop();
                    var prevSeat = female.PreviousSeatStack.Pop();
                    restingReturnOverrides.Add(female.UnitId, (prevRoomId, prevSeat));
                    female.PreviousRoomIdStack.Push(target.roomid);
                    female.PreviousSeatStack.Push(target.seat);
                    log?.LogDebug("Resting (from normal return)");
                }
            }
        }

        public static void UndoForcedRestAtIdle(Female female)
        {
            if (!female.DisplayName.Contains("look"))
                return;

            if (!restingReturnOverrides.TryGetValue(female.UnitId, out var target))
                return;

            if (female.Room.RoomType != ERoomType.SlaveCage)
                return;

            if (female.State != EState.Idle)
                return;

            if (target.roomid != NoReturnTuple.Item1)
            {
                female.PreviousRoomIdStack.Push(target.roomid);
                female.PreviousSeatStack.Push(target.seat);
                log?.LogDebug("Adding new target");
            }

            restingReturnOverrides.Remove(female.UnitId);

            log?.LogDebug("Removing return override");
        }

        public static bool TryGetSeatInRoomByType(ERoomType roomType, Character character, PlayData pd, out (int roomid, int seat) result)
        {
            var rooms = pd.GetClosedRoomList(ERoomType.SlaveCage, character.Position, pd.IsPrivateEstate);
            foreach (var room in rooms)
            {
                if (room.m_AllocatableSeatCount == 0)
                {
                    continue;
                }

                var seat = pd.GetEmptySeatInRoom(room.Sector, room.Slot);

                result = (room.UnitId, seat);
                return true;
            }

            result = (0, 0);
            return false;
        }
    }
}
