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
        private const string modVersion = "0.9.8";

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
        internal Material WaterMat;
        internal bool Rebalanced = false;
        internal bool Chameleon = false;
        internal bool TonightWeDine = false;
        internal bool ArtificeBlizzard = false;

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
            internal ConfigControl.Setting Default;
            internal string Description;

            //Construction
            public MapData(GameObject O, ConfigControl.Setting C = default, string D = default)
            {
                Object = O;
                Edit = new List<Edits>();
                Default = C;
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
            internal Found If;

            //Construction
            public Edits(string N, string T, EditEnums D, Vector3 P = default, Vector3 R = default, Vector3 S = default, bool G = false, int F = 0, Found I = default)
            {
                Name = N;
                Tag = T;
                Do = D;
                Postion = P;
                Rotation = R;
                Scale = S;
                FireExitIndex = F;
                Global = G;
                If = I;
            }
        }

        internal struct Found
        {
            internal string Name;
            internal string Tag;
            internal EditEnums Do;

            public Found(string N, string T, EditEnums D)
            {
                Name = N;
                Tag = T;
                Do = D;
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
            HasTrees,
            IfFound,
            Hazards,
            KillTrigger,
            Adopt
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
                        //Base Bundle holds reverb presets & water material
                        if (map[0].Equals("map")) {
                            reverbAssets = currentAsset.LoadAllAssets<ReverbPreset>();
                            WaterMat = currentAsset.LoadAllAssets<Material>()[0];
                            mls.LogInfo($"Water material: {WaterMat.name}");
                            //Rebalanced compat
                            files = Directory.GetFiles(location, "rebalancedmoons.dll", SearchOption.AllDirectories);
                            if (files.Length > 0 && files != null) {
                                files = Directory.GetFiles(location, "rebalancedmoonscenes.lethalbundle", SearchOption.AllDirectories);
                                if (files.Length > 0 && files != null) {
                                    mls.LogWarning($"Dopadream found :). Rebalancing moons more!");
                                    Rebalanced = true;
                                }
                            } else {
                                files = Directory.GetFiles(location, "rebalancedmoons.chameleoncompat.dll", SearchOption.AllDirectories);
                                if (files.Length > 0 && files != null) {
                                    files = Directory.GetFiles(location, "rebalancedmoonscenes.lethalbundle", SearchOption.AllDirectories);
                                    if (files.Length > 0 && files != null) {
                                        mls.LogWarning($"Dopadream found :). Rebalancing moons more!");
                                        Rebalanced = true;
                                    }
                                }
                            }
                            //Chameleon compat
                            files = Directory.GetFiles(location, "chameleon.dll", SearchOption.AllDirectories);
                            if (files.Length > 0 && files != null) {
                                mls.LogWarning($"Chameleon found, adding door checks!");
                                Chameleon = true;
                            }
                            //TonightWeDine compatability
                            files = Directory.GetFiles(location, "TonightWeDine.dll", SearchOption.AllDirectories);
                            if (files.Length > 0 && files != null) {
                                files = Directory.GetFiles(location, "tonightwedine", SearchOption.AllDirectories);
                                if (files.Length > 0 && files != null) {
                                    mls.LogWarning($"Tonight we Dine! Modifying Dine!");
                                    TonightWeDine = true;
                                }
                            }
                            //ArtificeBlizzard compatibilty
                            files = Directory.GetFiles(location, "ArtificeBlizzard.dll", SearchOption.AllDirectories);
                            if (files.Length > 0 && files != null) {
                                files = Directory.GetFiles(location, "artificeblizzard", SearchOption.AllDirectories);
                                if (files.Length > 0 && files != null) {
                                    mls.LogWarning($"Artifice Blizzard! Modifying Artifice!");
                                    ArtificeBlizzard = true;
                                }
                            }
                        }
                        if (currentAssetObjects != null && currentAssetObjects.Length > 0) {
                            for (int objects = 0; objects < currentAssetObjects.Length; objects++) {
                                string[] parse = currentAssetObjects[objects].name.ToLower().Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
                                if (parse.Length < 2) continue;
                                int index;
                                MapData data = new MapData();
                                List<Edits> adjustments = new List<Edits>();
                                string configDescrip = "";
                                ConfigControl.Setting configDefault = ConfigControl.Setting.Enabled;
                                //Find the index/exists of each moon and add to that item
                                switch (parse[0]) {
                                    case "experimentation":
                                        if (parse[1].Equals("b")) {
                                            //2nd fire
                                            adjustments.Add(new Edits("StairsA", "Metal", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OverlapColliders", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OverlapColliders (2)", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(-195.4f, 19, -31.25f), F: 2));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Fills the hole in the back of the alleyway with a fire exit.";
                                        } else if (parse[1].Equals("c")) {
                                            //Cruiser viable
                                            adjustments.Add(new Edits("TreeALOD0", "Untagged", EditEnums.Clone, new Vector3(14, 5, -9), new Vector3(-77, 77, -285)));
                                            adjustments.Add(new Edits("TreeBLOD0", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TreeCLOD0", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Ladder1.5x", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("CementRaisedBridge (1)/CementDividers", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("StraightRaiing (2)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("StraightRaiing (3)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("StraightRaiing (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TrainCarTank (1)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TrainCarBase (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (2)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (4)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (5)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (6)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (22)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (26)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (33)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (34)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ItemShipAnimContainer", "Untagged", EditEnums.Move, new Vector3(53.5f, 4.7f, -93)));
                                            adjustments.Add(new Edits("OverlapColliders", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OverlapColliders (1)", "Concrete", EditEnums.Destroy));
                                            //Enable nodes
                                            adjustments.Add(new Edits("PitA (2)", "Catwalk", EditEnums.IfFound, I: new Found("InsideNodes", "Untagged", EditEnums.Enable)));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Creates a pathway for the cruiser, as well as enemies.";
                                        } else {
                                            adjustments.Add(new Edits("TreeALOD0", "Untagged", EditEnums.Clone, new Vector3(125, 5, 72), new Vector3(-80, 191, -83)));
                                            adjustments.Add(new Edits("BigMachine", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Environment/ReverbTriggers (1)/Cube (2)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-205.1f, 19.765f, -13.195f), new Vector3(270, 0, 90))); // (1)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-194.666f, 19.75f, -30.85f), new Vector3(270, 0, 90))); // (2)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-194.65f, 19.75f, 6), new Vector3(270, 0, 90))); // (3)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-175.18f, 19.75f, -3.06f), new Vector3(270, 0, 90))); // (4)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-154.185f, -2.16f, 6.317f), new Vector3(270, 0, 90))); // (7)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-133.93f, -2.16f, 6.317f), new Vector3(270, 0, 90))); // (8)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-143.712f, -2.16f, 38.127f), new Vector3(270, 0, 90))); // (9)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-192.6f, 19.765f, -12f), new Vector3(270, 0, 0))); // (14)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-185.25f, 19.765f, -9.45f), new Vector3(270, 0, 0))); // (15)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-151.5f, -2.16f, 16.74f), new Vector3(270, 0, 180))); // (16)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-146.9f, -2.16f, 58.41f), new Vector3(270, 0, 180))); // (17)
                                            //adjustments.Add(new Edits("OutOfBoundsTriggerFactory", "Untagged", EditEnums.Clone, new Vector3(-179, -8, 23.5f), S: new Vector3(10, 1, 12)));
                                            adjustments.Add(new Edits("Environment/ScanNodes/ScanNode", "Untagged", EditEnums.Move, new Vector3(-185, 0, 37)));
                                            adjustments.Add(new Edits("EntranceTeleportA", "InteractTrigger", EditEnums.AllTransforms, new Vector3(-188.3f, -1, 41.3f), S: new Vector3(0.5f, 3.5f, 6)));
                                            //These doors don't need a door frame, so destroy the doorframe after cloning
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-169.375f, -0.75f, 6.29f), new Vector3(270, 0, 90), F: 10)); // (10)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-172.345f, -0.75f, 6.29f), new Vector3(270, 0, 90), new Vector3(1, -1, 1), F: 11)); // (11)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-170.067f, -0.75f, 54.141f), new Vector3(270, 0, 90), F: 12)); // (12)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-173.037f, -0.75f, 54.141f), new Vector3(270, 0, 90), new Vector3(1, -1, 1), F: 13)); // (13)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-96.6f, -2.2f, 0.32f), new Vector3(270, 0, 0), F: 18)); // (18)
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Clone, new Vector3(-96.6f, -2.2f, -2.635f), new Vector3(270, 0, 0), new Vector3(1, -1, 1), F: 19)); // (19)
                                            adjustments.Add(new Edits("SteelDoor(Clone) 10/DoorFrame", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("SteelDoor(Clone) 11/DoorFrame", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("SteelDoor(Clone) 12/DoorFrame", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("SteelDoor(Clone) 13/DoorFrame", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("SteelDoor(Clone) 18/DoorFrame", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("SteelDoor(Clone) 19/DoorFrame", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("SteelDoor", "Untagged", EditEnums.Destroy));
                                            //Reverbs, lights, decoration, navmesh, nodes, spawn blockers, catwalks
                                            adjustments.Add(new Edits("TriggerLarge", "Grass", EditEnums.Reverb, F: 5, G: true));
                                            adjustments.Add(new Edits("TriggerHall", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (1)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (2)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (3)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (4)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (5)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (6)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (7)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (8)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (9)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (10)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerHall (11)", "Grass", EditEnums.Reverb, F: 6, G: true));
                                            adjustments.Add(new Edits("TriggerAlley", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("TriggerAlley (1)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("TriggerRoom", "Grass", EditEnums.Reverb, F: 10, G: true));
                                            adjustments.Add(new Edits("TriggerRoom (1)", "Grass", EditEnums.Reverb, F: 10, G: true));
                                            adjustments.Add(new Edits("TriggerRoom (2)", "Grass", EditEnums.Reverb, F: 10, G: true));
                                            adjustments.Add(new Edits("TriggerRoom (3)", "Grass", EditEnums.Reverb, F: 10, G: true));
                                            adjustments.Add(new Edits("TriggerRoom (4)", "Grass", EditEnums.Reverb, F: 10, G: true));
                                            adjustments.Add(new Edits("TriggerRoom (5)", "Grass", EditEnums.Reverb, F: 10, G: true));
                                            adjustments.Add(new Edits("OverlapColliders (1)", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OverlapColliders (2)", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("HazardSpawn", "Bush", EditEnums.Hazards, F: 8));
                                            adjustments.Add(new Edits("HazardSpawn (1)", "Bush", EditEnums.Hazards, F: 7));
                                            adjustments.Add(new Edits("HazardSpawn (2)", "Bush", EditEnums.Hazards, F: 5));
                                            adjustments.Add(new Edits("HazardSpawn (3)", "Bush", EditEnums.Hazards, F: 25));
                                            adjustments.Add(new Edits("Kill Plane", "Snow", EditEnums.KillTrigger));
                                            //Rewrite whole terrain
                                            //adjustments.Add(new Edits("PitA", "Untagged", EditEnums.KillTrigger));
                                            //adjustments.Add(new Edits("terrainMap", "Gravel", EditEnums.Destroy));
                                            //adjustments.Add(new Edits("OutOfBoundsTerrain", "Gravel", EditEnums.Destroy));
                                            //Override B
                                            adjustments.Add(new Edits("StairsB", "Metal", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EntranceTeleport2", "InteractTrigger", EditEnums.AllTransforms, new Vector3(-214.2f, 19, -16.25f), new Vector3(0, -90, 0)));
                                            adjustments.Add(new Edits("FireExitDoor", "Concrete", EditEnums.AllTransforms, new Vector3(-172.5f, 54.6f, -52.15f), default));
                                            adjustments.Add(new Edits("FireExitWall", "Concrete", EditEnums.Destroy));
                                            //Enable C nodes
                                            adjustments.Add(new Edits("InsideNodes", "Untagged", EditEnums.IfFound, I: new Found("InsideNodes", "Untagged", EditEnums.Enable)));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Moves the Main Entrance back into the unused facility from v4.";
                                        }
                                        index = 0;
                                        break;
                                    case "assurance":
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("OutsideNode (22)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(-0.85f, 9.147f, 76.25f), F: 2));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Adds in a new Fire Exit and more environmental detailing.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("rock.012 (1)", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("rock.007", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("RockSingle4", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("rock.001 (1)", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideAINode (29)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("RockObstacles/NavObs (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("RockObstacles/NavObs (6)", "Untagged", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Removes rocks to create a path for the Cruiser to get to the Main Entrance.";
                                        } else {
                                            adjustments.Add(new Edits("Cube", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ReverbTriggers (1)/Cube", "Untagged", EditEnums.Move, new Vector3(-274.25f, 7.5f, -76)));
                                            adjustments.Add(new Edits("ReverbTriggers (1)/Cube (16)", "Untagged", EditEnums.AllTransforms, new Vector3(-265, 16, -83), default, new Vector3(0.5f, 36, 50)));
                                            adjustments.Add(new Edits("ReverbTriggers (1)/Cube (1)", "Untagged", EditEnums.AllTransforms, new Vector3(-268.5f, 10, -80), default, new Vector3(0.5f, 16, 40)));
                                            adjustments.Add(new Edits("Ladder1.5x", "Untagged", EditEnums.Clone, new Vector3(142.6f, -0.9f, 75.85f), new Vector3(0, 108, 0), new Vector3(0.6f, 1.2f, 0.6f)));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Modifies the Main Entrance buidling, allowing access to the pipe to the Fire Exit through a bit of parkour.";
                                        }
                                        index = 1;
                                        break;
                                    case "vow":
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
                                            //Rebalanced compat
                                            adjustments.Add(new Edits("HangingLight", "PoweredLight", EditEnums.Destroy));
                                            adjustments.Add(new Edits("HangingLight (1)", "PoweredLight", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDefault = ConfigControl.Setting.RandomC;
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
                                            configDefault = ConfigControl.Setting.RandomB;
                                            configDescrip = "Adds in a new facility building right behind the ship, with a Fire Exit that has been flooded.";
                                        } else {
                                            adjustments.Add(new Edits("ChainlinkFence (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EnterAlley", "Snow", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("ExitAlley", "Snow", EditEnums.Reverb, F: 9));
                                            adjustments.Add(new Edits("EnterGrove", "Snow", EditEnums.Reverb, F: 6));
                                            adjustments.Add(new Edits("ExitTop", "Snow", EditEnums.Reverb, F: 9));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(12.75f, 8, 211.25f), new Vector3(0, 180, 0), F : 2));
                                            configDefault = ConfigControl.Setting.Always;
                                            configDescrip = "Expands the Facility building, adding in a new area with Fire Exit through the alleyway.";
                                        }
                                        index = 2;
                                        break;
                                    case "offense":
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("rock.011 (1)", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Environment/BoundsWalls/Cube (1)", "Untagged", EditEnums.Rotate, R: new Vector3(0, -14, 0)));
                                            adjustments.Add(new Edits("Environment/Map/Ladder1.5x", "Untagged", EditEnums.Clone, new Vector3(-11.7f, -10.5f, -120.85f), new Vector3(-180, -163, 180), new Vector3(0.58f, 1.05f, 0.58f)));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(-7.2f, 6.3f, -122.44f), new Vector3(0, 106, 0), F: 2));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Expands the map behind the Facility with a new Fire Exit.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("TrainCarTank", "Aluminum", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Environment/ReverbTriggers (1)/Cube (3)", "Untagged", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Adds more decor & parkour to the Fire Exit. Fixes navigation areas for Giants.";
                                        } else {
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(8.2f, 8.4f, 75.3f), new Vector3(0, 82, 0), F: 2));
                                            configDefault = ConfigControl.Setting.Enabled;
                                            configDescrip = "Adds in more to far side of the map and a new Fire Exit.";
                                        }
                                        index = 3;
                                        break;
                                    case "march":
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("QuicksandContainer (4)", "Untagged", EditEnums.Clone, new Vector3(82, -22, -25)));
                                            adjustments.Add(new Edits("QuicksandContainer (4)", "Untagged", EditEnums.Clone, new Vector3(79.25f, -22, -33)));
                                            adjustments.Add(new Edits("tree.001_LOD0 (59)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("tree.001_LOD0 (64)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Cube.002", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Inside", "Snow", EditEnums.Reverb, F: 10));
                                            adjustments.Add(new Edits("Inside (1)", "Snow", EditEnums.Reverb, F: 10));
                                            adjustments.Add(new Edits("Inside (2)", "Snow", EditEnums.Reverb, F: 10));
                                            adjustments.Add(new Edits("Outside", "Snow", EditEnums.Reverb, F: 8));
                                            adjustments.Add(new Edits("Outside (1)", "Snow", EditEnums.Reverb, F: 8));
                                            adjustments.Add(new Edits("Outside (2)", "Snow", EditEnums.Reverb, F: 8));
                                            adjustments.Add(new Edits("FireB", "Puddle", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDefault = ConfigControl.Setting.RandomAny;
                                            configDescrip = "Adds more to the environment & cabins to hide from Giants.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("EntranceTeleportD", "InteractTrigger", EditEnums.FireExit, new Vector3(-82, -6.5f, 61), new Vector3(-180, 10, 180), F: 4));
                                            adjustments.Add(new Edits("QuicksandContainer (2)", "Untagged", EditEnums.Clone, new Vector3(-105.8f, -27, 128), new Vector3(11, 22, -11)));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Adds to March's hilly terrain with cliffs, as well as a Fire Exit across the lake in the back right of the map.";
                                        } else {
                                            adjustments.Add(new Edits("Cube.002", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Cube.002 (1)", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("LadderTrigger", "InteractTrigger", EditEnums.Clone, new Vector3(12.35f, 7.2f, -145.45f), new Vector3(0, 180, 0), new Vector3(0.87f, 79, 0.34f), true, I: new Found("LadderNest", "Gravel", EditEnums.Adopt)));
                                            adjustments.Add(new Edits("Environment/MapPropsContainer/March_A(Clone)/FireB/LadderNest/LadderTrigger(Clone)/TopPos", "Untagged", EditEnums.Move, new Vector3(0, -0.04f, -1.8f)));
                                            //adjustments.Add(new Edits("LadderTrigger", "InteractTrigger", EditEnums.Clone, new Vector3(12.35f, 1.5f, -145.45f), new Vector3(0, 180, 0), new Vector3(0.87f, 60, 0.34f), true, I: new Found("FireB", "Puddle", EditEnums.Adopt)));
                                            adjustments.Add(new Edits("BasementCabin", "Puddle", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Expands the multiple facilities to make them more obvious.";
                                        }
                                        index = 4;
                                        break;
                                    case "adamance":
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("Cube.002", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.AllTransforms, new Vector3(-75.9f, -2.4f, -112.35f), new Vector3(0, 223, 0)));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Adds in easier pathing to and from the Fire Exit while also adjusting it slightly.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (4)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(56.8f, 13.1f, -92.3f), new Vector3(0, -220, 0), F: 2));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Adds in a new Fire Exit and environmental detailing on the left side of the ship landing area.";
                                        } else {
                                            adjustments.Add(new Edits("treeLeaflessBrown.001 Variant (4)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Adds in unique mineshaft environmental details to differentiate it from other forest moons.";
                                        }
                                        index = 5;
                                        break;
                                    case "rend":
                                        if (parse[1].Equals("b")) {

                                        } else if (parse[1].Equals("c")) {

                                        } else {

                                        }
                                        index = 6;
                                        break;
                                    case "dine":
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("treeLeafless.003_LOD0 (39)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("treeLeafless.003_LOD0 (41)", "Wood", EditEnums.Destroy));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(-172.7f, 7.4f, -15.98f), new Vector3(0, -96, 0), G: true, F: 2));
                                            configDefault = ConfigControl.Setting.RandomC;
                                            configDescrip = "Expands on the facility building, adding a new Fire exit on top, with additional fences and pipes.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(86.7f, 6.2f, -19.85f), new Vector3(0, -265.5f, 0), G: true, F: 2));
                                            configDefault = ConfigControl.Setting.RandomB;
                                            configDescrip = "Adds in a new Fire exit only accessible via jumping off the ship early, similar to Offense.";
                                        } else {
                                            adjustments.Add(new Edits("EntranceTeleportA", "InteractTrigger", EditEnums.AllTransforms, new Vector3(-122.03f, -13.55f, -7), new Vector3(0, 90, 0), G: true));
                                            adjustments.Add(new Edits("DoorFrame (1)", "Untagged", EditEnums.AllTransforms, new Vector3(-122.04f, -16.2f, -6.83f), new Vector3(-90, 180, -89.2f), G: true));
                                            adjustments.Add(new Edits("SteelDoorFake", "Untagged", EditEnums.AllTransforms, new Vector3(-123.55f, -12.71f, -6.85f), new Vector3(-90, 180, -89.2f), G: true));
                                            adjustments.Add(new Edits("SteelDoorFake (1)", "Untagged", EditEnums.AllTransforms, new Vector3(-120.64f, -12.71f, -6.89f), new Vector3(-90, 180, -89.2f), G: true));
                                            adjustments.Add(new Edits("Environment/Plane", "Untagged", EditEnums.AllTransforms, new Vector3(-122f, -12.8f, -6.87f), new Vector3(270, 0, 0), G: true));
                                            adjustments.Add(new Edits("Environment/ScanNodes/ScanNode", "Untagged", EditEnums.AllTransforms, new Vector3(-123f, -11f, -12f), new Vector3(-180, 110, 90), new Vector3(25, 50, 33), G: true));
                                            adjustments.Add(new Edits("ChainlinkFence", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFence (1)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFence (2)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFence (3)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFenceHoleModifier", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ChainlinkFenceBend", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Environment/Map/Collider", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Environment/Map/Collider (1)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("CliffJump (2)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("InteriorReverb", "Grass", EditEnums.Reverb, F: 5));
                                            adjustments.Add(new Edits("ExitReverb", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("ExitReverb (1)", "Grass", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            configDefault = ConfigControl.Setting.Always;
                                            configDescrip = "Adds in fences around the edges, with holes to allow for escaping Giants. Adjusts the Main entrance area to prevent Giants loitering.";
                                        }
                                        index = 7;
                                        break;
                                    case "titan":
                                        if (parse[1].Equals("b")) {

                                        } else if (parse[1].Equals("c")) {

                                        } else {

                                        }
                                        index = 8;
                                        break;
                                    case "artifice":
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("ItemShipAnimContainer", "Untagged", EditEnums.AllTransforms, new Vector3(50.5f, -2.8f, -42), new Vector3(-95, -55, -50)));
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
                                            //adjustments.Add(new Edits("BuildingAmbience", "Untagged", EditEnums.Clone, new Vector3(46.25f, 7, -73), new Vector3(180, 0, 180)));
                                            //adjustments.Add(new Edits("BuildingAmbience (7)", "Untagged", EditEnums.Clone, new Vector3(10.75f, 3, -84)));
                                            //adjustments.Add(new Edits("InsideAmbience (1)", "Untagged", EditEnums.Clone, new Vector3(47.5f, 7, -73)));
                                            adjustments.Add(new Edits("OutsideA", "Gravel", EditEnums.Reverb, F: 9));
                                            adjustments.Add(new Edits("OutsideB", "Gravel", EditEnums.Reverb, F: 9));
                                            adjustments.Add(new Edits("InsideA", "Gravel", EditEnums.Reverb, F: 5));
                                            adjustments.Add(new Edits("InsideB", "Gravel", EditEnums.Reverb, F: 5));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            adjustments.Add(new Edits("LargePipeCorner (3)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("LargePipeCorner (4)", "Untagged", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.RandomC;
                                            configDescrip = "Adds in a warehouse for the ship to land in.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(120, 1.75f, -19.86f), new Vector3(180, -90, 180), F: 2, G: true));
                                            adjustments.Add(new Edits("OuterFence/ChainlinkFence (57)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Colliders/ChainlinkFence (45)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Colliders/Cube", "Untagged", EditEnums.Move, new Vector3(126, 1.4f, -69.5f)));
                                            //adjustments.Add(new Edits("BuildingAmbience (3)", "Untagged", EditEnums.Clone, new Vector3(140, 7, -79), new Vector3(180, 0, 180), new Vector3(0.5f, 10, 20)));
                                            //adjustments.Add(new Edits("OutsideAmbience (1)", "Untagged", EditEnums.Clone, new Vector3(138, 7, -79)));
                                            adjustments.Add(new Edits("Exit", "Gravel", EditEnums.Reverb, F: 9));
                                            adjustments.Add(new Edits("Enter", "Gravel", EditEnums.Reverb, F: 5));
                                            //adjustments.Add(new Edits("OceanFixed", "Puddle", EditEnums.Water));
                                            //adjustments.Add(new Edits("Ocean (1)", "Puddle", EditEnums.Water));
                                            //adjustments.Add(new Edits("Ocean (2)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("WaterTrigger", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("WaterTrigger (1)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("WaterTrigger (2)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("WaterTrigger (3)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("WaterTrigger (4)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("WaterTrigger (5)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("WaterTrigger (6)", "Puddle", EditEnums.Water));
                                            adjustments.Add(new Edits("TreeBreakTrigger", "Wood", EditEnums.HasTrees));
                                            adjustments.Add(new Edits("LargePipeCorner (3)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("LargePipeCorner (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("PipeFix", "Snow", EditEnums.Destroy));
                                            adjustments.Add(new Edits("PipeFix (1)", "Snow", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.RandomB;
                                            configDescrip = "Adds in large bodies of water and dams, with a new Fire exit behind the ship.";
                                        } else {
                                            adjustments.Add(new Edits("ItemShipAnimContainer", "Untagged", EditEnums.AllTransforms, new Vector3(82, -4.66f, -98.5f), new Vector3(-86, 28, -43)));
                                            //adjustments.Add(new Edits("OutsideAmbience (3)", "Untagged", EditEnums.Clone, new Vector3(55, 2, -187)));
                                            //adjustments.Add(new Edits("OutsideAmbience (4)", "Untagged", EditEnums.Clone, new Vector3(14, -1, -198)));
                                            //adjustments.Add(new Edits("BuildingAmbience (7)", "Untagged", EditEnums.Clone, new Vector3(15.25f, -1, -198)));
                                            //adjustments.Add(new Edits("BuildingAmbience (6)", "Untagged", EditEnums.Clone, new Vector3(53.5f, 3, -188)));
                                            adjustments.Add(new Edits("OutsideAmbienceA", "Gravel", EditEnums.Reverb, F: 9));
                                            adjustments.Add(new Edits("OutsideAmbienceB", "Gravel", EditEnums.Reverb, F: 9));
                                            adjustments.Add(new Edits("4thBuildingAmbience", "Gravel", EditEnums.Reverb, F: 5));
                                            adjustments.Add(new Edits("4thBuildingAmbience (1)", "Gravel", EditEnums.Reverb, F: 5));
                                            adjustments.Add(new Edits("CrateAmbience", "Gravel", EditEnums.Reverb, F: 3));
                                            adjustments.Add(new Edits("GarageDoorContainer (2)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("GreyRockGrouping2 (6)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Colliders/Cube (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OuterFence/ChainlinkFence (65)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("LargePipeCorner (3)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("LargePipeCorner (4)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OverlappingPipe", "Snow", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OverlappingPipe (1)", "Snow", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.Always;
                                            configDescrip = "Allows access to the 4th warehouse. Adjusts the dropship position. Adds in more nodes for AI pathfinding.";
                                        }
                                        index = 9;
                                        break;
                                    case "embrion":
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(205.75f, -8.7f, 116.3f), new Vector3(0, 125, 0), F: 2));
                                            adjustments.Add(new Edits("GroundFog", "Untagged", EditEnums.Move, new Vector3(75, 5.2f, 66)));
                                            adjustments.Add(new Edits("GroundFog", "Untagged", EditEnums.Clone, new Vector3(85, 5.2f, -144)));
                                            adjustments.Add(new Edits("BigMachine", "Untagged", EditEnums.Move, new Vector3(208, -20, 116)));
                                            //Improvement cross compat
                                            adjustments.Add(new Edits("Cave", "Wood", EditEnums.IfFound, I: new Found("WithoutA", "Concrete", EditEnums.Destroy)));
                                            adjustments.Add(new Edits("Cave", "Wood", EditEnums.IfFound, I: new Found("WithA", "Untagged", EditEnums.Enable)));
                                            adjustments.Add(new Edits("Cavern A", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Exit A", "Wood", EditEnums.Reverb, F: 0));
                                            //All
                                            adjustments.Add(new Edits("AllImprovements", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("AllImprovements (2)", "Rock", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.RandomAny;
                                            configDescrip = "Adds a new Fire exit to the left of Main entrance.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.Clone, new Vector3(270.75f, -5.37f, -90.35f), new Vector3(0, 2, 0)));
                                            adjustments.Add(new Edits("Environment/BoundsWalls/Cube", "Untagged", EditEnums.AllTransforms, new Vector3(700, 16.59f, 406), new Vector3(0, -15, 0)));
                                            adjustments.Add(new Edits("Environment/BoundsWalls/Cube (1)", "Untagged", EditEnums.AllTransforms, new Vector3(365, 19.5f, -4)));
                                            //adjustments.Add(new Edits("FireCave", "Rock", EditEnums.Destroy));
                                            //adjustments.Add(new Edits("Cube.002", "Concrete", EditEnums.Destroy));
                                            //Improvement cross compat
                                            adjustments.Add(new Edits("Cave", "Wood", EditEnums.IfFound, I: new Found("NoA", "Concrete", EditEnums.Destroy)));
                                            adjustments.Add(new Edits("Cave", "Wood", EditEnums.IfFound, I: new Found("NeedsA", "Untagged", EditEnums.Enable)));
                                            adjustments.Add(new Edits("Passage", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Passage (1)", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Passage (2)", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Outsider", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Outsider (1)", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Outsider (2)", "Wood", EditEnums.Reverb, F: 0));
                                            //All
                                            adjustments.Add(new Edits("Plane (2)", "Gravel", EditEnums.Destroy));
                                            adjustments.Add(new Edits("AllImprovements", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("AllImprovements (1)", "Rock", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.RandomAny;
                                            configDescrip = "Expands the play area, adds more AI nodes, and adds a Fire Exit behind the facility.";
                                        } else {
                                            adjustments.Add(new Edits("CementFacility1", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("LEDHangingLight", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("LEDHangingLight (1)", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Environment/NavObstacles/NavObs", "Untagged", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ScanNode", "Untagged", EditEnums.Move, new Vector3(-198, 8.6f, -21), G: true));
                                            //adjustments.Add(new Edits("RockJump", "Untagged", EditEnums.Destroy));
                                            //adjustments.Add(new Edits("RockJump (5)", "Untagged", EditEnums.Destroy));
                                            //adjustments.Add(new Edits("RockJump (6)", "Untagged", EditEnums.Destroy));
                                            //adjustments.Add(new Edits("RockJump (7)", "Untagged", EditEnums.Destroy));
                                            //adjustments.Add(new Edits("GreyRockGrouping4", "Untagged", EditEnums.Destroy));
                                            //adjustments.Add(new Edits("OutsideNode (24)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (43)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (44)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (47)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (48)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("OutsideNode (49)", "OutsideAINode", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Environment/Plane", "Untagged", EditEnums.Move, new Vector3(235.55f, 1.4f, -7.1f)));
                                            adjustments.Add(new Edits("DoorFrame (1)", "Untagged", EditEnums.Move, new Vector3(235.53f, -1.96f, -7.17f)));
                                            adjustments.Add(new Edits("SteelDoorFake", "Untagged", EditEnums.Move, new Vector3(236.48f, 1.437f, -5.98f)));
                                            adjustments.Add(new Edits("SteelDoorFake (1)", "Untagged", EditEnums.Move, new Vector3(234.57f, 1.437f, -8.25f)));
                                            adjustments.Add(new Edits("EntranceTeleportA", "InteractTrigger", EditEnums.Move, new Vector3(235.4f, 0.7f, -7.07f)));
                                            adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.Move, new Vector3(197.6f, 4, -134.3f)));
                                            adjustments.Add(new Edits("Cave", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Cave (1)", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Cave (2)", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Cave (3)", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Cave (4)", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Exit", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (1)", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (2)", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (3)", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (4)", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Exit (5)", "Wood", EditEnums.Reverb, F: 0));
                                            //Modify other improvements
                                            adjustments.Add(new Edits("WithoutA", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("NoA", "Concrete", EditEnums.Destroy));
                                            adjustments.Add(new Edits("FireExitDoorB", "Carpet", EditEnums.IfFound, I: new Found("WithA", "Untagged", EditEnums.Enable)));
                                            adjustments.Add(new Edits("FireExitDoorC", "Carpet", EditEnums.IfFound, I: new Found("NeedsA", "Untagged", EditEnums.Enable)));
                                            adjustments.Add(new Edits("Cavern A", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Exit A", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Passage", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Passage (1)", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Passage (2)", "Wood", EditEnums.Reverb, F: 2, G: true));
                                            adjustments.Add(new Edits("Outsider", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Outsider (1)", "Wood", EditEnums.Reverb, F: 0));
                                            adjustments.Add(new Edits("Outsider (2)", "Wood", EditEnums.Reverb, F: 0));
                                            //All
                                            adjustments.Add(new Edits("AllImprovements (1)", "Rock", EditEnums.Destroy));
                                            adjustments.Add(new Edits("AllImprovements (2)", "Rock", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.RandomAll;
                                            configDescrip = "Integrates the Main entrance and Fire exit buildings into the environment better.";
                                        }
                                        index = 10;
                                        break;
                                    case "company building":
                                        if (parse[1].Equals("b")) {
                                            adjustments.Add(new Edits("ShippingContainer", "Aluminum", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ShippingContainer (3)", "Aluminum", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ShippingContainer (5)", "Aluminum", EditEnums.Destroy));
                                            adjustments.Add(new Edits("ShippingContainer (6)", "Aluminum", EditEnums.Destroy));
                                            adjustments.Add(new Edits("Puddle2", "Untagged", EditEnums.Clone, new Vector3(-11, 3.853f, 55.5f), new Vector3(0, 55, 0), new Vector3(3, 2.25f, 2.5f)));
                                            configDefault = ConfigControl.Setting.RandomC;
                                            configDescrip = "Removes some shipping containers for easy driving of the Company Cruiser.";
                                        } else if (parse[1].Equals("c")) {
                                            adjustments.Add(new Edits("DrillMainBody", "Aluminum", EditEnums.Move, new Vector3(0, 0, 24.25f)));
                                            adjustments.Add(new Edits("PullCordArmature", "Untagged", EditEnums.Move, new Vector3(0.04f, 2.027f, 24.63f)));
                                            adjustments.Add(new Edits("SteelBolt", "Untagged", EditEnums.Move, new Vector3(-0.96f, 1.5264f, 21.13f)));
                                            adjustments.Add(new Edits("DrillWallMark", "Untagged", EditEnums.Move, new Vector3(52.621f, -23.7604f, -1.458f)));
                                            adjustments.Add(new Edits("PumpCordBroken", "Untagged", EditEnums.Destroy));
                                            configDefault = ConfigControl.Setting.RandomB;
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
                                            files = Directory.GetFiles(location, "mrgrm7.LethalCasino.dll", SearchOption.AllDirectories);
                                            if (files.Length > 0 && files != null) {
                                                files = Directory.GetFiles(location, "lethalcasinoassets", SearchOption.AllDirectories);
                                                if (files.Length > 0 && files != null) {
                                                    mls.LogWarning($"Let's go gambling! Casino mod found, modifying company.");
                                                    adjustments.Add(new Edits("ShippingCasino", "Aluminum", EditEnums.Destroy));
                                                }
                                            }
                                            configDefault = ConfigControl.Setting.Always;
                                            configDescrip = "Recieves more shipments, with some of them being open. Great for playing hide and seek with friends, while also limiting the area with walls.";
                                        }
                                        index = 11;
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
                                                    Found iff = default;
                                                    configDefault = ConfigControl.Setting.Enabled;
                                                    configDescrip = $"Improvements for the modded moon {parse[0]}";
                                                    for (int k = 0; k < map.Length; k++) {
                                                        if (map[k].IsNullOrWhiteSpace()) {
                                                            adjustments.Add(new Edits(obj, tag, edit, pos, rot, scl, glo, fir, iff));
                                                            on = 0;
                                                            obj = "";
                                                            tag = "";
                                                            edit = EditEnums.Disable;
                                                            pos = default;
                                                            rot = default;
                                                            scl = default;
                                                            glo = false;
                                                            fir = 0;
                                                            iff = default;
                                                        } else {
                                                            //Check for default config
                                                            if (map[k][0].Equals('@')) {
                                                                string setting = map[k].Substring(1).ToLower().Trim();
                                                                switch (setting) {
                                                                    case "disabled":
                                                                        configDefault = ConfigControl.Setting.Disabled;
                                                                        break;
                                                                    case "never":
                                                                        configDefault = ConfigControl.Setting.Never;
                                                                        break;
                                                                    case "always":
                                                                        configDefault = ConfigControl.Setting.Always;
                                                                        break;
                                                                    case "combine a":
                                                                    case "combinea":
                                                                        configDefault = ConfigControl.Setting.CombineA;
                                                                        break;
                                                                    case "combine b":
                                                                    case "combineb":
                                                                        configDefault = ConfigControl.Setting.CombineB;
                                                                        break;
                                                                    case "combine c":
                                                                    case "combinec":
                                                                        configDefault = ConfigControl.Setting.CombineC;
                                                                        break;
                                                                    case "combine all":
                                                                    case "combineall":
                                                                        configDefault = ConfigControl.Setting.CombineAll;
                                                                        break;
                                                                    case "random a":
                                                                    case "randoma":
                                                                        configDefault = ConfigControl.Setting.RandomA;
                                                                        break;
                                                                    case "random b":
                                                                    case "randomb":
                                                                        configDefault = ConfigControl.Setting.RandomB;
                                                                        break;
                                                                    case "random c":
                                                                    case "randomc":
                                                                        configDefault = ConfigControl.Setting.RandomC;
                                                                        break;
                                                                    case "random any":
                                                                    case "randomany":
                                                                        configDefault = ConfigControl.Setting.RandomAny;
                                                                        break;
                                                                    case "random all":
                                                                    case "randomall":
                                                                        configDefault = ConfigControl.Setting.RandomAll;
                                                                        break;
                                                                    default:
                                                                        configDefault = ConfigControl.Setting.Enabled;
                                                                        break;
                                                                }
                                                                mls.LogInfo($"Found default config setting for moon {parse[0]} = {configDefault}");
                                                                continue;
                                                            }
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
                                                                        case "all transforms":
                                                                            edit = EditEnums.AllTransforms;
                                                                            break;
                                                                        case "destroy":
                                                                            edit = EditEnums.Destroy;
                                                                            break;
                                                                        case "fire":
                                                                        case "fireexit":
                                                                        case "fire exit":
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
                                                                        case "reverb trigger":
                                                                            edit = EditEnums.Reverb;
                                                                            break;
                                                                        case "trees":
                                                                        case "hastrees":
                                                                        case "has trees":
                                                                            edit = EditEnums.HasTrees;
                                                                            break;
                                                                        case "iffound":
                                                                        case "if found":
                                                                            edit = EditEnums.IfFound;
                                                                            break;
                                                                        case "hazards":
                                                                            edit = EditEnums.Hazards;
                                                                            break;
                                                                        case "kill":
                                                                        case "killtrigger":
                                                                        case "kill trigger":
                                                                            edit = EditEnums.KillTrigger;
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
                                                                        case EditEnums.FireExit:
                                                                        case EditEnums.AllTransforms:
                                                                            pos = StringToVector3(map[k]);
                                                                            break;
                                                                        case EditEnums.Rotate:
                                                                            rot = StringToVector3(map[k]);
                                                                            break;
                                                                        case EditEnums.Scale:
                                                                            scl = StringToVector3(map[k]);
                                                                            break;
                                                                        case EditEnums.IfFound:
                                                                            mls.LogError($"If Found enum has not been implemented yet. Skipping");
                                                                            break;
                                                                        default:
                                                                            if (int.TryParse(map[k], out int z)) fir = z;
                                                                            if (StringToBoolean(map[k])) glo = true;
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
                                data.Default = configDefault;
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