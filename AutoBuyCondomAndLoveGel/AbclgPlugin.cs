using BepInEx;
using BepInEx.Unity.IL2CPP;
using MbmModdingTools;
using MBMScripts;

namespace AutoBuyCondomAndLoveGel
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency(ToolsPlugin.GUID)]
    public class AbclgPlugin : BasePlugin
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

        /// <summary>
        /// Register process.
        /// </summary>
        public override void Load()
        {
            Log.LogMessage("Registering");
            //ToolsPlugin.RegisterPeriodicAction(1, Run);
        }

        public void Run()
        {
            if (GM == null || PD == null)
                return;


            var x = PD.CountOfCondomBuyable;
            var y = PD.CountOfCondomToBuy;
            var xx = PD.CountOfLoveGelBuyable;
            var xy = PD.CountOfLoveGelToBuy;
            var yx = PD.SenaLenaHideSceneButton;
            var message = $"SenaLena is disabled: {yx}";
            //Log.LogMessage(message);

            var SL = UnityEngine.Object.FindObjectOfType<InteractionSenaLena>();
        }
    }
}
