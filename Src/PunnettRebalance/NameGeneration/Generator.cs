using System.Xml.Serialization;
using BepInEx.Configuration;
using MBMScripts;
using PunnettRebalance.NameGeneration.Models;
using UnityEngine;

namespace PunnettRebalance.NameGeneration;

public static class Generator
{
    private static NamesFile<NameList, NameList>? DragonNames;
    private static NamesFile<PrefixSuffixPair, NameList>? DwarfNames;
    private static NamesFile<PrefixSuffixPair, PrefixSuffixPair>? ElfNames;
    private static NamesFile<NameList, NameList>? GoblinNames;
    private static NamesFile<NameList, NameList>? HitsujiNames;
    private static NamesFile<NameList, NameList>? HumanNames;
    private static NamesFile<NameList, NameList>? InuNames;
    private static NamesFile<NameList, NameList>? MinotaurNames;
    private static NamesFile<NameList, NameList>? NekoNames;
    private static NamesFile<NameList, NameList>? OrcNames;
    private static NamesFile<NameList, NameList>? SalamanderNames;
    private static NamesFile<NameList, NameList>? UsagiNames;
    private static NamesFile<NameList, NameList>? WerewolfNames;

    public static System.Random Random = new();

    static Generator()
    {
        // DragonNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.DragonNames);
        DwarfNames = JsonUtility.FromJson<NamesFile<PrefixSuffixPair, NameList>>(Names.DwarfNames);
        ElfNames = JsonUtility.FromJson<NamesFile<PrefixSuffixPair, PrefixSuffixPair>>(Names.ElfNames);
        GoblinNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.GoblinNames);
        HitsujiNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.HitsujiNames);
        HumanNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.HumanNames);
        InuNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.InuNames);
        MinotaurNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.MinotaurNames);
        NekoNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.NekoNames);
        OrcNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.OrcNames);
        SalamanderNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.SalamanderNames);
        UsagiNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.UsagiNames);
        WerewolfNames = JsonUtility.FromJson<NamesFile<NameList, NameList>>(Names.WerewolfNames);
    }

    public static bool TryGetRaceName(
        ERace race,
        out string fullname,
        string? motherName = null,
        string? fatherName = null
    )
    {
        string? name = null;

        Plugin.log?.LogWarning(NamesFile<NameList, NameList>.ParseXml(Names.DragonNames).Names[5]);

        switch (race)
        {
            case ERace.Dragonian:
                name = DragonNames?.GetFullName(motherName);
                break;
            case ERace.Dwarf:
                name = DwarfNames?.GetFullName(motherName);
                break;
            case ERace.Elf:
                name = ElfNames?.GetFullName(motherName);
                break;
            case ERace.Goblin:
                name = GoblinNames?.GetFullName(fatherName);
                break;
            case ERace.Hitsuji:
                name = HitsujiNames?.GetFullName(motherName);
                break;
            case ERace.Human:
                name = HumanNames?.GetFullName(motherName);
                break;
            case ERace.Inu:
                name = InuNames?.GetFullName(motherName);
                break;
            case ERace.Minotaur:
                name = MinotaurNames?.GetFullName(fatherName);
                break;
            case ERace.Neko:
                name = NekoNames?.GetFullName(motherName);
                break;
            case ERace.Orc:
                name = OrcNames?.GetFullName(fatherName);
                break;
            case ERace.Salamander:
                name = SalamanderNames?.GetFullName(fatherName);
                break;
            case ERace.Usagi:
                name = UsagiNames?.GetFullName(motherName);
                break;
            case ERace.Werewolf:
                name = WerewolfNames?.GetFullName(fatherName);
                break;
        }

        if (name == null)
        {
            fullname = string.Empty;
            return false;
        }

        fullname = name;
        return true;
    }
}
