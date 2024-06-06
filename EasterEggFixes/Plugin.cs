using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using EggFixes.Patches;

namespace EggFixes
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class EasterEggFixesModBase : BaseUnityPlugin
    {
        public const string modGUID = "EasterEggFixes";
        private const string modName = "EasterEggFixes";
        private const string modVersion = "1.0.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static EasterEggFixesModBase Instance;
        internal static ManualLogSource mls;

        void Awake()
        {
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            harmony.PatchAll(typeof(EasterEggFixesModBase));
            harmony.PatchAll(typeof(StunGrenadeItemPatch));
            mls.LogInfo($"Easter Egg Fixes was loaded.");
        }
    }
}