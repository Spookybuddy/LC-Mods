using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MaskedRagdoll.Patches;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace MaskedRagdoll
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class RagdollModBase : BaseUnityPlugin
    {
        public const string modGUID = "MaskedRagdolls";
        private const string modName = "MaskedRagdoll";
        private const string modVersion = "0.9.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        internal static RagdollModBase Instance;
        internal static ManualLogSource mls;
        internal Config Configuration;

        internal static AssetBundle currentAsset;
        internal static GameObject[] ragdolls;
        internal static AudioClip[] soundClips;
        internal static bool foundRagdolls;
        internal Vector3 lastExplodePos;

        void Awake()
        {
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            //Find custom ragdolls for masked deaths
            mls.LogInfo($"Searching for maskragdoll.bundle...");
            string[] ragdollFile = Directory.GetFiles(Path.GetDirectoryName(Info.Location), "maskragdoll.bundle");
            foundRagdolls = false;
            if (ragdollFile != null && ragdollFile.Length > 0) {
                if (ragdollFile[0] != null) {
                    currentAsset = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), ragdollFile[0]));
                    if (currentAsset != null) {
                        GameObject[] allObjects = currentAsset.LoadAllAssets<GameObject>();
                        List<GameObject> valid = new List<GameObject>();
                        for (int i = 0; i < allObjects.Length; i++) {
                            if ((allObjects[i].name.ToLower().Split('_'))[0].Equals("ragdoll")) valid.Add(allObjects[i]);
                        }
                        ragdolls = valid.ToArray();
                        soundClips = currentAsset.LoadAllAssets<AudioClip>();
                        foundRagdolls = true;
                    }
                }
            }

            //No bundle, no mod. Don't care to implement base game ragdolls now that the bundle includes much more information
            if (!foundRagdolls) {
                mls.LogError($"Could not load custom ragdolls, disabling mod!");
                return;
            }

            Configuration = new Config(Config);
            lastExplodePos = new Vector3();

            harmony.PatchAll(typeof(RagdollModBase));
            harmony.PatchAll(typeof(AICollisionPatch));
            harmony.PatchAll(typeof(ForestGiantPatch));
            harmony.PatchAll(typeof(SpikeTrapPatch));
            harmony.PatchAll(typeof(LandminePatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
            harmony.PatchAll(typeof(VehiclePatch));
            harmony.PatchAll(typeof(MaskedPatch));
            harmony.PatchAll(typeof(TurretPatch));
            harmony.PatchAll(typeof(Config));

            mls.LogInfo($"Masked Ragdolls loaded.");
        }
    }
}