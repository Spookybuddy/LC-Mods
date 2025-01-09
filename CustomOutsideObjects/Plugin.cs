using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using CustomOutsideObjects.Patches;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LethalLib.Extras;
using LethalLevelLoader;

namespace CustomOutsideObjects
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    [BepInDependency(LethalLevelLoader.Plugin.ModGUID)]
    public class CustomOutsideModBase : BaseUnityPlugin
    {
        //Mod declaration
        public const string modGUID = "CustomOutsideObjects";
        private const string modName = "CustomOutsideObjects";
        private const string modVersion = "1.2.0";

        //Mod initializers
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static CustomOutsideModBase Instance;
        internal static ManualLogSource mls;
        internal static ConfigControl Configuration;

        //Mod variables
        internal static string[] foundOutsideAssetFiles;
        internal static AssetBundle currentAsset;
        internal static SpawnableOutsideObjectDef[] currentAssetObjects;
        internal static List<SpawnableOutsideObjectWithRarity> loadedInjectableOutsideObjects = new List<SpawnableOutsideObjectWithRarity>();
        internal static List<string> customMoonList = new List<string>();

        //Tag search terms
        internal static readonly string[] tags = new string[] {
            "Gravel",
            "Grass",
            "Snow",
            "Rock",
            "Concrete",
            "Catwalk",
            "Wood"
        };

        void Awake()
        {
            //Initialize mod; Find all .coo files, finding any Spawnable Outside Objects. Then check for breakability. Then add to list
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            //Search all directories for *.coo files
            string location = Path.GetDirectoryName(Info.Location).ToString();
            string[] files = location.Split('\\');
            for (int c = files.Length - 1; c > 0; c--) {
                if (files[c].Equals("plugins")) {
                    for (int j = 0; j < files.Length - c - 1; j++) location = Directory.GetParent(location).ToString();
                    foundOutsideAssetFiles = Directory.GetFiles(location, "*.coo", SearchOption.AllDirectories);
                    break;
                }
            }

            //Nested so much for error catching: | No .coo files | Invalid asset bundle | No spawnable rarities | Spawnable rarity has no object
            if (foundOutsideAssetFiles != null && foundOutsideAssetFiles.Length > 0) {
                for (int x = 0; x < foundOutsideAssetFiles.Length; x++) {
                    currentAsset = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), foundOutsideAssetFiles[x]));
                    if (currentAsset != null) {
                        currentAssetObjects = currentAsset.LoadAllAssets<SpawnableOutsideObjectDef>();
                        if (currentAssetObjects != null && currentAssetObjects.Length > 0) {
                            GameObject[] prefabs = currentAsset.LoadAllAssets<GameObject>();
                            for (int objects = 0; objects < currentAssetObjects.Length; objects++) {
                                if (currentAssetObjects[objects].spawnableMapObject != null) {
                                    //Check for break collider in the object. Break collider will be given the custom break script, allowing for custom sounds and particles
                                    if (currentAssetObjects[objects].spawnableMapObject.spawnableObject.prefabToSpawn.layer.Equals(25)) {
                                        if (currentAssetObjects[objects].spawnableMapObject.spawnableObject.prefabToSpawn.transform.GetChild(0).name.Equals("BreakTrigger")) {
                                            currentAssetObjects[objects].spawnableMapObject.spawnableObject.prefabToSpawn.transform.GetChild(0).gameObject.AddComponent<TerrainObstacleTrigger>();
                                            string name = currentAssetObjects[objects].spawnableMapObject.spawnableObject.prefabToSpawn.name;
                                            if (currentAssetObjects[objects].spawnableMapObject.spawnableObject.prefabToSpawn.GetComponentInChildren<TerrainObstacleTrigger>() != null) mls.LogInfo($"Added generic break trigger script to {name}!");
                                            else mls.LogError($"Add generic break to {name} failed!");
                                        } else {
                                            BreakObject collide = currentAssetObjects[objects].spawnableMapObject.spawnableObject.prefabToSpawn.transform.GetChild(0).gameObject.AddComponent<BreakObject>();
                                            string parse = currentAssetObjects[objects].spawnableMapObject.spawnableObject.prefabToSpawn.name;
                                            for (int y = 0; y < prefabs.Length; y++) {
                                                if (prefabs[y].name.Contains(parse) && prefabs[y].name.Contains("Break")) collide.prefabs.Add(prefabs[y]);
                                            }
                                            if (currentAssetObjects[objects].spawnableMapObject.spawnableObject.prefabToSpawn.GetComponentInChildren<BreakObject>() != null) mls.LogInfo($"Added custom break trigger script to {parse}!");
                                            else mls.LogError($"Add custom break to {parse} failed!");

                                        }
                                    }
                                    loadedInjectableOutsideObjects.Add(currentAssetObjects[objects].spawnableMapObject);
                                    mls.LogInfo($"Custom Outside Object added: {currentAssetObjects[objects].spawnableMapObject.spawnableObject.name}");
                                } else mls.LogWarning($"Spawnable Rarity {currentAssetObjects[objects]} found, but no Spawnable Map Object is attached.");
                            }
                        } else mls.LogWarning($"No Spawnable Map Object found in {foundOutsideAssetFiles[x]}.coo.");
                    } else mls.LogError($"Failed to load {foundOutsideAssetFiles[x]}.coo.");
                }
            } else {
                //.dll is installed, but no .coo files are included
                mls.LogWarning($"No Custom Outside Objects were found.");
                return;
            }
            //If the injectable list is empty, exit
            if (loadedInjectableOutsideObjects.Count < 1 || loadedInjectableOutsideObjects == null) {
                mls.LogWarning($"No Custom Outside Objects were added.");
                return;
            }

            //Find all custom moons by locating the parent plugins folder. All .lethalbundle files are found, and if there is X & X()scene(s), add that as custom moon
            location = Path.GetDirectoryName(Info.Location).ToString();
            files = location.Split('\\');
            for (int i = files.Length - 1; i > 0; i--) {
                if (files[i].Equals("plugins")) {
                    for (int j = 0; j < files.Length - i - 1; j++) location = Directory.GetParent(location).ToString();
                    files = Directory.GetFiles(location, "*.lethalbundle", SearchOption.AllDirectories);
                    string[] paths = new string[files.Length];
                    for (int f = 0; f < files.Length; f++) {
                        paths[f] = files[f];
                        files[f] = Path.GetFileNameWithoutExtension(files[f]);
                    }
                    for (int k = 0; k < files.Length - 1; k++) {
                        for (int l = k + 1; l < files.Length; l++) {
                            if (files[l].Contains(files[k])) {
                                if (files[l].Replace(files[k], "").Equals("scene")) customMoonList.Add(files[k]);
                                else if (files[l].Replace(files[k], "").Equals(" scene")) customMoonList.Add(files[k]);
                                else if (files[l].Replace(files[k], "").Equals("scenes")) {
                                    //Catch for bundled moons
                                    AssetBundle ass = AssetBundle.LoadFromFile(paths[k]);
                                    ExtendedMod[] mod = ass.LoadAllAssets<ExtendedMod>();
                                    List<ExtendedLevel> extended = mod[0].ExtendedLevels;
                                    for (int z = 0; z < extended.Count; z++) {
                                        string[] level = extended[z].SelectableLevel.name.Split(new[] { "level", "selectable", "Level", "Selectable" }, System.StringSplitOptions.RemoveEmptyEntries);
                                        customMoonList.Add(level[0]);
                                    }
                                    ass.Unload(true);
                                }
                            } else {
                                //Catch for DemonMae's naming conventions
                                if (files[l].Replace(files[k].Replace("moon", ""), "").Equals("scene")) customMoonList.Add(files[k]);
                                //Catch for Zingar's naming conventions
                                else if (files[k].Replace(files[l], "").Equals(" scene")) customMoonList.Add(files[l]);
                                //Catch for GordionSaga
                                if (files[l].Replace(files[k].Replace("assets", ""), "").Equals("mod")) {
                                    AssetBundle ass = AssetBundle.LoadFromFile(paths[l]);
                                    ExtendedMod[] mod = ass.LoadAllAssets<ExtendedMod>();
                                    List<ExtendedLevel> extended = mod[0].ExtendedLevels;
                                    for (int z = 0; z < extended.Count; z++) {
                                        string[] level = extended[z].SelectableLevel.name.Split(new[] { "level", "selectable", "Level", "Selectable" }, System.StringSplitOptions.RemoveEmptyEntries);
                                        customMoonList.Add(level[0]);
                                    }
                                    ass.Unload(true);
                                }
                            }
                        }
                    }
                    break;
                }
            }
            for (int i = 0; i < customMoonList.Count; i++) mls.LogInfo($"Found custom moon {customMoonList[i]}.");

            //Generate config file for all objects found
            Configuration = new ConfigControl(Config);
            mls.LogInfo($"Generated Config file for all loaded objects.");
            harmony.PatchAll(typeof(CustomOutsideModBase));
            harmony.PatchAll(typeof(RoundManagerPatch));
            mls.LogInfo($"Outside Objects loaded.");
        }
    }
}