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
    public static ConfigEntry<bool>? Enable;

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
    public static ConfigEntry<bool>? EnablePrivatePixy;

    public static void Initialize(ConfigFile config)
    {
        Enable = config.Bind(
            new ConfigInfo<bool>()
            {
                Section = nameof(SuperPixy),
                Name = "Enable",
                Description = "Enables/Disables all mod functionality.",
                DefaultValue = true
            }
        );

        EmergencyRescue = config.Bind(
            new ConfigInfo<bool>()
            {
                Section = nameof(SuperPixy),
                Name = "EmergencyRescue",
                Description = "If enabled, will rescue low health units from any state.",
                DefaultValue = false
            }
        );

        SafeHealthLevel = config.Bind(
            new ConfigInfo<float>()
            {
                Section = nameof(SuperPixy),
                Name = "SafeHealthLevel",
                Description = "The percentage of health that the pixy will wait for before moving a unit.",
                DefaultValue = 1f
            }
        );

        CriticalHealthLevel = config.Bind(
            new ConfigInfo<float>()
            {
                Section = nameof(SuperPixy),
                Name = "CriticalHealthLevel",
                Description = "The percentage of health that the pixy will try to save unit.",
                DefaultValue = 1f
            }
        );

        EnablePrivatePixy = config.Bind(
            new ConfigInfo<bool>()
            {
                Section = nameof(SuperPixy),
                Name = "CriticalHealthLevel",
                Description = "Enables pixies to operate in the private area.",
                DefaultValue = false
            }
        );
    }

    [HarmonyPatch(typeof(Female), "UpdatePixy", [ ])]
    [HarmonyPrefix]
    public static bool OverrideUpdatePixy(Female __instance)
    {
        if (Enable?.Value != true)
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
    }

    private static bool IsUnitMoveable(Female unit)
    {
        var playerData = GameManager.Instance.PlayerData;

        if (unit.IsPrivate && EnablePrivatePixy?.Value != true)
        {
            return false;
        }

        // Check if unit is in an invalid state

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
        // Check for customers in room
        switch (roomType)
        {
            // Check that audience of Birth Show Room is empty
            case ERoomType.BirthShowRoom:
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 6) != null)
                    return false;
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 7) != null)
                    return false;
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 8) != null)
                    return false;
                return true;

            // Check partnered rooms that don't support pixy
            case ERoomType.None:
            case ERoomType.Pillar:
            case ERoomType.BackDoor:
            case ERoomType.WaitingPlace:
            case ERoomType.GloryHole:
            case ERoomType.KokiRoom:
            case ERoomType.VipRoom:
                return unit.Partner != null;

            // Check that audience of Vvip Room is empty
            case ERoomType.VvipRoom:
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 0) != null)
                    return false;
                if (playerData.GetUnit(ESector.Npc, unit.Slot, 1) != null)
                    return false;
                return true;

            // Check that audience of Horse Show Room is empty
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

    private static EResult TryFindDestination(Female unit, out ERoomType destination, out bool pushPreviousRoom)
    {
        var playerData = GameManager.Instance.PlayerData;
        destination = ERoomType.None;
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
                        destination = ERoomType.SlaveCage;
                        pushPreviousRoom = false;
                        return EResult.Continue;
                    }
                    if (unit.SoldSoon)
                    {
                        destination = ERoomType.Depot;
                        pushPreviousRoom = false;
                        return EResult.Continue;
                    }
                    break;
            }
            return EResult.Continue;
        }

        // handle dead unit
        if (unit.PixyDead && playerData.FloraPixyDead && unit.IsDead)
        {
            if (room == null || roomType != ERoomType.Morgue)
            {
                destination = ERoomType.MilkingRoom;
                pushPreviousRoom = false;
                return EResult.Continue;
            }
            return EResult.Continue;
        }

        // unit marked for sale
        if (unit.SoldSoon && roomType == ERoomType.SlaveCage)
        {
            destination = ERoomType.Depot;
            pushPreviousRoom = false;
            return EResult.Continue;
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
            destination = ERoomType.SlaveCage;
            return EResult.Continue;
        }

        // ignore healing units
        var safeHealth = SafeHealthLevel?.Value ?? 1f;
        if (roomType == ERoomType.SlaveCage && unit.MentalityPercent < safeHealth)
        {
            return EResult.Continue;
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
                destination = ERoomType.SlaveCage;

            if (
                unit.PixyShowFirst
                && playerData.FloraPixyShowFirst
                && !isMonsterBaby
                && playerData.ExistBirthShowRoomAvailable()
            )
            {
                destination = ERoomType.BirthShowRoom;
                return EResult.Continue;
            }

            if (!isTentacleEggs && playerData.ExistDeliveryRoomAvailable())
            {
                destination = ERoomType.DeliveryRoom;
                return EResult.Continue;
            }

            if (!isMonsterBaby && playerData.ExistBirthShowRoomAvailable())
            {
                destination = ERoomType.BirthShowRoom;
                return EResult.Continue;
            }

            return EResult.Continue;
        }

        if (roomType == ERoomType.SlaveCage)
        {
            if (unit.PixyMilk && playerData.FloraPixyMilk)
            {
                destination = ERoomType.MilkingRoom;
                return EResult.Continue;
            }

            if (unit.PixyCapsule && playerData.FloraPixyCapsule)
            {
                destination = ERoomType.MilkingRoom;
                return EResult.Continue;
            }
        }
    }

    /// <summary>
    /// Attempts to move to
    /// </summary>
    private static EResult TryReturnToPreviousRoom(Female unit)
    {
        var playerData = GameManager.Instance.PlayerData;
        Unit room = playerData.GetUnit(unit.PreviousRoomIdStack.Peek());

        // check if the room still exists
        if (room == null)
        {
            unit.PreviousRoomIdStack.Pop();
            unit.PreviousSeatStack.Pop();
            unit.PixyIsWaiting = false;
            return EResult.Failure;
        }

        // check if the room has an available slot
        if (
            playerData.Allocate(unit, ESector.Female, room.Slot, unit.PreviousSeatStack.Peek(), true, -1, true)
            || playerData.Allocate(unit, ESector.Female, room.Slot, -1, true, -1, true)
        )
        {
            switch (TryMovePixy(unit, room))
            {
                case EResult.Success:
                    unit.PreviousRoomIdStack.Pop();
                    unit.PreviousSeatStack.Pop();
                    unit.PixyIsWaiting = false;
                    return EResult.Success;
            }
        }
        else
        {
            // wait until moving is possible
            unit.PixyIsWaiting = true;
        }

        return EResult.Continue;
    }

    private static EResult TryMovePixy(Female unit, Unit room)
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
        movePixy.EndWorldPosition = unit.Position;
        float duration =
            Vector3.Distance(movePixy.StartWorldPosition, movePixy.EndWorldPosition)
            / 10.24f
            * GameManager.ConfigData.PixyMoveSpeed;
        movePixy.Duration = duration;
        movePixy.PixyType = EPixyType.End;
        movePixy.ActivateAsPooledObject();
        unit.Fade(EFadeType.In, 0f, 1f, false, duration);
        if (room is Room room8 && room8.PixyUnitSeqList.Contains(unit.UnitId))
        {
            room8.PixyUnitSeqList.Remove(unit.UnitId);
            return EResult.Success;
        }

        return EResult.Continue;
    }
}
