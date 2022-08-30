using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using MbmModdingTools;
using MBMScripts;

namespace CureVenerealDisease
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency(ToolsPlugin.GUID)]
    public class CvdPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(CureVenerealDisease),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "2.0.0.0";

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

        public CvdPlugin()
        {
            log = Log;
        }

        public override void Load()
        {
            ToolsPlugin.RegisterPeriodicAction(1, Run);
        }

        public static void Run()
        {

        }

        public static void CVD(Female female)
        {
            if (female.VenerealDisease && !female.DisplayName.Contains("cvdexclude"))
            {
                log?.LogDebug("Curing Venereal disease");
                female.VenerealDisease = false;
            }
        }

        public static void BuyCondomAndLoveGel(PlayData instance)
        {
            try
            {
                InteractionSenaLena? senaLena = null;

                if (instance.CountOfCondomBuyable > 0 && instance.HaveEnoughGold(10000))
                {
                    log?.LogDebug("Buying Condoms");
                    senaLena ??= UnityEngine.Object.FindObjectOfType<InteractionSenaLena>();
                    senaLena.SetCondomCountToBuy(instance.m_CountOfCondomBuyable);
                    senaLena.BuyCondom();
                }

                if (instance.CountOfLoveGelBuyable > 0 && instance.HaveEnoughGold(10000))
                {
                    log?.LogDebug("Buying LoveGel");
                    senaLena ??= UnityEngine.Object.FindObjectOfType<InteractionSenaLena>();
                    senaLena.SetCondomCountToBuy(instance.m_CountOfCondomBuyable);
                    senaLena.BuyCondom();
                }
            }
            catch (Exception e)
            {
                log?.LogError(e);
            }
        }
    }
}
