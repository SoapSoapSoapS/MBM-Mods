using System;
using BepInEx.Configuration;
using Fucktorio.Models;
using HarmonyLib;
using MBMScripts;
using Tools;
using UnityEngine;

namespace Fucktorio;

public static class SuperPixy
{
    /// <summary>
    /// Enables/Disables all mod functionality.
    /// </summary>
    public static ConfigEntry<bool>? CustomPixyLogic;

    /// <summary>
    /// If enabled, will rescue low health units from any state.
    /// </summary>
    public static ConfigEntry<bool>? EmergencyRescue;

    /// <summary>
    /// The percentage of health that the pixy will wait for before moving a unit.
    /// </summary>
    public static ConfigEntry<float>? SafeHealthLevel;

    /// <summary>
    /// The percentage of health that the pixy will try to save unit.
    /// </summary>
    public static ConfigEntry<float>? CriticalHealthLevel;

    /// <summary>
    /// Enables pixies to operate in the private area.
    /// </summary>
    public static ConfigEntry<bool>? PrivatePixy;

    public static void Initialize(ConfigFile config)
    {
        CustomPixyLogic = config.Bind(
            new ConfigInfo<bool>()
            {
                Section = nameof(SuperPixy),
                Name = nameof(CustomPixyLogic),
                Description = "Enables/Disables all mod functionality.",
                DefaultValue = true
            }
        );

        EmergencyRescue = config.Bind(
            new ConfigInfo<bool>()
            {
                Section = nameof(SuperPixy),
                Name = nameof(EmergencyRescue),
                Description = "If enabled, will rescue low health units from any state.",
                DefaultValue = false
            }
        );

        SafeHealthLevel = config.Bind(
            new ConfigInfo<float>()
            {
                Section = nameof(SuperPixy),
                Name = nameof(SafeHealthLevel),
                Description = "The percentage of health that the pixy will wait for before moving a unit.",
                DefaultValue = 1f
            }
        );

        CriticalHealthLevel = config.Bind(
            new ConfigInfo<float>()
            {
                Section = nameof(SuperPixy),
                Name = nameof(CriticalHealthLevel),
                Description = "The percentage of health that the pixy will try to save unit.",
                DefaultValue = 1f
            }
        );

        PrivatePixy = config.Bind(
            new ConfigInfo<bool>()
            {
                Section = nameof(SuperPixy),
                Name = nameof(PrivatePixy),
                Description = "Enables pixies to operate in the private area.",
                DefaultValue = false
            }
        );
    }

    [HarmonyPatch(typeof(Female), "UpdatePixy", [ ])]
    [HarmonyPrefix]
    public static bool OverrideUpdatePixy(Female __instance)
    {
        if (CustomPixyLogic?.Value != true)
            return true;

        PixyLogic(__instance);

        return false;
    }

    private static void PixyLogic(Female unit)
    {
        var instance = GameManager.Instance;
        var playerData = instance.PlayerData;

        if (!IsUnitMoveable(unit))
            return;

        // Unused game logic, don't fix what isn't broken.
        if (unit.Hunger)
        {
            unit.FeedFood();
        }

        if(unit.PreviousRoomIdStack.Count != 0 && ReturnToPreviousRoom(unit))
        {
            return;
        }

        FindDesiredDestination(unit, out var destinationType, out var pushPreviousRoom);

        if (destinationType == ERoomType.Depot)
        {
            if (playerData.Allocate(unit, ESector.Product, instance.Depot.Slot, -1, false, -1, true))
            {
                MovePixy(unit, instance.Depot, EPixyType.Once);
            }
            return;
        }

        if (destinationType != ERoomType.None)
        {
            MoveToNewRoom(unit, destinationType, pushPreviousRoom);
            return;
        }
    }

    private static bool IsUnitMoveable(Female unit)
    {
        var playerData = GameManager.Instance.PlayerData;

        if (unit.IsPrivate && PrivatePixy?.Value != true)
        {
            return false;
        }

        // Checks if unit is being moved already
        var sector = unit.Sector;
        if (sector != ESector.Female && sector != ESector.Baby)
            return false;
        if (unit.IsDragging)
            return false;
        if (unit.IsSelected)
            return false;
        if (unit.FadeValue < 1f)
            return false;
        if (unit.StateTime < 3f)
            return false;
        if (unit.Room == null)
            return false;

        if (EmergencyRescue?.Value == true)
            return true;

        var roomType = unit.Room.RoomType;
        // Checks for customers in room
        switch (roomType)
        {
            // Checks that audience of Birth Show Room is empty
            case ERoomType.BirthShowRoom:
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 6) != null)
                    return false;
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 7) != null)
                    return false;
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 8) != null)
                    return false;
                return true;

            // Checks partnered rooms that don't support pixy
            case ERoomType.None:
            case ERoomType.Pillar:
            case ERoomType.BackDoor:
            case ERoomType.WaitingPlace:
            case ERoomType.GloryHole:
            case ERoomType.KokiRoom:
            case ERoomType.VipRoom:
                return unit.Partner != null;

            // Checks that audience of Vvip Room is empty
            case ERoomType.VvipRoom:
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 0) != null)
                    return false;
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 1) != null)
                    return false;
                return true;

            // Checks that audience of Horse Show Room is empty
            case ERoomType.Stall:
                switch (unit.Seat)
                {
                    case 0:
                        if (playerData.GetUnit(ESector.Npc, unit.Slot, 2) != null)
                            return false;
                        if (playerData.GetUnit(ESector.Npc, unit.Slot, 3) != null)
                            return false;
                        if (playerData.GetUnit(ESector.Npc, unit.Slot, 4) != null)
                            return false;
                        return true;
                    case 1:
                        if (playerData.GetUnit(ESector.Npc, unit.Slot, 5) != null)
                            return false;
                        if (playerData.GetUnit(ESector.Npc, unit.Slot, 6) != null)
                            return false;
                        if (playerData.GetUnit(ESector.Npc, unit.Slot, 7) != null)
                            return false;
                        return true;
                }
                return true;

            default:
                return true;
        }
    }

    private static void FindDesiredDestination(Female unit, out ERoomType destinationType, out bool pushPreviousRoom)
    {
        var playerData = GameManager.Instance.PlayerData;
        destinationType = ERoomType.None;
        pushPreviousRoom = true;

        var room = unit.Room;
        var roomType = room.RoomType;

        // handle baby units
        if (unit.IsBaby)
        {
            switch (roomType)
            {
                case ERoomType.BirthShowRoom:
                case ERoomType.DeliveryRoom:
                    if (unit.MovedSoon)
                    {
                        destinationType = ERoomType.SlaveCage;
                        pushPreviousRoom = false;
                        return;
                    }
                    if (unit.SoldSoon)
                    {
                        destinationType = ERoomType.Depot;
                        pushPreviousRoom = false;
                        return;
                    }
                    break;
            }
            return;
        }

        // handle dead unit
        if (unit.PixyDead && playerData.FloraPixyDead && unit.IsDead)
        {
            if (room == null || roomType != ERoomType.Morgue)
            {
                destinationType = ERoomType.MilkingRoom;
                pushPreviousRoom = false;
                return;
            }
            return;
        }

        // unit marked for sale
        if (unit.SoldSoon && roomType == ERoomType.SlaveCage)
        {
            destinationType = ERoomType.Depot;
            pushPreviousRoom = false;
            return;
        }

        // unit at risk of death
        var criticalHealth = CriticalHealthLevel?.Value ?? 0.2f;
        if (
            unit.PixyHealth
            && playerData.FloraPixyHealth
            && roomType != ERoomType.SlaveCage
            && unit.MentalityPercent < criticalHealth
        )
        {
            destinationType = ERoomType.SlaveCage;
            return;
        }

        // ignore healing units
        var safeHealth = SafeHealthLevel?.Value ?? 1f;
        if (roomType == ERoomType.SlaveCage && unit.MentalityPercent < safeHealth)
        {
            return;
        }

        // handle unit ready to give birth
        if (unit.PixyBirth && playerData.FloraPixyBirth && unit.IsPregnant)
        {
            var isMonsterBaby = false;
            var isTentacleEggs = false;

            foreach (int unitId in unit.FetusUnitIdList)
            {
                switch (playerData.GetUnit(unitId).Race)
                {
                    case ERace.Goblin:
                    case ERace.Orc:
                    case ERace.Werewolf:
                    case ERace.Minotaur:
                    case ERace.Salamander:
                        isMonsterBaby = true;
                        break;
                    case ERace.Tentacle:
                        isMonsterBaby = true;
                        isTentacleEggs = true;
                        break;
                }
            }

            if (roomType != ERoomType.SlaveCage)
                destinationType = ERoomType.SlaveCage;

            if (
                unit.PixyShowFirst
                && playerData.FloraPixyShowFirst
                && !isMonsterBaby
                && playerData.ExistBirthShowRoomAvailable()
            )
            {
                destinationType = ERoomType.BirthShowRoom;
                return;
            }

            if (!isTentacleEggs && playerData.ExistDeliveryRoomAvailable())
            {
                destinationType = ERoomType.DeliveryRoom;
                return;
            }

            if (!isMonsterBaby && playerData.ExistBirthShowRoomAvailable())
            {
                destinationType = ERoomType.BirthShowRoom;
                return;
            }

            return;
        }

        if (roomType == ERoomType.SlaveCage)
        {
            if (unit.PixyMilk && playerData.FloraPixyMilk)
            {
                destinationType = ERoomType.MilkingRoom;
                return;
            }

            if (unit.PixyCapsule && playerData.FloraPixyCapsule && unit.Training < GameManager.ConfigData.MaxTraining)
            {
                destinationType = ERoomType.MilkingRoom;
                return;
            }
        }

        return;
    }

    private static void MoveToNewRoom(Female unit, ERoomType destinationType, bool pushPreviousRoom)
    {
        var playerData = GameManager.Instance.PlayerData;
        var pixyType = pushPreviousRoom ? EPixyType.Start : EPixyType.Once;

        if (destinationType == ERoomType.Depot) { }

        foreach (Room room in playerData.GetClosedRoomList(destinationType, unit.Room.Position, false))
        {
            if (playerData.Allocate(unit, ESector.Female, room.Slot, -1, true, -1, true))
            {
                MovePixy(unit, room, pixyType);
                return;
            }
        }

        return;
    }

    private static bool ReturnToPreviousRoom(Female unit)
    {
        var playerData = GameManager.Instance.PlayerData;
        Unit room = playerData.GetUnit(unit.PreviousRoomIdStack.Peek());

        // Checks if the room still exists
        if (room == null)
        {
            unit.PreviousRoomIdStack.Pop();
            unit.PreviousSeatStack.Pop();
            unit.PixyIsWaiting = false;
            return true;
        }

        // Checks if the room has an available slot
        if (
            playerData.Allocate(unit, ESector.Female, room.Slot, unit.PreviousSeatStack.Peek(), true, -1, true)
            || playerData.Allocate(unit, ESector.Female, room.Slot, -1, true, -1, true)
        )
        {
            MovePixy(unit, room, EPixyType.End);
            return true;
        }
        else
        {
            // wait until moving is possible
            unit.PixyIsWaiting = true;
            return false;
        }
    }

    private static void MovePixy(Female unit, Unit destination, EPixyType pixyType)
    {
        var playerData = GameManager.Instance.PlayerData;
        var position = unit.Position;
        var unitX = Traverse.Create(unit);

        unitX
            .Field("m_RaceTraitList")
            .SetValue(SeqObjectPoolManager.Instance.GetPooledObject("MovePixy", null, null).GetComponent<MovePixy>());

        var movePixy = unitX.Field("m_RaceTraitList").GetValue<MovePixy>();

        movePixy.GetComponent<TargetUnit>().Unit = unit;
        movePixy.Character = unit;
        MovePixy movePixy3 = movePixy;
        movePixy3.OnRelease = (Action)
            Delegate.Combine(
                movePixy3.OnRelease,
                new Action(
                    delegate()
                    {
                        movePixy = null;
                    }
                )
            );
        movePixy.StartWorldPosition = position;
        if (destination is Room room && room.RoomType == ERoomType.Depot)
            movePixy.EndWorldPosition = room.Center;
        else
            movePixy.EndWorldPosition = unit.Position;
        float duration =
            Vector3.Distance(movePixy.StartWorldPosition, movePixy.EndWorldPosition)
            / 10.24f
            * GameManager.ConfigData.PixyMoveSpeed;
        movePixy.Duration = duration;
        movePixy.PixyType = EPixyType.End;

        movePixy.PixyType = pixyType;
        if (pixyType == EPixyType.Start)
        {
            unit.PreviousRoomIdStack.Push(unit.Room.UnitId);
            unit.PreviousSeatStack.Push(unit.Seat);
            if (!unit.Room.PixyUnitSeqList.Contains(unit.UnitId))
            {
                unit.Room.PixyUnitSeqList.Add(unit.UnitId);
            }
        }

        movePixy.ActivateAsPooledObject();
        unit.Fade(EFadeType.In, 0f, 1f, false, duration);

        if (pixyType == EPixyType.End)
        {
            if (destination is Room room2 && room2.PixyUnitSeqList.Contains(unit.UnitId))
            {
                room2.PixyUnitSeqList.Remove(unit.UnitId);
            }

            unit.PreviousRoomIdStack.Pop();
            unit.PreviousSeatStack.Pop();
            unit.PixyIsWaiting = false;
        }

        return;
    }
}
