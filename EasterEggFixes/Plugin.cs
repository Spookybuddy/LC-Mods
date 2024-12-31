using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using EggFixes.Patches;

namespace EggFixes
{
    [BepInPlugin(modGUID, modName, modVersion)]
    //[BepInDependency(LethalLib.Plugin.ModGUID)]
    public class EasterEggFixesModBase : BaseUnityPlugin
    {
        public const string modGUID = "EasterEggFixes";
        private const string modName = "EasterEggFixes";
        private const string modVersion = "2.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);
        internal ConfigControl Configuration;

        internal static EasterEggFixesModBase Instance;
        internal static ManualLogSource mls;

        internal enum EggSettings
        {
            ExplodeOnThrow,
            AlwaysExplode,
            NeverExplode,
            ChanceToExplode
        }

        void Awake()
        {
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            Configuration = new ConfigControl(Config);
            harmony.PatchAll(typeof(EasterEggFixesModBase));
            harmony.PatchAll(typeof(StunGrenadeItemPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            mls.LogInfo($"Easter Egg Fixes was loaded.");
        }
    }
}