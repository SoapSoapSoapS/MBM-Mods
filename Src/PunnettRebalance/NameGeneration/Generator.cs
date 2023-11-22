using System;
using MBMScripts;
using Newtonsoft.Json;
using PunnettRebalance.NameGeneration.Models;

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

    public static Random Random = new Random();

    static Generator()
    {
        DragonNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.DragonNames);
        DwarfNames = JsonConvert.DeserializeObject<NamesFile<PrefixSuffixPair, NameList>>(Names.DwarfNames);
        ElfNames = JsonConvert.DeserializeObject<NamesFile<PrefixSuffixPair, PrefixSuffixPair>>(Names.ElfNames);
        GoblinNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.GoblinNames);
        HitsujiNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.HitsujiNames);
        HumanNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.HumanNames);
        InuNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.InuNames);
        MinotaurNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.MinotaurNames);
        NekoNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.NekoNames);
        OrcNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.OrcNames);
        SalamanderNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.SalamanderNames);
        UsagiNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.UsagiNames);
        WerewolfNames = JsonConvert.DeserializeObject<NamesFile<NameList, NameList>>(Names.WerewolfNames);
    }

    private static string? GetFullName<T, U>(NamesFile<T, U>? nameFile)
        where T : INameGenerator
        where U : INameGenerator 
    {
        if(nameFile == null)
            return null;

        return nameFile.Names.GetName() + " " + nameFile.Surnames.GetName();
    }

    public static bool TryGetRaceName(ERace race, out string fullname)
    {
        string? name = null;

        switch(race)
        {
            case ERace.Dragonian:
                name = GetFullName(DragonNames);
                break;
            case ERace.Dwarf:
                name = GetFullName(DwarfNames);
                break;
            case ERace.Elf:
                name = GetFullName(ElfNames);
                break;
            case ERace.Goblin:
                name = GetFullName(GoblinNames);
                break;
            case ERace.Hitsuji:
                name = GetFullName(HitsujiNames);
                break;
            case ERace.Human:
                name = GetFullName(HumanNames);
                break;
            case ERace.Inu:
                name = GetFullName(InuNames);
                break;
            case ERace.Minotaur:
                name = GetFullName(MinotaurNames);
                break;
            case ERace.Neko:
                name = GetFullName(NekoNames);
                break;
            case ERace.Orc:
                name = GetFullName(OrcNames);
                break;
            case ERace.Salamander:
                name = GetFullName(SalamanderNames);
                break;
            case ERace.Usagi:
                name = GetFullName(UsagiNames);
                break;
            case ERace.Werewolf:
                name = GetFullName(WerewolfNames);
                break;
        }

        if(name == null)
        {
            fullname = string.Empty;
            return false;
        }

        fullname = name;
        return true;
    }
}
