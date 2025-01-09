using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MapImprovements.Patches;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace MapImprovements
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLevelLoader.Plugin.ModGUID)]
    public class MapImprovementModBase : BaseUnityPlugin
    {
        //Mod declaration
        public const string modGUID = "MapImprovements";
        private const string modName = "MapImprovements";
        private const string modVersion = "0.9.3";

        //Mod initializers
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static MapImprovementModBase Instance;
        internal static ManualLogSource mls;
        internal static ConfigControl Configuration;

        //Mod variables
        internal static string[] foundOutsideAssetFiles;
        internal static AssetBundle currentAsset;
        internal static GameObject[] currentAssetObjects;
        internal static TextAsset[] currentInstructions;
        internal ReverbPreset[] reverbAssets;
        internal static readonly string[] ReverbNames = new string[] { "Alley", "BigCanyon", "Cave", "ConcreteTunnel", "Elevator", "LargeRoom", "NoReverb", "Outside1", "OutsideForest", "OutsideSnow", "SmallRoom" };

        //Internal moon vars
        internal List<Collection> Moons = new List<Collection>() {
            new Collection(new List<MapData>(), "experimentation"),
            new Collection(new List<MapData>(), "assurance"),
            new Collection(new List<MapData>(), "vow"),
            new Collection(new List<MapData>(), "offense"),
            new Collection(new List<MapData>(), "march"),
            new Collection(new List<MapData>(), "adamance"),
            new Collection(new List<MapData>(), "rend"),
            new Collection(new List<MapData>(), "dine"),
            new Collection(new List<MapData>(), "titan"),
            new Collection(new List<MapData>(), "artifice"),
            new Collection(new List<MapData>(), "embrion"),
            new Collection(new List<MapData>(), "gordion")
        };

        internal struct Collection
        {
            internal List<MapData> Adjustments;
            internal string Planet;
            public Collection(List<MapData> A, string name)
            {
                Adjustments = A;
                Planet = name;
            }
        }

        //The object to spawn and any edits to the map as well
        internal struct MapData
        {
            internal GameObject Object;
            internal List<Edits> Edit;
            internal string Description;

            //Construction
            public MapData(GameObject O, string D = default)
            {
                Object = O;
                Edit = new List<Edits>();
                Description = D;
            }
        }

        //Find objects by tag an name, and then what to edit about it
        internal struct Edits
        {
            internal string Name;
            internal string Tag;
            internal EditEnums Do;
            internal Vector3 Postion;
            internal Vector3 Rotation;
            internal Vector3 Scale;
            internal int FireExitIndex;
            internal bool Global;

            //Construction
            public Edits(string N, string T, EditEnums D, Vector3 P = default, Vector3 R = default, Vector3 S = default, bool G = false, int F = 0)
            {
                Name = N;
                Tag = T;
                Do = D;
                Postion = P;
                Rotation = R;
                Scale = S;
                FireExitIndex = F;
                Global = G;
            }
        }

        //The ways you can edit the gameobjects
        internal enum EditEnums
        {
            Move,
            Rotate,
            Scale,
            AllTransforms,
            Destroy,
            FireExit,
            Clone,
            Enable,
            Disable,
            Water,
            Reverb,
            StoryLog,
            Bridge,
            HasTrees
        }

        void Awake()
        {
            //Find the base mapimprovements, then look for any other X improvements.bundle
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            //Search all directories
            string location = Path.GetDirectoryName(Info.Location).ToString();
            string[] files = location.Split('\\');
            for (int c = files.Length - 1; c > 0; c--) {
                if (files[c].Equals("plugins")) {
                    for (int j = 0; j < files.Length - c - 1; j++) location = Directory.GetParent(location).ToString();
                    foundOutsideAssetFiles = Directory.GetFiles(location, "*improvements.bundle", SearchOption.AllDirectories);
                    break;
                }
            }

            //Parse moon improvements
            if (foundOutsideAssetFiles != null && foundOutsideAssetFiles.Length > 0) {
                for (int i = 0; i < foundOutsideAssetFiles.Length; i++) {
                    mls.LogInfo($"Found {foundOutsideAssetFiles[i]}");
                    currentAsset = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), foundOutsideAssetFiles[i]));
                    if (currentAsset != null) {
                        currentAssetObjects = currentAsset.LoadAllAssets<GameObject>();
                        currentInstructions = currentAsset.LoadAllAssets<TextAsset>();
                        string[] map = foundOutsideAssetFiles[i].ToLower().Split(new[] { "\\" }, System.StringSplitOptions.RemoveEmptyEntries);
                        map[0] = map[map.Length - 1].ToLower().Split(new[] { "improvements.bundle" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
                        //Reverb assets only from base bundle
                        if (map[0].Equals("map")) {
                            reverbAssets = currentAsset.LoadAllAssets<ReverbPreset>();
                            for (int o = 0; o < reverbAssets.Length; o++) mls.LogError($"#{o}:{reverbAssets[o].name}");
                        }
                        if (currentAssetObjects != null && currentAssetObjects.Length > 0) {
                            for (int objects = 0; objects < currentAssetObjects.Length; objects++) {
                                string[] parse = currentAssetObjects[objects].name.ToLower().Split(new[] { "_", " " }, System.StringSplitOptions.RemoveEmptyEntries);
                                int index;
                                MapData data = new MapData();
                                List<Edits> adjustments = new List<Edits>();
                                string configDescrip = "";
                                //Find the index/exists of each moon and add to that item
                                //ADD IN FORCED NAV REBAKE TO COVER FOR WHEN NO HAZARDS SPAWN
                                switch (parse[0]) {
                                    case "experimentation":
                                        index = 0;
                                        if (parse[1].Equals("b")) {

                                        } else if (parse[1].Equals("c")) {

                                        } else {

                                        }
                                        break;
                                    case "assurance":
                                        index = 1;
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("OutsideNode (22)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(-0.85f, 9.147f, 76.25f), F: 2));
                                            configDescrip = "Adds in a new Fire Exit and more environmental detailing.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("rock.012 (1)", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("rock.007", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("RockSingle4", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("rock.001 (1)", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideAINode (29)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("RockObstacles/NavObs (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("RockObstacles/NavObs (6)", "Untagged", EditEnums.Destroy));
                                            configDescrip = "Removes rocks to create a path for the Cruiser to get to the Main Entrance.";
                                        } else {
                                            adjustments.Add(new Edits("Cube", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ReverbTriggers (1)/Cube", "Untagged", EditEnums.Move, new Vector3(-274.25f, 7.5f, -76)));
                                            adjustments.Add(new Edits("ReverbTriggers (1)/Cube (16)", "Untagged", EditEnums.AllTransforms, new Vector3(-265, 16, -83), default, new Vector3(0.5f, 36, 50)));
                                            adjustments.Add(new Edits("ReverbTriggers (1)/Cube (1)", "Untagged", EditEnums.AllTransforms, new Vector3(-268.5f, 10, -80), default, new Vector3(0.5f, 16, 40)));
                                            adjustments.Add(new Edits("Ladder1.5x", "Untagged", EditEnums.Clone, new Vector3(142.6f, -0.9f, 75.85f), new Vector3(0, 108, 0), new Vector3(0.6f, 1.2f, 0.6f)));
                                            configDescrip = "Modifies the Main Entrance buidling, allowing access to the pipe to the Fire Exit through a bit of parkour.";
                                        }
                                        break;
                                    case "vow":
                                        index = 2;
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("DangerousBridge", "Untagged", EditEnums.Clone, new Vector3(-68.25f, -9.7f, 104.25f), new Vector3(1.75f, 159.25f, 1.1f), new Vector3(0.81f, 0.81f, 0.82f)));
                                            adjustments.Add(new Edits("WaterDam", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("WaterBig", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("WaterTriggers", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("BoundsWalls/Cube (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("BoundsWalls/Cube (5)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("FireExitDoorContainer", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("StoryLogCollectable (3)", "InteractTrigger", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (97)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (98)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (99)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.AllTransforms, new Vector3(93.2f, -7, 162.15f), Vector3.zero));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDescrip = "The river has dried up, leaving behind a valley with trees and rocks. The dam has been replaced with another breakable bridge, and the Fire Exit has been moved to the right end of the Facility.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(119.9f, -29.5f, 37.65f), new Vector3(0, 180, 0), F: 2));
                                            adjustments.Add(new Edits("OutsideNode (55)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (54)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (44)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (32)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (31)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (26)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("tree.002_LOD0 (30)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("tree.003_LOD0 (26)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("tree.003_LOD0 (25)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDescrip = "Adds in a new facility building right behind the ship, with a Fire Exit that has been flooded.";
                                        } else {
                                            adjustments.Add(new Edits("ChainlinkFence (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EnterAlley", "Untagged", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("ExitAlley", "Untagged", EditEnums.Reverb, F: 9));
                                            adjustments.Add(new Edits("EnterGrove", "Untagged", EditEnums.Reverb, F: 6));
                                            adjustments.Add(new Edits("ExitTop", "Untagged", EditEnums.Reverb, F: 9));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(12.75f, 8, 211.25f), new Vector3(0, 180, 0), F : 2));
                                            configDescrip = "Expands the Facility building, adding in a new area with Fire Exit through the alleyway.";
                                        }
                                        break;
                                    case "offense":
                                        index = 3;
                                        if (parse[1].Equals("b")) {

                                        } else if (parse[1].Equals("c")) {

                                        } else {

                                        }
                                        break;
                                    case "march":
                                        index = 4;
                                        if (parse[1].Equals("b")) {

                                        } else if (parse[1].Equals("c")) {

                                        } else {

                                        }
                                        break;
                                    case "adamance":
                                        index = 5;
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (4)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (5)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (7)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (8)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (9)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (11)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (12)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Cube.002", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.AllTransforms, new Vector3(-75.9f, -2.4f, -112.35f), new Vector3(0, 223, 0)));
                                            configDescrip = "Adds in easier pathing to and from the Fire Exit while also adjusting it slightly.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (4)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(56.8f, 13.1f, -92.3f), new Vector3(0, -220, 0), F: 2));
                                            configDescrip = "Adds in a new Fire Exit and environmental detailing on the left side of the ship landing area.";
                                        } else {
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (4)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDescrip = "Adds in unique mineshaft environmental details to differentiate it from other forest moons.";
                                        }
                                        break;
                                    case "rend":
                                        index = 6;
                                        if (parse[1].Equals("b")) {

                                        } else if (parse[1].Equals("c")) {

                                        } else {

                                        }
                                        break;
                                    case "dine":
                                        index = 7;
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("treeLeafless.003_LOD0 (39)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeafless.003_LOD0 (41)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(-172.7f, 7.4f, -15.98f), new Vector3(0, -96, 0), G: true, F: 2));
                                            configDescrip = "Expands on the facility building, adding a new fire exit on top, with additional fences and pipes.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(86.7f, 6.2f, -19.85f), new Vector3(0, -265.5f, 0), G: true, F: 2));
                                            configDescrip = "Adds in a new Fire Exit only accessible via jumping off the ship early, similar to Offense.";
                                        } else {
                                            adjustments.Add(new Edits("EntranceTeleportA", "InteractTrigger", EditEnums.AllTransforms, new Vector3(-122.03f, -13.55f, -7), new Vector3(0, 90, 0), G: true));
                                            adjustments.Add(new Edits("DoorFrame (1)", "Untagged", EditEnums.AllTransforms, new Vector3(-122.04f, -16.2f, -6.83f), new Vector3(-90, 180, -89.2f), G: true));
                                            adjustments.Add(new Edits("SteelDoorFake", "Untagged", EditEnums.AllTransforms, new Vector3(-123.55f, -12.71f, -6.85f), new Vector3(-90, 180, -89.2f), G: true));
                                            adjustments.Add(new Edits("SteelDoorFake (1)", "Untagged", EditEnums.AllTransforms, new Vector3(-120.64f, -12.71f, -6.89f), new Vector3(-90, 180, -89.2f), G: true));
                                            adjustments.Add(new Edits("Environment/Plane", "Untagged", EditEnums.AllTransforms, new Vector3(-122f, -12.8f, -6.87f), new Vector3(270, 0, 0), G: true));
                                            adjustments.Add(new Edits("ScanNode", "Untagged", EditEnums.AllTransforms, new Vector3(-123f, -11f, -12f), new Vector3(-180, 110, 90), new Vector3(25, 50, 33), G: true));
                                            adjustments.Add(new Edits("NeonLightsSingle", "PoweredLight", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Cube.002", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFence", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFence (1)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFence (2)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFence (3)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFenceHoleModifier", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFenceBend", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Environment/Map/Collider", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Environment/Map/Collider (1)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("CliffJump (2)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("InteriorReverb", "Untagged", EditEnums.Reverb, F: 5));
                                            adjustments.Add(new Edits("ExitReverb", "Untagged", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("ExitReverb (1)", "Untagged", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDescrip = "Adds in fences around the edges, with holes to allow for escaping Giants. Adjusts the main entrance area to prevent Giants loitering.";
                                        }
                                        break;
                                    case "titan":
                                        index = 8;
                                        if (parse[1].Equals("b")) {

                                        } else if (parse[1].Equals("c")) {

                                        } else {

                                        }
                                        break;
                                    case "artifice":
                                        index = 9;
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("ItemShipAnimContainer", "Untagged", EditEnums.AllTransforms, new Vector3(72, 2.75f, -62.5f), new Vector3(-90, 45, -50)));
                                            adjustments.Add(new Edits("treeLeafless", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeafless (4)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (97)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (95)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (94)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (93)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (92)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (90)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (8)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (7)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (6)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeafless.002_LOD0 (3)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("SteelDoorMapModel (4)", "Untagged", EditEnums.Clone, new Vector3(12.25f, 4.5f, -11.21f), new Vector3(180, -92, -180), new Vector3(1, 1.05f, 1)));
                                            adjustments.Add(new Edits("FogExclusionZone (2)", "Untagged", EditEnums.Clone, new Vector3(30.5f, 14, -0.75f)));
                                            adjustments.Add(new Edits("BuildingAmbience", "Untagged", EditEnums.Clone, new Vector3(46.25f, 7, -73), new Vector3(180, 0, 180)));
                                            adjustments.Add(new Edits("BuildingAmbience (7)", "Untagged", EditEnums.Clone, new Vector3(10.75f, 3, -84)));
                                            adjustments.Add(new Edits("InsideAmbience (1)", "Untagged", EditEnums.Clone, new Vector3(47.5f, 7, -73)));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDescrip = "Adds in a warehouse and platform for the ship to land in.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(14, 157.4f, -274.9f), new Vector3(180, 26, 180), F: 2));
                                            adjustments.Add(new Edits("OuterFence/ChainlinkFence (57)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Colliders/ChainlinkFence (45)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Colliders/Cube", "Untagged", EditEnums.Move, new Vector3(126, 1.4f, -69.5f)));
                                            adjustments.Add(new Edits("BuildingAmbience (3)", "Untagged", EditEnums.Clone, new Vector3(140, 7, -79), new Vector3(180, 0, 180), new Vector3(0.5f, 10, 20)));
                                            adjustments.Add(new Edits("OutsideAmbience (1)", "Untagged", EditEnums.Clone, new Vector3(138, 7, -79)));
                                            adjustments.Add(new Edits("Ocean", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("Ocean (1)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("Ocean (2)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("Ocean (3)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("Ocean (4)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("WaterTrigger", "Untagged", EditEnums.Water));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDescrip = "Adds in large bodies of water and dams.";
                                        } else {
                                            adjustments.Add(new Edits("ItemShipAnimContainer", "Untagged", EditEnums.AllTransforms, new Vector3(82, -4.66f, -98.5f), new Vector3(-86, 28, -43)));
                                            adjustments.Add(new Edits("OutsideAmbience (3)", "Untagged", EditEnums.Clone, new Vector3(55, 2, -187)));
                                            adjustments.Add(new Edits("OutsideAmbience (4)", "Untagged", EditEnums.Clone, new Vector3(14, -1, -198)));
                                            adjustments.Add(new Edits("BuildingAmbience (7)", "Untagged", EditEnums.Clone, new Vector3(15.25f, -1, -198)));
                                            adjustments.Add(new Edits("BuildingAmbience (6)", "Untagged", EditEnums.Clone, new Vector3(53.5f, 3, -188)));
                                            adjustments.Add(new Edits("GarageDoorContainer (2)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("GreyRockGrouping2 (6)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Colliders/Cube (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OuterFence/ChainlinkFence (65)", "Untagged", EditEnums.Destroy));
                                            configDescrip = "Allows access to the 4th warehouse. Adjusts the dropship position. Adds in more nodes for AI pathfinding.";
                                        }
                                        break;
                                    case "embrion":
                                        index = 10;
                                        if (parse[1].Equals("b")) {

                                        } else if (parse[1].Equals("c")) {

                                        } else {

                                        }
                                        break;
                                    case "company":
                                        index = 11;
                                        if (parse[2].Equals("b")) {
                                            adjustments.Add(new Edits("ShippingContainer", "Aluminum", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ShippingContainer (3)", "Aluminum", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ShippingContainer (5)", "Aluminum", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ShippingContainer (6)", "Aluminum", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Puddle2", "Untagged", EditEnums.Clone, new Vector3(-11, 3.853f, 55.5f), new Vector3(0, 55, 0), new Vector3(3, 2.25f, 2.5f)));
                                            configDescrip = "Removes some shipping containers for easy driving of the Company Cruiser.";
                                        } else if (parse[2].Equals("c")) {
                                            configDescrip = "Secret drill :)";
                                        } else {
                                            adjustments.Add(new Edits("Puddle", "Untagged", EditEnums.Clone, new Vector3(-9, 3.85f, -74.5f), new Vector3(-180, -41, 180), new Vector3(-1.5f, 1.5f, 1.5f)));
                                            adjustments.Add(new Edits("Exit (1)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (2)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (3)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (4)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (5)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (6)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (7)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (8)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Enter (1)", "Grass", EditEnums.Reverb, F: 3));
                                            adjustments.Add(new Edits("Enter (2)", "Grass", EditEnums.Reverb, F: 3));
                                            adjustments.Add(new Edits("Enter (3)", "Grass", EditEnums.Reverb, F: 3));
                                            adjustments.Add(new Edits("Enter (4)", "Grass", EditEnums.Reverb, F: 3));
                                            adjustments.Add(new Edits("Enter (5)", "Grass", EditEnums.Reverb, F: 3));
                                            adjustments.Add(new Edits("Enter (6)", "Grass", EditEnums.Reverb, F: 3));
                                            adjustments.Add(new Edits("Enter (7)", "Grass", EditEnums.Reverb, F: 3));
                                            //Casino compatibility: search for specific files & destroy crate if found
                                            location = Path.GetDirectoryName(Info.Location).ToString();
                                            files = location.Split('\\');
                                            for (int c = files.Length - 1; c > 0; c--) {
                                                if (files[c].Equals("plugins")) {
                                                    for (int j = 0; j < files.Length - c - 1; j++) location = Directory.GetParent(location).ToString();
                                                    files = Directory.GetFiles(location, "mrgrm7.LethalCasino.dll", SearchOption.AllDirectories);
                                                    if (files.Length > 0 && files != null) {
                                                        files = Directory.GetFiles(location, "lethalcasinoassets", SearchOption.AllDirectories);
                                                        if (files.Length > 0 && files != null) {
                                                            mls.LogWarning($"Let's go gambling! Casino mod found, modifying company.");
                                                            adjustments.Add(new Edits("ShippingCasino", "Aluminum", EditEnums.Destroy));
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                            configDescrip = "Recieves more shipments, with some of them being open. Great for playing hide and seek with friends, while also limiting the area with walls.";
                                        }
                                        break;
                                    default:
                                        //Find objects with the same name as the beginning of the file
                                        map = foundOutsideAssetFiles[i].ToLower().Split(new[] { "\\" }, System.StringSplitOptions.RemoveEmptyEntries);
                                        map[0] = map[map.Length-1].ToLower().Split(new[] { "improvements.bundle" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
                                        if (map[0].Equals(parse[0])) {
                                            index = Moons.FindIndex(x => x.Planet.Equals(map[0]));
                                            if (index < 0) {
                                                Moons.Add(new Collection(new List<MapData>(), map[0]));
                                                index = Moons.FindIndex(x => x.Planet.Equals(map[0]));
                                                mls.LogInfo($"Created new collection for {map[0]}.");
                                            }
                                            for (int j = 0; j < currentInstructions.Length; j++) {
                                                if (currentAssetObjects[objects].name.ToLower().Equals(currentInstructions[j].name.ToLower())) {
                                                    mls.LogWarning($"Found instructions for {parse[0]}");
                                                    map = currentInstructions[j].text.Split(new[] { "\n" }, System.StringSplitOptions.None);
                                                    int on = 0;
                                                    string obj = "";
                                                    string tag = "";
                                                    EditEnums edit = EditEnums.Disable;
                                                    Vector3 pos = default;
                                                    Vector3 rot = default;
                                                    Vector3 scl = default;
                                                    bool glo = false;
                                                    int fir = 0;
                                                    configDescrip = $"Improvements for the modded moon {parse[0]}";
                                                    for (int k = 0; k < map.Length; k++) {
                                                        if (map[k].IsNullOrWhiteSpace()) {
                                                            adjustments.Add(new Edits(obj, tag, edit, pos, rot, scl, glo, fir));
                                                            on = 0;
                                                            obj = "";
                                                            tag = "";
                                                            edit = EditEnums.Disable;
                                                            pos = default;
                                                            rot = default;
                                                            scl = default;
                                                            glo = false;
                                                            fir = 0;
                                                        } else {
                                                            //Check for description to exit
                                                            if (map[k][0].Equals('#')) {
                                                                configDescrip = map[k].Substring(1);
                                                                mls.LogInfo("Found config description, ending parsing.");
                                                                break;
                                                            }
                                                            map[k] = map[k].Trim();
                                                            switch (on) {
                                                                case 0:
                                                                    obj = map[k];
                                                                    break;
                                                                case 1:
                                                                    tag = map[k];
                                                                    break;
                                                                case 2:
                                                                    string lower = map[k].ToLower();
                                                                    switch (lower) {
                                                                        case "move":
                                                                            edit = EditEnums.Move;
                                                                            break;
                                                                        case "rotate":
                                                                            edit = EditEnums.Rotate;
                                                                            break;
                                                                        case "scale":
                                                                            edit = EditEnums.Scale;
                                                                            break;
                                                                        case "transform":
                                                                        case "transforms":
                                                                        case "alltransforms":
                                                                            edit = EditEnums.AllTransforms;
                                                                            break;
                                                                        case "destroy":
                                                                            edit = EditEnums.Destroy;
                                                                            break;
                                                                        case "fire":
                                                                        case "fireexit":
                                                                            edit = EditEnums.FireExit;
                                                                            break;
                                                                        case "clone":
                                                                            edit = EditEnums.Clone;
                                                                            break;
                                                                        case "enable":
                                                                            edit = EditEnums.Enable;
                                                                            break;
                                                                        case "disable":
                                                                            edit = EditEnums.Disable;
                                                                            break;
                                                                        case "water":
                                                                            edit = EditEnums.Water;
                                                                            break;
                                                                        case "reverb":
                                                                        case "reverbtrigger":
                                                                            edit = EditEnums.Reverb;
                                                                            break;
                                                                        case "log":
                                                                        case "storylog":
                                                                            edit = EditEnums.StoryLog;
                                                                            break;
                                                                        case "bridge":
                                                                            edit = EditEnums.Bridge;
                                                                            break;
                                                                        case "trees":
                                                                        case "hastrees":
                                                                            edit = EditEnums.HasTrees;
                                                                            break;
                                                                        default:
                                                                            mls.LogError("Unable to parse Edit Enum. Please make sure things are formatted properly.");
                                                                            edit = EditEnums.Disable;
                                                                            break;
                                                                    }
                                                                    break;
                                                                case 3:
                                                                    switch (edit) {
                                                                        case EditEnums.Move:
                                                                            pos = StringToVector3(map[k]);
                                                                            break;
                                                                        case EditEnums.Rotate:
                                                                            rot = StringToVector3(map[k]);
                                                                            break;
                                                                        case EditEnums.Scale:
                                                                            scl = StringToVector3(map[k]);
                                                                            break;
                                                                        default:
                                                                            if (int.TryParse(map[k], out int z)) fir = z;
                                                                            break;
                                                                    }
                                                                    break;
                                                                case 4:
                                                                    //Check for global transform overload
                                                                    if (StringToBoolean(map[k])) glo = true;
                                                                    rot = StringToVector3(map[k]);
                                                                    break;
                                                                case 5:
                                                                    if (StringToBoolean(map[k])) glo = true;
                                                                    scl = StringToVector3(map[k]);
                                                                    break;
                                                                case 6:
                                                                    if (StringToBoolean(map[k])) glo = true;
                                                                    if (int.TryParse(map[k], out int x)) fir = x;
                                                                    break;
                                                                case 7:
                                                                    if (StringToBoolean(map[k])) glo = true;
                                                                    if (int.TryParse(map[k], out int y)) fir = y;
                                                                    break;
                                                                default:
                                                                    mls.LogError("Error parsing instructions. Please make sure things are formatted properly.");
                                                                    on = 0;
                                                                    break;
                                                            }
                                                            on++;
                                                        }
                                                    }
                                                }
                                            }
                                        } else continue;
                                        break;
                                }
                                //Combine all the data into the adjustments
                                mls.LogInfo($"{parse[0]} improvements found!");
                                data.Object = currentAssetObjects[objects];
                                data.Description = configDescrip;
                                data.Edit = adjustments;
                                Moons[index].Adjustments.Add(data);
                            }
                        } else mls.LogError($"No Objects found in {foundOutsideAssetFiles[i]}.");
                    } else mls.LogError($"Failed to load {foundOutsideAssetFiles[i]}.");
                }
            } else {
                //.dll is installed, but no *improvements.bundle files are included
                mls.LogWarning($"No Map Improvements were found.");
                return;
            }
            harmony.PatchAll(typeof(MapImprovementModBase));
            harmony.PatchAll(typeof(RoundManagerPatch));
            Configuration = new ConfigControl(Config);
            mls.LogInfo($"Maps Improved.");
        }

        //Converts input string into a vector3. Only reads for the first 3 values, sets to 0 if not enough input is given
        internal static Vector3 StringToVector3(string parse)
        {
            string[] values = parse.Split(',');
            float[] v = new float[3] { 0, 0, 0 };
            for (int a = 0; a < values.Length; a++) {
                if (a > 2) break;
                values[a].Trim();
                if (float.TryParse(values[a], out float res)) v[a] = res;
            }
            return new Vector3(v[0], v[1], v[2]);
        }

        //Get boolean from string
        internal static bool StringToBoolean(string parse)
        {
            parse = parse.ToLower();
            switch (parse) {
                case "global": return true;
                case "local": return false;
                case "true": return true;
                case "false": return false;
                default: return false;
            }
        }
    }
}