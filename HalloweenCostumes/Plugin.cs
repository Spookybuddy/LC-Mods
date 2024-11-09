using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Costumes.Patches;
using System.IO;
using UnityEngine;

namespace Costumes
{
    [BepInPlugin(modGUID, modName, modVersion)]
    //[BepInDependency(LethalLib.Plugin.ModGUID)]
    public class CostumesModBase : BaseUnityPlugin
    {
        public const string modGUID = "EnemyHalloweenCostumes";
        private const string modName = "EnemyHalloweenCostumes";
        private const string modVersion = "1.0.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static CostumesModBase Instance;
        internal static ManualLogSource mls;
        internal static GameObject[] costumes;
        public static ConfigEntry<bool> Lootbug;
        public static ConfigEntry<bool> Tulipsnake;

        void Awake()
        {
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            string file = Directory.GetFiles(Path.GetDirectoryName(Info.Location), "costumes.bundle")[0];
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), file));
            if (bundle != null) {
                costumes = bundle.LoadAllAssets<GameObject>();
                if (costumes == null || costumes.Length < 1) {
                    mls.LogError($"Costumes failed to load from bundle!");
                    return;
                } else for (int i = 0; i < costumes.Length; i++) mls.LogInfo($"Found {costumes[i].name}");
            } else {
                mls.LogError($"Costumes not found!");
                return;
            }
            //Successful loading: begin config & patching
            Lootbug = Config.Bind("Enemy Halloween Costumes", "Hoarding Bug Ghost", true, "The Hoarding Bugs put on their ghost costume.");
            Tulipsnake = Config.Bind("Enemy Halloween Costumes", "Tulipsnake Pumpkin", true, "The Tulipsnakes put on their pumpkin head costume.");
            harmony.PatchAll(typeof(CostumesModBase));
            harmony.PatchAll(typeof(LootbugPatch));
            harmony.PatchAll(typeof(TulipPatch));
            mls.LogInfo($"Spooky season is back!");
        }
    }
}