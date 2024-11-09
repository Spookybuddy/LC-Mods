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
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class MapImprovementModBase : BaseUnityPlugin
    {
        //Mod declaration
        public const string modGUID = "MapImprovements";
        private const string modName = "MapImprovements";
        private const string modVersion = "0.9.0";

        //Mod initializers
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static MapImprovementModBase Instance;
        internal static ManualLogSource mls;
        internal ConfigControl Configuration;

        //Mod variables
        internal static string[] foundOutsideAssetFiles;
        internal static AssetBundle currentAsset;
        internal static GameObject[] currentAssetObjects;
        internal static TextAsset[] currentInstructions;

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

            //Construction
            public Edits(string N, string T, EditEnums D, Vector3 P = default, Vector3 R = default, Vector3 S = default, int F = 0)
            {
                Name = N;
                Tag = T;
                Do = D;
                Postion = P;
                Rotation = R;
                Scale = S;
                FireExitIndex = F;
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
            Bridge
        }

        void Awake()
        {
            //Find the base mapimprovements, then look for any other X improvements.bundle
            if (Instance == null) Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            foundOutsideAssetFiles = Directory.GetFiles(Path.GetDirectoryName(Info.Location), "*improvements.bundle");
            if (foundOutsideAssetFiles != null && foundOutsideAssetFiles.Length > 0) {
                //Parse moon improvements
                for (int i = 0; i < foundOutsideAssetFiles.Length; i++) {
                    mls.LogInfo($"Found {foundOutsideAssetFiles[i]}");
                    currentAsset = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), foundOutsideAssetFiles[i]));
                    if (currentAsset != null) {
                        currentAssetObjects = currentAsset.LoadAllAssets<GameObject>();
                        currentInstructions = currentAsset.LoadAllAssets<TextAsset>();
                        if (currentAssetObjects != null && currentAssetObjects.Length > 0) {
                            for (int objects = 0; objects < currentAssetObjects.Length; objects++) {
                                string[] parse = currentAssetObjects[objects].name.ToLower().Split(new[] { "_" }, System.StringSplitOptions.RemoveEmptyEntries);
                                int index;
                                MapData data = new MapData();
                                List<Edits> adjustments = new List<Edits>();
                                string configDescrip = "";
                                //Find the index/exists of each moon and add to that item
                                switch (parse[0]) {
                                    case "experimentation":
                                        index = 0;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int exp)) {
                                                if (exp == 1) {

                                                } else if (exp == 2) {

                                                }
                                            }
                                        } else {

                                        }
                                        break;
                                    case "assurance":
                                        index = 1;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int ass)) {
                                                if (ass == 1) {

                                                } else if (ass == 2) {

                                                }
                                            }
                                        } else {

                                        }
                                        break;
                                    case "vow":
                                        index = 2;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int vow)) {
                                                if (vow == 1) {

                                                } else if (vow == 2) {

                                                }
                                            }
                                        } else {

                                        }
                                        break;
                                    case "offense":
                                        index = 3;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int off)) {
                                                if (off == 1) {

                                                } else if (off == 2) {

                                                }
                                            }
                                        } else {

                                        }
                                        break;
                                    case "march":
                                        index = 4;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int mar)) {
                                                if (mar == 1) {

                                                } else if (mar == 2) {

                                                }
                                            }
                                        } else {

                                        }
                                        break;
                                    case "adamance":
                                        index = 5;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int ada)) {
                                                if (ada == 1) {
                                                    adjustments.Add(new Edits("Cube.002", "Concrete", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.AllTransforms, new Vector3(-75.9f, -2.4f, -112.35f), new Vector3(0, 223, 0)));
                                                    configDescrip = "Adds in easier pathing to and from the Fire Exit while also adjusting it slightly.";
                                                } else if (ada == 2) {
                                                    adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(56.8f, 13.1f, -92.3f), new Vector3(0, -220, 0), F: 2));
                                                    configDescrip = "Adds in a new Fire Exit and environmental detailing on the left side of the ship landing area.";
                                                }
                                            }
                                        } else configDescrip = "Adds in unique environmental details to differentiate it from other forest moons. Makes certain hills easier to climb.";
                                        break;
                                    case "rend":
                                        index = 6;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int ren)) {
                                                if (ren == 1) {

                                                } else if (ren == 2) {

                                                }
                                            }
                                        } else {

                                        }
                                        break;
                                    case "dine":
                                        index = 7;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int din)) {
                                                if (din == 1) {
                                                    adjustments.Add(new Edits("treeLeafless.003_LOD0 (39)", "Wood", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("treeLeafless.003_LOD0 (41)", "Wood", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(195, -0.4f, 11.5f), new Vector3(0, 84, 0), F: 2));
                                                    configDescrip = "Expands on the facility building, adding a new fire exit on top, with additional fences and pipes.";
                                                } else if (din == 2) {
                                                    adjustments.Add(new Edits("EntranceTeleportB", "InteractTrigger", EditEnums.FireExit, new Vector3(-64.4f, -1.6f, 15.3f), new Vector3(0, -85.546f, 0), F: 2));
                                                    configDescrip = "Adds in a new Fire Exit only accessible via jumping off the ship early, similar to Offense.";
                                                }
                                            }
                                        } else {
                                            adjustments.Add(new Edits("EntranceTeleportA", "InteractTrigger", EditEnums.Move, new Vector3(144.4f, -21.25f, 2.5f)));
                                            adjustments.Add(new Edits("DoorFrame (1)", "Untagged", EditEnums.Move, new Vector3(144.33f, -23.91f, 2.34f)));
                                            adjustments.Add(new Edits("SteelDoorFake", "Untagged", EditEnums.Move, new Vector3(145.85f, -20.41f, 2.37f)));
                                            adjustments.Add(new Edits("SteelDoorFake (1)", "Untagged", EditEnums.Move, new Vector3(142.94f, -20.41f, 2.4f)));
                                            adjustments.Add(new Edits("Environment/Plane", "Untagged", EditEnums.Move, new Vector3(144.33f, -20.51f, 2.38f)));
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
                                            adjustments.Add(new Edits("CliffJump (3)", "Untagged", EditEnums.Destroy));
                                            configDescrip = "Adds in fences around the edges, with holes to allow for escaping Giants. Adjusts the main entrance area to prevent Giants loitering.";
                                        }
                                        break;
                                    case "titan":
                                        index = 8;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int tit)) {
                                                if (tit == 1) {

                                                } else if (tit == 2) {

                                                }
                                            }
                                        } else {

                                        }
                                        break;
                                    case "artifice":
                                        index = 9;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int art)) {
                                                if (art == 1) {
                                                    adjustments.Add(new Edits("ItemShipAnimContainer", "Untagged", EditEnums.AllTransforms, new Vector3(72, 2.75f, -62.5f), new Vector3(-90, 45, -50)));
                                                    adjustments.Add(new Edits("treeLeafless", "Wood", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("treeLeafless (4)", "Wood", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("treeLeafless.002_LOD0 (3)", "Wood", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("SteelDoorMapModel (4)", "Untagged", EditEnums.Clone, new Vector3(12.25f, 4.5f, -11.21f), new Vector3(180, -92, -180), new Vector3(1, 1.05f, 1)));
                                                    adjustments.Add(new Edits("FogExclusionZone (2)", "Untagged", EditEnums.Clone, new Vector3(30.5f, 14, -0.75f)));
                                                    adjustments.Add(new Edits("BuildingAmbience", "Untagged", EditEnums.Clone, new Vector3(46.25f, 7, -73), new Vector3(180, 0, 180)));
                                                    adjustments.Add(new Edits("BuildingAmbience (7)", "Untagged", EditEnums.Clone, new Vector3(10.75f, 3, -84)));
                                                    adjustments.Add(new Edits("InsideAmbience (1)", "Untagged", EditEnums.Clone, new Vector3(47.5f, 7, -73)));
                                                    configDescrip = "Adds in a warehouse and platform for the ship to land in.";
                                                } else if (art == 2) {
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
                                                    configDescrip = "Adds in large bodies of water and dams.";
                                                }
                                            }
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
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int emb)) {
                                                if (emb == 1) {

                                                } else if (emb == 2) {

                                                }
                                            }
                                        } else {

                                        }
                                        break;
                                    case "companybuilding":
                                        index = 11;
                                        if (parse.Length > 1) {
                                            if (int.TryParse(parse[1], out int com)) {
                                                if (com == 1) {
                                                    adjustments.Add(new Edits("ShippingContainer", "Aluminum", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("ShippingContainer (3)", "Aluminum", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("ShippingContainer (5)", "Aluminum", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("ShippingContainer (6)", "Aluminum", EditEnums.Destroy));
                                                    adjustments.Add(new Edits("Puddle2", "Untagged", EditEnums.Clone, new Vector3(-11, 3.853f, 55.5f), new Vector3(0, 55, 0), new Vector3(3, 2.25f, 2.5f)));
                                                    configDescrip = "Removes some shipping containers for easy driving of the Company Cruiser.";
                                                } else if (com == 2) {
                                                    configDescrip = "Secret drill :)";
                                                }
                                            }
                                        } else {
                                            adjustments.Add(new Edits("Puddle", "Untagged", EditEnums.Clone, new Vector3(-9, 3.85f, -74.5f), new Vector3(-180, -41, 180), new Vector3(-1.5f, 1.5f, 1.5f)));
                                            configDescrip = "Recieves more shipments, with some of them being open. Great for playing hide and seek with friends, while also limiting the area with walls.";
                                        }
                                        break;
                                    default:
                                        //Find objects with the same name as the beginning of the file
                                        string[] map = foundOutsideAssetFiles[i].ToLower().Split(new[] { "\\" }, System.StringSplitOptions.RemoveEmptyEntries);
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
                                                    int fir = 0;
                                                    configDescrip = $"Improvements for the modded moon {parse[0]}";
                                                    for (int k = 0; k < map.Length; k++) {
                                                        if (map[k].IsNullOrWhiteSpace()) {
                                                            adjustments.Add(new Edits(obj, tag, edit, pos, rot, scl, fir));
                                                            on = 0;
                                                            obj = "";
                                                            tag = "";
                                                            edit = EditEnums.Disable;
                                                            pos = default;
                                                            rot = default;
                                                            scl = default;
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
                                                                        default:
                                                                            mls.LogError("Unable to parse Edit Enum. Please make sure things are formatted properly.");
                                                                            edit = EditEnums.Disable;
                                                                            break;
                                                                    }
                                                                    break;
                                                                case 3:
                                                                    pos = StringToVector3(map[k]);
                                                                    break;
                                                                case 4:
                                                                    rot = StringToVector3(map[k]);
                                                                    break;
                                                                case 5:
                                                                    scl = StringToVector3(map[k]);
                                                                    break;
                                                                case 6:
                                                                    if (int.TryParse(map[k], out int x)) fir = x;
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
            float[] v = new float[3];
            for (int a = 0; a < values.Length; a++) {
                if (a > 2) break;
                values[a].Trim();
                if (float.TryParse(values[a], out float res)) v[a] = res;
            }
            return new Vector3(v[0], v[1], v[2]);
        }
    }
}