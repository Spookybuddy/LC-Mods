using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using STFixes.Patches;

namespace STFixes
{
    [BepInPlugin(modGUID, modName, modVersion)]
    //[BepInDependency(LethalLib.Plugin.ModGUID)]
    public class STFixModBase : BaseUnityPlugin
    {
        public const string modGUID = "SpikeTrapFixes";
        private const string modName = "SpikeTrapFixes";
        private const string modVersion = "1.1.2";

        private readonly Harmony harmony = new Harmony(modGUID);
        internal ConfigControl Configuration;

        internal static STFixModBase Instance;
        internal static ManualLogSource mls;

        internal enum EnumOptions
        {
            IntervalOnly,
            DetectionOnly,
            Both
        }

        void Awake()
        {
            if (Instance == null) Instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            Configuration = new ConfigControl(Config);

            harmony.PatchAll(typeof(STFixModBase));
            harmony.PatchAll(typeof(SpikeRoofTrapPatch));
            harmony.PatchAll(typeof(ConfigControl));

            mls.LogInfo($"Spike Trap Fixes was loaded.");
        }
    }
}