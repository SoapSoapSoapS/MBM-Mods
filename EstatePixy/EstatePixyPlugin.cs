using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib.Tools;
using HarmonyLib;
using MBMScripts;
using MbmModdingTools;

namespace EstatePixy
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency(ToolsPlugin.GUID)]
    public class EstatePixyPlugin : BasePlugin
    {
        public const string
            MODNAME = nameof(EstatePixy),
            AUTHOR = "SoapBoxHero",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.1.0";

        /// <summary>
        /// Mod log instance
        /// </summary>
        public static BepInEx.Logging.ManualLogSource? log;


        [HarmonyPatch(typeof(Female), nameof(Female.Sex))]
        [HarmonyPostfix]
        public static void OnSex(Female __instance)
        {
            if (__instance.State != EState.Sex && __instance.State != EState.Drained)
            {

                log?.LogWarning(__instance.IsPrivate);
            }
        }

        /// <summary>
        /// Initialize logger.
        /// </summary>
        public EstatePixyPlugin()
        {
            log = Log;
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
                harmony.PatchAll(typeof(EstatePixyPlugin));

                Log.LogMessage("Harmony Patch Successful");
            }
            catch
            {
                Log.LogWarning("Harmony Patch Failed");
            }
        }

        /// <summary>
        /// Find nearest room of specified type with open seat
        /// </summary>
        public static bool TryGetSeatInRoomByType(ERoomType roomType, Character character, out (Room room, int seat) result)
        {
            result = (new Room(0), 0);
            if (ToolsPlugin.PD == null)
                return false;

            var rooms = ToolsPlugin.PD.GetClosedRoomList(roomType, character.Position, ToolsPlugin.PD.IsPrivateEstate);
            foreach (var room in rooms)
            {
                if (room.m_AllocatableSeatCount == 0)
                    continue;

                var seat = ToolsPlugin.PD.GetEmptySeatInRoom(room.Sector, room.Slot);

                result = (room, seat);
                return true;
            }
            return false;
        }
    }
}
