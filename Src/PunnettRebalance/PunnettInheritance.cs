using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using MBMScripts;
using Tools;
using Random = UnityEngine.Random;

namespace PunnettRebalance;

/*
 * Note: In MBMScripts, SeqList fields are wrappers for List fields with same name.
 * You should modify the SeqList when UI update is necessary, and use the List when changes don't
 * require UI to update.
*/

public static class PunnettInheritance
{
    /// <summary>
    /// Rest time.
    /// </summary>
    public static ConfigEntry<bool>? Enable;

    public static void Initialize(ConfigFile config)
    {
        Enable = config.Bind(
            new ConfigInfo<bool>()
            {
                Section = nameof(PunnettInheritance),
                Name = nameof(Enable),
                Description = "Allows 50/50 genetic inheritance",
                DefaultValue = true
            }
        );
    }

    private static ETrait[] TraitArray = (ETrait[])Enum.GetValues(typeof(ETrait));

    [HarmonyPatch(typeof(Character), nameof(Character.InitializeTrait), new[] { typeof(Character), typeof(Character) })]
    [HarmonyPrefix]
    public static bool OverrideInitializeTrait(Character female, Character male, Character __instance)
    {
        if (Enable == null)
            return true;
        if (!Enable.Value)
            return true;

        Plugin.log?.LogMessage("Begin Custom genetic Sequencing");

        var mother = Traverse.Create(female);
        var father = Traverse.Create(male);
        var instancePrivates = Traverse.Create(__instance);

        var f_RaceTraitList = mother.Field("m_RaceTraitList").GetValue<List<ETrait>>();
        var m_RaceTraitList = father.Field("m_RaceTraitList").GetValue<List<ETrait>>();
        var f_TraitInfoDictionary = mother.Property("TraitInfoDictionary").GetValue<SeqDictionary<ETrait, TraitInfo>>();
        var m_TraitInfoDictionary = father.Property("TraitInfoDictionary").GetValue<SeqDictionary<ETrait, TraitInfo>>();

        // Blank slate just in case.
        __instance.OnEnableTrait();
        __instance.ClearRaceTrait();
        __instance.ClearTrait();

        /* --- Inherit racial traits --- */

        // Females inherit mother race traits only
        if (__instance is Female)
        {
            using (List<ETrait>.Enumerator enumerator = f_RaceTraitList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ETrait trait = enumerator.Current;
                    __instance.AddRaceTrait(trait);
                }
                goto IL_86;
            }
        }
        // Males inherit father race traits only
        foreach (ETrait trait2 in m_RaceTraitList)
        {
            __instance.AddRaceTrait(trait2);
        }
        IL_86:

        /* --- Inherit non-racial traits --- */
        /* --- CUSTOM CODE --- */
        var childTraits = new List<(ETrait, float)>();

        // Build trait pairs (type, mother value, father value)
        var traitCandidates = TraitArray
            .Where(t => f_TraitInfoDictionary.ContainsKey(t) || m_TraitInfoDictionary.ContainsKey(t))
            .Select(etrait =>
            {
                f_TraitInfoDictionary.TryGet(etrait, out var motherTrait);
                m_TraitInfoDictionary.TryGet(etrait, out var fatherTrait);
                return (etrait, motherTrait, fatherTrait);
            });

        // Mother Trait: superior genetics, select best genes only
        if (female.TraitContains(ETrait.Trait67))
        {
            foreach (var (etrait, motherTrait, fatherTrait) in traitCandidates)
            {
                TraitData traitMetaData = Database<TraitData>.GetDataByDataId((int)etrait);
                if (traitMetaData.IsPositive)
                {
                    if (
                        motherTrait.Trait != ETrait.None
                        && motherTrait.Value > 0f
                        && motherTrait.Value > fatherTrait.Value
                    )
                    {
                        childTraits.Add((etrait, motherTrait.Value));
                    }
                    else if (fatherTrait.Trait != ETrait.None && fatherTrait.Value > 0f)
                    {
                        childTraits.Add((etrait, fatherTrait.Value));
                    }
                }
                else
                {
                    if (
                        motherTrait.Trait != ETrait.None
                        && motherTrait.Value < 0f
                        && motherTrait.Value < fatherTrait.Value
                    )
                    {
                        childTraits.Add((etrait, motherTrait.Value));
                    }
                    else if (fatherTrait.Trait != ETrait.None && fatherTrait.Value < 0f)
                    {
                        childTraits.Add((etrait, fatherTrait.Value));
                    }
                }
            }
        }
        // 50/50 for all traits
        else
        {
            foreach (var (etrait, motherTrait, fatherTrait) in traitCandidates)
            {
                var r = Random.Range(0f, 100f);
                var selectedTrait = r < 50f ? motherTrait : fatherTrait;
                Plugin.log?.LogMessage(r);

                if (selectedTrait.Trait != ETrait.None)
                {
                    childTraits.Add((etrait, selectedTrait.Value));
                }
            }
        }

        // Add selected traits to child
        foreach (var (eTrait, value) in childTraits)
        {
            __instance.AddTrait(eTrait);
            __instance.AddTraitValue(eTrait, value);
        }

        /* --- END CUSTOM CODE --- */

        var i_TraitList = instancePrivates.Field("m_TraitList").GetValue<List<ETrait>>();

        /* --- Mother Unique Trait: genetic enhancement, one of the inherited traits is enhanced --- */
        if (female.TraitContains(ETrait.Trait76))
        {
            ETrait trait3 = i_TraitList[Random.Range(0, i_TraitList.Count)];
            float traitValue = __instance.GetTraitValue(trait3);
            switch (trait3)
            {
                case ETrait.Trait93:
                    __instance.AddTraitValue(
                        trait3,
                        DecimalAdd(traitValue, GameManager.ConfigData.MinIncreaseOrDecreaseConceptionRate)
                    );
                    break;
                case ETrait.Trait94:
                    __instance.AddTraitValue(
                        trait3,
                        DecimalSubstract(traitValue, GameManager.ConfigData.MinIncreaseOrDecreaseGrowthTime)
                    );
                    break;
                case ETrait.Trait95:
                    __instance.AddTraitValue(
                        trait3,
                        DecimalSubstract(traitValue, GameManager.ConfigData.MinIncreaseOrDecreaseFoodConsumption)
                    );
                    break;
                case ETrait.Trait96:
                    __instance.AddTraitValue(
                        trait3,
                        DecimalAdd(traitValue, GameManager.ConfigData.MinIncreaseOrDecreaseHealth)
                    );
                    break;
                case ETrait.Trait97:
                    __instance.AddTraitValue(
                        trait3,
                        DecimalAdd(traitValue, GameManager.ConfigData.MinIncreaseOrDecreaseBirthCount)
                    );
                    break;
                case ETrait.Trait98:
                    __instance.AddTraitValue(
                        trait3,
                        DecimalAdd(traitValue, GameManager.ConfigData.MinIncreaseOrDecreaseMultiplePregnancyCount)
                    );
                    break;
                case ETrait.Trait99:
                    __instance.AddTraitValue(
                        trait3,
                        DecimalSubstract(traitValue, GameManager.ConfigData.MinIncreaseOrDecreaseSexTime)
                    );
                    break;
                case ETrait.Trait100:
                    __instance.AddTraitValue(
                        trait3,
                        DecimalSubstract(
                            traitValue,
                            GameManager.ConfigData.MinIncreaseOrDecreaseDecreasingMentalityOnSex
                        )
                    );
                    break;
            }
        }
        /* --- Male Trait: rare genetic reversal, one of the inherited negative traits may be flipped to a positive trait of equal value --- */
        if (male.TraitContains(ETrait.Trait90) && Random.Range(0f, 100f) < GameManager.GetTraitValue(ETrait.Trait90, 0))
        {
            List<ETrait> list2 = new();
            for (int l = 0; l < i_TraitList.Count; l++)
            {
                ETrait etrait = i_TraitList[l];
                TraitData dataByDataId3 = Database<TraitData>.GetDataByDataId((int)etrait);
                float traitValue2 = __instance.GetTraitValue(etrait);
                if ((dataByDataId3.IsPositive && traitValue2 < 0f) || (!dataByDataId3.IsPositive && traitValue2 > 0f))
                {
                    list2.Add(etrait);
                    break;
                }
            }
            if (list2.Count > 0)
            {
                SeqUtil.ShuffleList(list2);
                ETrait trait4 = list2[0];
                float traitValue3 = __instance.GetTraitValue(trait4);
                __instance.AddTraitValue(trait4, traitValue3 * -2f);
            }
            female.PopUpMessage(ETrait.Trait90.GetName(), null, 0.25f);
        }
        __instance.SortTrait();
        return false;
    }

    private static bool TryGet(this SeqDictionary<ETrait, TraitInfo> self, ETrait key, out TraitInfo value)
    {
        if (self.ContainsKey(key))
        {
            value = self[key];
            return true;
        }

        value = new TraitInfo
        {
            Trait = ETrait.None,
            Value = 0f,
            UpgradedValue = 0f
        };
        return false;
    }

    private static float DecimalAdd(float x, float y)
    {
        return (float)((decimal)x + (decimal)y);
    }

    private static float DecimalSubstract(float x, float y)
    {
        return (float)((decimal)x - (decimal)y);
    }
}
