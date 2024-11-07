using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using SpeedCheat.Patches;

namespace SpeedCheat
{
    [BepInPlugin(modGUID, modName, modVersion)]
    //[BepInDependency(LethalLib.Plugin.ModGUID)]
    public class SpeedCheatBase : BaseUnityPlugin
    {
        //Mod declaration
        public const string modGUID = "DisableSpeedCheat";
        private const string modName = "DisableSpeedCheat";
        private const string modVersion = "1.0.0";

        //Mod initializers
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static SpeedCheatBase Instance;
        internal static ManualLogSource mls;

        //Config
        private static ConfigEntry<string> configPath;
        private static ConfigEntry<bool> configDisable;
        internal static string Binding;
        internal static bool Disabled
        {
            get => configDisable.Value;
            set => configDisable.Value = value;
        }

        void Awake()
        {
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            configPath = Config.Bind("Speed Cheat", "Binding", "delete", "The keybinding for the SpeedCheat HUD. Rebound to Delete + Shift. Vanilla is H + Shift.");
            configDisable = Config.Bind("Speed Cheat", "Disabled", false, "Prevent opening the Speed Cheat HUD entirely.");
            Binding = "<Keyboard>/" + configPath.Value.ToLower();
            harmony.PatchAll(typeof(SpeedCheatBase));
            harmony.PatchAll(typeof(SpeedCheatPatch));
        }
    }
}