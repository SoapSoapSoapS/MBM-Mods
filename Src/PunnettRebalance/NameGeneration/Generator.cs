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
         DragonNames = NamesFile<NameList, NameList>.ParseXml(Names.DragonNames);
        DwarfNames = NamesFile<PrefixSuffixPair, NameList>.ParseXml(Names.DwarfNames);
        ElfNames = NamesFile<PrefixSuffixPair, PrefixSuffixPair>.ParseXml(Names.ElfNames);
        GoblinNames = NamesFile<NameList, NameList>.ParseXml(Names.GoblinNames);
        HitsujiNames = NamesFile<NameList, NameList>.ParseXml(Names.HitsujiNames);
        HumanNames = NamesFile<NameList, NameList>.ParseXml(Names.HumanNames);
        InuNames = NamesFile<NameList, NameList>.ParseXml(Names.InuNames);
        MinotaurNames = NamesFile<NameList, NameList>.ParseXml(Names.MinotaurNames);
        NekoNames = NamesFile<NameList, NameList>.ParseXml(Names.NekoNames);
        OrcNames = NamesFile<NameList, NameList>.ParseXml(Names.OrcNames);
        SalamanderNames = NamesFile<NameList, NameList>.ParseXml(Names.SalamanderNames);
        UsagiNames = NamesFile<NameList, NameList>.ParseXml(Names.UsagiNames);
        WerewolfNames = NamesFile<NameList, NameList>.ParseXml(Names.WerewolfNames);
    }

    public static bool TryGetRaceName(
        ERace race,
        out string fullname,
        string? motherName = null,
        string? fatherName = null
    )
    {
        string? name = null;

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
                name = GoblinNames?.GetFullName();
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
