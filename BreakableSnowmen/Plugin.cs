using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Breakables.Patches;
using System.IO;
using UnityEngine;

namespace Breakables
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class BreakableSnowmenModBase : BaseUnityPlugin
    {
        //Mod declaration
        public const string modGUID = "BreakableSnowmen";
        private const string modName = "BreakableSnowmen";
        private const string modVersion = "1.0.0";

        //Mod initializers
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static BreakableSnowmenModBase Instance;
        internal static ManualLogSource mls;

        //Mod variables
        internal GameObject trigger;
        internal GameObject tallTrigger;

        void Awake()
        {
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            //Search whole plugins subfolders
            string location = Path.GetDirectoryName(Info.Location).ToString();
            string[] files = location.Split('\\');
            for (int c = files.Length - 1; c > 0; c--) {
                if (files[c].Equals("plugins")) {
                    for (int j = 0; j < files.Length - c - 1; j++) location = Directory.GetParent(location).ToString();
                    location = Directory.GetFiles(location, "breakablesnowmen.bundle", SearchOption.AllDirectories)[0];
                    break;
                }
            }
            //Load assets
            AssetBundle currentAsset = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), location));
            if (currentAsset != null) {
                GameObject[] prefabs = currentAsset.LoadAllAssets<GameObject>();
                for (int i = 0; i < prefabs.Length; i++) {
                    if (prefabs[i].CompareTag("Puddle")) continue;
                    BreakSnowman script = prefabs[i].AddComponent<BreakSnowman>();
                    for (int j = 0; j < prefabs.Length; j++) script.prefabs.Add(prefabs[j]);
                    if (prefabs[i].name.Equals("SnowmanTrigger")) trigger = prefabs[i];
                    if (prefabs[i].name.Equals("TallSnowmanTrigger")) tallTrigger = prefabs[i];
                }
            } else mls.LogError($"Failed to load breakablesnowmen.bundle");
            //Patch
            harmony.PatchAll(typeof(BreakableSnowmenModBase));
            harmony.PatchAll(typeof(RoundManagerPatch));
            mls.LogInfo($"Snowmen have become fragile :(");
        }
    }
}