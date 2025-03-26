using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using STFixes.Patches;
using UnityEngine;

namespace STFixes
{
    [BepInPlugin(modGUID, modName, modVersion)]
    //[BepInDependency(LethalLib.Plugin.ModGUID)]
    public class STFixModBase : BaseUnityPlugin
    {
        public const string modGUID = "SpikeTrapFixes";
        private const string modName = "SpikeTrapFixes";
        private const string modVersion = "1.2.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        internal ConfigControl Configuration;
        internal static STFixModBase Instance;
        internal static ManualLogSource mls;
        internal static AudioClip soundFix;

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
            string[] file = Directory.GetFiles(Path.GetDirectoryName(Info.Location), "spiketrapfix.bundle");
            if (file != null && file.Length > 0) {
                if (file[0] != null) {
                    AssetBundle currentAsset = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), file[0]));
                    if (currentAsset != null) soundFix = currentAsset.LoadAllAssets<AudioClip>()[0];
                    else currentAsset.Unload(true);
                }
            }
            if (soundFix != null) mls.LogInfo($"Spike trap audio fix loaded.");
            else mls.LogWarning($"Spike trap audio fix not found.");
            Configuration = new ConfigControl(Config);
            harmony.PatchAll(typeof(STFixModBase));
            harmony.PatchAll(typeof(SpikeRoofTrapPatch));
            harmony.PatchAll(typeof(ConfigControl));
            mls.LogInfo($"Spike Trap Fixes was loaded.");
        }
    }
}