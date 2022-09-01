using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HarmonyLib.Tools;
using MbmModdingTools;
using MBMScripts;

namespace PixyPlus
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency(ToolsPlugin.GUID)]
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

        /// <summary>
        /// Mod config instance
        /// </summary>
        public static Config? config;

        /// <inheritdoc cref="ToolsPlugin.Females"/>
        public static IDictionary<int, Female> Females
        {
            get => ToolsPlugin.Females;
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
        /// Dictionary of cage seats already allocated.
        /// </summary>
        public static IDictionary<int, (int roomid, int seat)> reservedRooms = new Dictionary<int, (int roomid, int seat)>();

        /// <summary>
        /// MaxHealth of girls injured to leave breeding room.
        /// </summary>
        public static IDictionary<int, (float health, int training)> stolenHealth = new Dictionary<int, (float, int)>();

        /// <summary>
        /// Stores female previous PixyHealth value.
        /// </summary>
        public static IDictionary<int, bool> FemalePreviousPixyHealth = new Dictionary<int, bool>();

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
        /// Placeholder to denote that a character was already processed and had no return location.
        /// </summary>
        public static (int, int) NoReturnTuple => (-1, -1);

        /// <summary>
        /// Keep track of change in PixyHealth value.
        /// </summary>
        public static bool PreviousFloraPixyHealth = false;

        /// <summary>
        /// Clear unit return state on drag start.
        /// </summary>
        [HarmonyPatch(typeof(Unit), nameof(Unit.IsDragging), MethodType.Setter)]
        [HarmonyPostfix]
        public static void OnDragUnit(Unit __instance)
        {
            if (__instance.IsDragging && __instance.Sector == ESector.Female)
            {
                ClearUnitState(__instance.UnitId);
            }
        }

        /// <summary>
        /// Clear all state on load.
        /// </summary>
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Load))]
        [HarmonyPostfix]
        public static void BeforeLoad()
        {
            ClearState();
        }

        /// <summary>
        /// Undo override and block on save.
        /// </summary>
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Save))]
        [HarmonyPrefix]
        public static void BeforeSave()
        {
            ResetState();
        }

        /// <summary>
        /// List of actions to be performed in Run()
        /// </summary>
        public IList<Action<Female>> EnabledActions = new List<Action<Female>>();

        public PixyPlusPlugin()
        {
            log = Log;
            config = new Config(Config);
            if (config.AutoRestFromBreedingRoomEnabled.Value)
            {
                EnabledActions.Add(RestForcibly);
                EnabledActions.Add(UndoForcedHealthReduction);
            }
            if (config.AutoRestEnabled.Value)
            {
                EnabledActions.Add(PreRestIfAvailable);
                EnabledActions.Add(RestIfAvailable);
                EnabledActions.Add(UndoForcedRestAtIdle);
            }
            EnabledActions.Add(ClearReservedRoom);
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
                harmony.PatchAll(typeof(PixyPlusPlugin));

                Log.LogMessage("Harmony Patch Successful");

                Log.LogMessage("Registering");
                ToolsPlugin.RegisterPeriodicAction(1, Run);
            }
            catch
            {
                Log.LogWarning("Harmony Patch Failed");
            }
        }

        /// <summary>
        /// Run plugin operations.
        /// </summary>
        public void Run()
        {
            if (config == null)
                return;

            if (IsPixyHealthDisabled())
                return;

            var ownedFemales = ToolsPlugin.GetOwnedFemales();
            foreach (var female in ownedFemales)
            {
                if (IsFemalePixyHealthDisabled(female))
                    continue;

                if (female.DisplayName.Contains("look"))
                {
                    //Log.LogWarning(female.IsPixyAvailable);
                }

                foreach(var action in EnabledActions)
                {
                    action(female);
                }
            }
        }

        /// <summary>
        /// Handle PixyHealth being disabled
        /// </summary>
        public bool IsPixyHealthDisabled()
        {
            if (PD == null)
                return true;

            if (!PD.FloraPixyHealth)
            {
                if (PreviousFloraPixyHealth)
                {
                    ResetState();
                }
                PreviousFloraPixyHealth = PD.FloraPixyHealth;
                return true;
            }
            PreviousFloraPixyHealth = PD.FloraPixyHealth;
            return false;
        }

        /// <summary>
        /// Check if the unit specific pixy health is disabled.
        /// </summary>
        public bool IsFemalePixyHealthDisabled(Female female)
        {
            if (!female.PixyHealth)
            {
                if (FemalePreviousPixyHealth.TryGetValue(female.UnitId, out var prevPixyHealth) && prevPixyHealth)
                {
                    ResetFemaleState(female);
                }
                FemalePreviousPixyHealth[female.UnitId] = female.PixyHealth;
                return true;
            }
            FemalePreviousPixyHealth[female.UnitId] = female.PixyHealth;
            return false;
        }

        /// <summary>
        /// Block auto-return on task end.
        /// </summary>
        public void PreRestIfAvailable(Female female)
        {
            if (restingReturnBlocks.ContainsKey(female.UnitId))
                return;

            if (!PreReadyToRestStates.Contains(female.State))
                return;

            if (female.PreviousRoomIdStack.Count == 0 || female.PreviousSeatStack.Count == 0)
            {
                restingReturnBlocks.Add(female.UnitId, NoReturnTuple);
                Log.LogDebug("Block return (with no return target)");
                return;
            }

            var prevRoom = female.PreviousRoomIdStack.Pop();
            var prevSeat = female.PreviousSeatStack.Pop();
            restingReturnBlocks.Add(female.UnitId, (prevRoom, prevSeat));
            Log.LogDebug("Block return");
        }

        /// <summary>
        /// Move unit to rest in cage if otherwise idle.
        /// </summary>
        public void RestIfAvailable(Female female)
        {
            if (restingReturnOverrides.ContainsKey(female.UnitId))
                return;

            var isReadyToRestState = ReadyToRestStates.Contains(female.State);
            var isRestAvailable = isReadyToRestState;

            if (isRestAvailable && TryGetSeatInRoomByType(ERoomType.SlaveCage, female, out var target))
            {
                if (restingReturnBlocks.TryGetValue(female.UnitId, out var prevRoom))
                {
                    restingReturnOverrides.Add(female.UnitId, prevRoom);
                    restingReturnBlocks.Remove(female.UnitId);
                    female.PreviousRoomIdStack.Push(target.room.UnitId);
                    female.PreviousSeatStack.Push(target.seat);
                    Log.LogDebug("Resting (from block return)");
                }

                else if (female.PreviousRoomIdStack.Count == 0 || female.PreviousSeatStack.Count == 0)
                {
                    restingReturnOverrides.Add(female.UnitId, NoReturnTuple);
                    female.PreviousRoomIdStack.Push(target.room.UnitId);
                    female.PreviousSeatStack.Push(target.seat);
                    Log.LogDebug("Resting (from no return)");
                    return;
                }

                else
                {
                    var prevRoomId = female.PreviousRoomIdStack.Pop();
                    var prevSeat = female.PreviousSeatStack.Pop();
                    restingReturnOverrides.Add(female.UnitId, (prevRoomId, prevSeat));
                    female.PreviousRoomIdStack.Push(target.room.UnitId);
                    female.PreviousSeatStack.Push(target.seat);
                    Log.LogDebug("Resting (from normal return)");
                }
            }
        }

        /// <summary>
        /// Injure unit to trigger health pixy.
        /// </summary>
        public void RestForcibly(Female female)
        {
            if (restingReturnOverrides.ContainsKey(female.UnitId) || stolenHealth.ContainsKey(female.UnitId))
                return;

            var isRestingInBreedingRoomNotPregnant = female.Room.RoomType == ERoomType.BreedingRoom && female.State == EState.Rest && !female.IsPregnant;

            if (isRestingInBreedingRoomNotPregnant)
            {
                stolenHealth.Add(female.UnitId, (female.Health, female.Training));
                restingReturnOverrides.Add(female.UnitId, (female.Room.UnitId, female.Seat));
                female.Training = 10;
                female.TrainingLevel = 1;
                female.Health = female.MaxHealth * .1f;
                Log.LogDebug("Resting (from forced return)");
            }
        }

        /// <summary>
        /// Restore unit health once in cage.
        /// </summary>
        public void UndoForcedHealthReduction(Female female)
        {
            if (!stolenHealth.TryGetValue(female.UnitId, out var stolen))
                return;

            if (female.Room.RoomType != ERoomType.SlaveCage)
                return;

            female.Health = stolen.health;
            female.Training = stolen.training;
            stolenHealth.Remove(female.UnitId);
            Log.LogDebug("Returning stolen health");

            if(restingReturnOverrides.TryGetValue(female.UnitId, out var target))
            {
                female.PreviousRoomIdStack.Clear();
                female.PreviousSeatStack.Clear();
            }
        }

        /// <summary>
        /// Once unit in cage is done resting, move them back if necessary.
        /// </summary>
        public void UndoForcedRestAtIdle(Female female)
        {
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
                Log.LogDebug("Adding new target");
            }

            restingReturnOverrides.Remove(female.UnitId);

            Log.LogDebug("Removing return override");
        }

        /// <summary>
        /// Clear reserved room once unit is in cage.
        /// </summary>
        public static void ClearReservedRoom(Female female)
        {
            if (!reservedRooms.ContainsKey(female.UnitId))
                return;

            if (female.Room.RoomType != ERoomType.SlaveCage)
                return;

            reservedRooms.Remove(female.UnitId);
        }

        /// <summary>
        /// Only before load save game
        /// </summary>
        public static void ClearState()
        {
            restingReturnBlocks.Clear();
            restingReturnOverrides.Clear();
            stolenHealth.Clear();
            reservedRooms.Clear();
        }

        /// <summary>
        /// Remove all movement state from unit.
        /// </summary>
        public static void ClearUnitState(int id)
        {
            restingReturnBlocks.Remove(id);
            restingReturnOverrides.Remove(id);
            reservedRooms.Remove(id);
            if(!Females.TryGetValue(id, out var female))
                stolenHealth.Remove(id);

            if (stolenHealth.TryGetValue(id, out var target))
            {
                female.Health = target.health;
                female.Training = target.training;
            }

            female.PreviousRoomIdStack.Clear();
            female.PreviousSeatStack.Clear();
            female.m_PixyIsWaiting = false;
        }

        public static void ResetState()
        {
            foreach(var kvp in Females)
            {
                var female = kvp.Value;
                ResetFemaleState(female);
            }

            restingReturnBlocks.Clear();
            restingReturnOverrides.Clear();
            stolenHealth.Clear();
            reservedRooms.Clear();
        }

        public static void ResetFemaleState(Female female)
        {
            female.PreviousRoomIdStack.Clear();
            female.PreviousSeatStack.Clear();

            if(restingReturnOverrides.TryGetValue(female.UnitId, out var prevLocation))
            {
                female.PreviousRoomIdStack.Push(prevLocation.roomid);
                female.PreviousSeatStack.Push(prevLocation.seat);
                restingReturnOverrides.Remove(female.UnitId);
            }
            if (restingReturnBlocks.TryGetValue(female.UnitId, out var blockLocation))
            {
                female.PreviousRoomIdStack.Push(blockLocation.roomid);
                female.PreviousSeatStack.Push(blockLocation.seat);
                restingReturnBlocks.Remove(female.UnitId);
            }
            if (stolenHealth.TryGetValue(female.UnitId, out var target))
            {
                female.Health = target.health;
                female.Training = target.training;
                stolenHealth.Remove(female.UnitId);
            }

            reservedRooms.Remove(female.UnitId);
        }

        /// <summary>
        /// Find nearest room of specified type with open seat
        /// </summary>
        public bool TryGetSeatInRoomByType(ERoomType roomType, Character character, out (Room room, int seat) result)
        {
            result = (new Room(0), 0);
            if (PD == null)
                return false;

            var rooms = PD.GetClosedRoomList(roomType, character.Position, PD.IsPrivateEstate);
            foreach (var room in rooms)
            {
                if (room.m_AllocatableSeatCount == 0)
                    continue;

                var seat = PD.GetEmptySeatInRoom(room.Sector, room.Slot);

                if (reservedRooms.Any(res => res.Value.roomid == room.UnitId && res.Value.seat == seat))
                    continue;

                result = (room, seat);
                return true;
            }
            return false;
        }
    }
}
