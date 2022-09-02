using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Il2CppSystem;
using MbmModdingTools;
using MBMScripts;

namespace AutoBuyCondomAndLoveGel
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency(ToolsPlugin.GUID)]
    public class AutoBuyPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(AutoBuyCondomAndLoveGel),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

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

        public const int DayBrothelOpens = 20;

        /// <summary>
        /// Should buy condoms.
        /// </summary>
        public ConfigEntry<bool> EnableBuyCondoms;

        /// <summary>
        /// Max single purchase
        /// </summary>
        public ConfigEntry<int> CondomBuyMax;

        /// <summary>
        /// Max single purchase
        /// </summary>
        public ConfigEntry<int> CondomBuyMin;

        /// <summary>
        /// Time between purchases
        /// </summary>
        public ConfigEntry<float> CondomBuyPeriod;

        /// <summary>
        /// Should buy lovegel.
        /// </summary>
        public ConfigEntry<bool> EnableBuyLoveGel;

        /// <summary>
        /// Time between purchases
        /// </summary>
        public ConfigEntry<float> LoveGelBuyPeriod;

        /// <summary>
        /// Max single purchase
        /// </summary>
        public ConfigEntry<int> LoveGelBuyMax;

        /// <summary>
        /// Max single purchase
        /// </summary>
        public ConfigEntry<int> LoveGelBuyMin;

        public AutoBuyPlugin()
        {
            EnableBuyCondoms = Config.Bind(new ConfigInfo<bool>()
            {
                Section = "AutoBuyCondoms",
                Name = "Enable",
                Description = "If true, automatically buys condoms",
                DefaultValue = true
            });

            CondomBuyMax = Config.Bind(new ConfigInfo<int>()
            {
                Section = "AutoBuyCondoms",
                Name = "Max",
                Description = "Maximum condoms per buy",
                DefaultValue = 100,
                AcceptableValues = new AcceptableValueRange<int>(1, 100)
            });

            CondomBuyMin = Config.Bind(new ConfigInfo<int>()
            {
                Section = "AutoBuyCondoms",
                Name = "Min",
                Description = "Minimum condoms per buy",
                DefaultValue = 10,
                AcceptableValues = new AcceptableValueRange<int>(1, 100)
            });

            CondomBuyPeriod = Config.Bind(new ConfigInfo<float>()
            {
                Section = "AutoBuyCondoms",
                Name = "Period",
                Description = "Seconds between each purchase",
                DefaultValue = 1,
                AcceptableValues = new AcceptableValueRange<float>(1f, 99999)
            });

            EnableBuyLoveGel = Config.Bind(new ConfigInfo<bool>()
            {
                Section = "AutoBuyLovegel",
                Name = "Enable",
                Description = "If true, automatically buys lovegel",
                DefaultValue = true
            });

            LoveGelBuyMax = Config.Bind(new ConfigInfo<int>()
            {
                Section = "AutoBuyLovegel",
                Name = "Max",
                Description = "Maximum lovegel per buy",
                DefaultValue = 100,
                AcceptableValues = new AcceptableValueRange<int>(1, 100)
            });

            LoveGelBuyMin = Config.Bind(new ConfigInfo<int>()
            {
                Section = "AutoBuyLovegel",
                Name = "Min",
                Description = "Minimum lovegel per buy",
                DefaultValue = 10,
                AcceptableValues = new AcceptableValueRange<int>(1, 100)
            });

            LoveGelBuyPeriod = Config.Bind(new ConfigInfo<float>()
            {
                Section = "AutoBuyLovegel",
                Name = "Period",
                Description = "Seconds between each purchase",
                DefaultValue = 1,
                AcceptableValues = new AcceptableValueRange<float>(1f, 99999)
            });
        }

        /// <summary>
        /// Register process.
        /// </summary>
        public override void Load()
        {
            Log.LogMessage("Registering");

            if (EnableBuyCondoms.Value)
            {
                ToolsPlugin.RegisterPeriodicAction(CondomBuyPeriod.Value, BuyCondoms);
            }
            if (EnableBuyLoveGel.Value)
            {
                ToolsPlugin.RegisterPeriodicAction(LoveGelBuyPeriod.Value, BuyLoveGel);
            }
        }

        public void BuyCondoms()
        {
            if (GM == null || PD == null)
                return;

            if (PD.Days <= DayBrothelOpens)
                return;

            if (PD.PlayEventListContains(EPlayEventType.SenaLena2))
                return;

            if (PD.CountOfCondomBuyable < CondomBuyMin.Value)
                return;

            var SL = UnityEngine.Object.FindObjectOfType<InteractionSenaLena>();

            PD.CountOfCondomToBuy = Math.Min(PD.CountOfCondomBuyable, CondomBuyMax.Value);
            SL.BuyCondom();
            Log.LogDebug("Buying condoms");
        }

        public void BuyLoveGel()
        {
            if (GM == null || PD == null)
                return;

            if (PD.Days <= DayBrothelOpens)
                return;

            if (PD.PlayEventListContains(EPlayEventType.SenaLena2))
                return;

            if (PD.CountOfLoveGelBuyable < LoveGelBuyMin.Value)
                return;

            var SL = UnityEngine.Object.FindObjectOfType<InteractionSenaLena>();

            PD.CountOfLoveGelToBuy = Math.Min(PD.CountOfLoveGelBuyable, LoveGelBuyMax.Value);
            SL.BuyLoveGel();
            Log.LogDebug("Buying lovegel");
        }
    }
}
