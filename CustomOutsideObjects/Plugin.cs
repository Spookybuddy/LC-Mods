using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using CustomOutsideObjects.Patches;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LethalLib.Extras;

namespace CustomOutsideObjects
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class CustomOutsideModBase : BaseUnityPlugin
    {
        //Mod declaration
        public const string modGUID = "CustomOutsideObjects";
        private const string modName = "CustomOutsideObjects";
        private const string modVersion = "1.0.1";

        //Mod initializers
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static CustomOutsideModBase Instance;
        internal static ManualLogSource mls;
        internal ConfigControl Configuration;

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

        //Mod internal variables
        internal static bool Set_Experimentation;
        internal static bool Set_Assurance;
        internal static bool Set_Vow;
        internal static bool Set_Offense;
        internal static bool Set_March;
        internal static bool Set_Adamance;
        internal static bool Set_Rend;
        internal static bool Set_Dine;
        internal static bool Set_Titan;
        internal static bool Set_Embrion;
        internal static bool Set_Artifice;

        void Awake()
        {
            //Initialize mod; Find all .coo files, finding any Spawnable Outside Objects. Then check for breakability. Then add to list
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            foundOutsideAssetFiles = Directory.GetFiles(Path.GetDirectoryName(Info.Location), "*.coo");
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

            //Find all custom moons by locating the parent plugins folder. All .lethalbundle files are found, and if there is X & Xscene(s), add that as custom moon
            string location = Path.GetDirectoryName(Info.Location).ToString();
            string[] files = location.Split('\\');
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
                                else if (files[l].Replace(files[k], "").Equals("scenes")) customMoonList.Add(files[k]);
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
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            mls.LogInfo($"Outside Objects loaded.");
        }
    }
}