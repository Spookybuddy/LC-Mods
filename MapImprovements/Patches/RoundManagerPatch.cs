using HarmonyLib;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.SceneManagement;
using static MapImprovements.MapImprovementModBase;

namespace MapImprovements.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static bool ReExpScene = false;
        private static bool ReDinScene = false;
        private static bool ReEmbScene = false;

        private static Material WaterMat;
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void GetWaterMat()
        {
            WaterMat = GameObject.Find("TimeAndWeather").transform.GetChild(4).GetChild(1).GetComponent<MeshRenderer>().material;
        }

        [HarmonyPatch("FinishGeneratingNewLevelClientRpc")]
        [HarmonyPostfix]
        static void FixChameleonDoors(RoundManager __instance) {
            if (!MapImprovementModBase.Instance.Chameleon || !ConfigControl.Instance.ModEnabled) return;
            GameObject Chameleon = GameObject.Find("WideDoorFrame(Clone)");
            if (Chameleon != null) {
                int id = GetMoonIndex(__instance.currentLevel.name.ToLower().Trim());
                if (id < 0) return;
                switch (id) {
                    case 0:
                        if (GameObject.Find("Experimentation A(Clone)")) GameObject.Destroy(Chameleon);
                        return;
                    case 7:
                        if (GameObject.Find("Dine A(Clone)")) {
                            ApplyEnum(Chameleon, new MapImprovementModBase.Edits("", "", MapImprovementModBase.EditEnums.AllTransforms, new Vector3(-122.04f, -15.25f, -6.9f), new Vector3(-90, 180, -89.2f), G: true), 0);
                            MapImprovementModBase.mls.LogInfo("Moved main door.");
                        }
                        return;
                    case 10:
                        if (GameObject.Find("Embrion A(Clone)")) {
                            ApplyEnum(Chameleon, new MapImprovementModBase.Edits("", "", MapImprovementModBase.EditEnums.AllTransforms, new Vector3(235.6f, 1.5f, -7.15f)), 0);
                            MapImprovementModBase.mls.LogInfo("Moved main door.");
                        }
                        return;
                    default:
                        return;
                }
            }
        }

        [HarmonyPatch("SetChallengeFileRandomModifiers")]
        [HarmonyPrefix]
        static void AddImprovements(RoundManager __instance)
        {
            //Exit if mod is disabled
            if (!ConfigControl.Instance.ModEnabled) return;

            GameObject container = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (container == null) return;
            int moon = GetMoonIndex(__instance.currentLevel.name.ToLower().Trim());
            if (moon < 0) return;

            //Empty / Disabled; Exit
            if (MapImprovementModBase.Instance.Moons[moon].Adjustments == null || MapImprovementModBase.Instance.Moons[moon].Adjustments.Count < 1) return;
            if (!ConfigControl.Instance.cfgMoons[moon].Enabled) return;

            //Rebalanced scene check
            if (MapImprovementModBase.Instance.Rebalanced) {
                if (SceneManager.GetSceneByName("ReExperimentationScene").IsValid()) {
                    MapImprovementModBase.mls.LogWarning("Rebalanced Experiementation detected! Cull!");
                    ReExpScene = true;
                }
                if (SceneManager.GetSceneByName("ReDineScene").IsValid()) {
                    MapImprovementModBase.mls.LogWarning("Rebalanced Dine detected! Don't kill!");
                    ReDinScene = true;
                }
                if (SceneManager.GetSceneByName("ReEmbrionScene").IsValid()) {
                    MapImprovementModBase.mls.LogWarning("Rebalanced Embrion detected! Remove Colliders!");
                    ReEmbScene = true;
                }
            }

            //Apply mod
            Mod(moon, __instance.playersManager.randomMapSeed, container);

            //Compats
            switch (moon) {
                case 0:
                    if (ReExpScene) {
                        FindObject(new Edits("InsideNodes", "Untagged", EditEnums.Destroy));
                        FindObject(new Edits("Environment/ScanNodes/ScanNode", "Untagged", EditEnums.Move, new Vector3(-95, 0, 0)));
                        FindObject(new Edits("EntranceTeleportA", "InteractTrigger", EditEnums.AllTransforms, new Vector3(-96.35f, -3.12f, -1.15f), S: new Vector3(0.33f, 3.27f, 3.4f)));
                        FindObject(new Edits("EntranceTeleport2", "InteractTrigger", EditEnums.AllTransforms, new Vector3(-195.4f, 19, -31.25f), Vector3.zero));
                    } else {
                        FindObject(new Edits("Experimentation_A(Clone)", "Untagged", EditEnums.IfFound, I: new Found("SteelDoor (5)", "Untagged", EditEnums.Destroy)));
                        FindObject(new Edits("Experimentation_A(Clone)", "Untagged", EditEnums.IfFound, I: new Found("SteelDoor (6)", "Untagged", EditEnums.Destroy)));
                    }
                    break;
                case 7:
                    if (!ReDinScene && !MapImprovementModBase.Instance.TonightWeDine) {
                        FindObject(new Edits("Dine_A(Clone)", "Untagged", EditEnums.IfFound, I: new Found("NeonLightsSingle", "PoweredLight", EditEnums.Destroy)));
                        FindObject(new Edits("Dine_A(Clone)", "Untagged", EditEnums.IfFound, I: new Found("Cube.002", "Concrete", EditEnums.Destroy)));
                    }
                    break;
                case 10:
                    if (ReEmbScene) {
                        FindObject(new Edits("Embrion_B(Clone)", "Untagged", EditEnums.IfFound, I: new Found("TerrainFix", "Rock", EditEnums.Destroy)));
                        FindObject(new Edits("Embrion_B(Clone)", "Untagged", EditEnums.IfFound, I: new Found("TerrainFix (1)", "Rock", EditEnums.Destroy)));
                        FindObject(new Edits("Embrion_B(Clone)", "Untagged", EditEnums.IfFound, I: new Found("Cube (1)", "Concrete", EditEnums.Destroy)));
                        FindObject(new Edits("Embrion_B(Clone)", "Untagged", EditEnums.IfFound, I: new Found("Cube (2)", "Concrete", EditEnums.Destroy)));
                    }
                    break;
                default:
                    break;
            }

            //Trying out outside hazards :)
            OutsideHazards(__instance.currentLevel);

            //Bake Navmesh for custom moons - Maybe also bake for no outside hazard spawns?
            GameObject navigation = GameObject.FindGameObjectWithTag("OutsideLevelNavMesh");
            if (navigation != null) {
                if (navigation.TryGetComponent<NavMeshSurface>(out NavMeshSurface build)) build.BuildNavMesh();
            }
        }

        //Everything 
        static void Mod(int moon, int randomSeed, GameObject container)
        {
            //Check enum and randomize
            bool vanilla = ConfigControl.Instance.cfgMoons[moon].Vanilla;
            int fireOffset = 0;

            //Always spawning objects
            for (int i = 0; i < ConfigControl.Instance.cfgMoons[moon].cfgObjects.Length; i++) {
                if (ConfigControl.Instance.cfgMoons[moon].cfgObjects[i].Settings.Equals(ConfigControl.Setting.Always)) fireOffset += ApplyObject(moon, i, container.transform);
            }

            //Calculate the base spawn, exiting if the vanilla is selected / oob / disabled from spawning (but that should never happen)
            int index = RandomIndex(moon, randomSeed, vanilla);
            if (index < 0 || index >= MapImprovementModBase.Instance.Moons[moon].Adjustments.Count) return;
            if (ConfigControl.Instance.cfgMoons[moon].cfgObjects[index].Settings.Equals(ConfigControl.Setting.Disabled)) return;
            if (ConfigControl.Instance.cfgMoons[moon].cfgObjects[index].Settings.Equals(ConfigControl.Setting.Never)) return;
            fireOffset += ApplyObject(moon, index, container.transform);

            //New System
            switch (ConfigControl.Instance.cfgMoons[moon].cfgObjects[index].Settings) {
                case ConfigControl.Setting.CombineA:
                    if (index != 0) ApplyObject(moon, 0, container.transform, fireOffset);
                    break;
                case ConfigControl.Setting.CombineB:
                    if (index != 1) ApplyObject(moon, 1, container.transform, fireOffset);
                    break;
                case ConfigControl.Setting.CombineC:
                    if (index != 2) ApplyObject(moon, 2, container.transform, fireOffset);
                    break;
                case ConfigControl.Setting.CombineAll:
                    for (int i = 0; i < MapImprovementModBase.Instance.Moons[moon].Adjustments.Count; i++) {
                        if (index != i) fireOffset += ApplyObject(moon, i, container.transform, fireOffset);
                    }
                    break;
                case ConfigControl.Setting.RandomA:
                    if (index != 0) {
                        if (Fifty_Fifty(moon, (int)(randomSeed * 5 / 3.0f), 0)) ApplyObject(moon, 0, container.transform, fireOffset);
                    }
                    break;
                case ConfigControl.Setting.RandomB:
                    if (index != 1) {
                        if (Fifty_Fifty(moon, (int)(randomSeed * 5 / 3.0f), 1)) ApplyObject(moon, 1, container.transform, fireOffset);
                    }
                    break;
                case ConfigControl.Setting.RandomC:
                    if (index != 2) {
                        if (Fifty_Fifty(moon, (int)(randomSeed * 5 / 3.0f), 2)) ApplyObject(moon, 2, container.transform, fireOffset);
                    }
                    break;
                case ConfigControl.Setting.RandomAny:
                    int second = RandomIndex(moon, randomSeed, vanilla);
                    if (second == index || second < 0 || second >= MapImprovementModBase.Instance.Moons[moon].Adjustments.Count) return;
                    ApplyObject(moon, second, container.transform, fireOffset);
                    break;
                case ConfigControl.Setting.RandomAll:
                    for (int i = 0; i < MapImprovementModBase.Instance.Moons[moon].Adjustments.Count; i++) {
                        if (index != i) {
                            if (ConfigControl.Instance.cfgMoons[moon].cfgObjects[i].Settings.Equals(ConfigControl.Setting.Always)) continue;
                            if (Fifty_Fifty(moon, (int)(randomSeed * 5 / (2.0f + i)), i)) ApplyObject(moon, i, container.transform, fireOffset);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        //Random from enabled improvements
        static int RandomIndex(int moon, int seed, bool vanilla)
        {
            //Use the random seed to determine which adjustment to spawn, if there are multiple
            byte mod = 0;
            for (int i = 0; i < ConfigControl.Instance.cfgMoons[moon].cfgObjects.Length; i++) {
                if (ConfigControl.Instance.cfgMoons[moon].cfgObjects[i].Settings.Equals(ConfigControl.Setting.Always)) continue;
                if (!ConfigControl.Instance.cfgMoons[moon].cfgObjects[i].Settings.Equals(ConfigControl.Setting.Disabled)) {
                    if (!ConfigControl.Instance.cfgMoons[moon].cfgObjects[i].Settings.Equals(ConfigControl.Setting.Never)) mod += (byte)Mathf.Pow(2, i);
                }
            }
            if (mod > 0) {
                if (vanilla) mod += 8;
                switch (mod) {
                    case 1:
                        return 0;
                    case 2:
                        return 1;
                    case 3:
                        return (seed % 2);
                    case 4:
                        return 2;
                    case 5:
                        return (seed % 2) * 2;
                    case 6:
                        return (seed % 2) + 1;
                    case 7:
                        return (seed % 3);
                    //Vanilla+
                    case 9:
                        return (seed % 2) - 1;
                    case 10:
                        return (seed % 2) * 2 - 1;
                    case 11:
                        return (seed % 3) - 1;
                    case 12:
                        return (seed % 2) * 3 - 1;
                    case 13:
                        return (seed % 3) * 2 - 2;
                    case 14:
                        return (seed % 3) + 1;
                    case 15:
                        return (seed % 4);
                }
            }
            return -1;
        }

        //Roll for spawning the desired index 50/50
        static bool Fifty_Fifty(int moon, int seed, int index)
        {
            if (!ConfigControl.Instance.cfgMoons[moon].cfgObjects[index].Settings.Equals(ConfigControl.Setting.Disabled)) return (seed % 2) == 0;
            else return false;
        }

        //Simplifed spawn and adjust
        static int ApplyObject(int moon, int index, Transform parent, int fireOffset = 0)
        {
            SpawnObject(moon, index, parent);
            if (MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit == null || MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit.Count < 1) return 0;
            return AdjustObject(moon, index, fireOffset);
        }

        //Spawn the object if not null
        static void SpawnObject(int moon, int index, Transform parent)
        {
            if (MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Object != null) {
                GameObject.Instantiate(MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Object, parent, true);
                MapImprovementModBase.mls.LogInfo($"Spawned in " + MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Object.name);
            } else MapImprovementModBase.mls.LogWarning($"Moon's object was null!");
        }

        //Loop through the adjustments and apply to all objects that match the requirements
        static int AdjustObject(int moon, int index, int fireOffset = 0)
        {
            int _return = 0;
            for (int i = 0; i < MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit.Count; i++) _return += FindObject(MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i], fireOffset);
            return _return;
        }

        //Find object in given edit
        static int FindObject(MapImprovementModBase.Edits edit, int fireOffset = 0)
        {
            if (edit.Do.Equals(MapImprovementModBase.EditEnums.Enable) || edit.If.Equals(MapImprovementModBase.EditEnums.Enable)) {
                GameObject[] inactive = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int j = 0; j < inactive.Length; j++) {
                    if (inactive[j].name.Equals(edit.Name) && inactive[j].tag.Equals(edit.Tag)) return ApplyEnum(inactive[j], edit, fireOffset);
                }
            } else {
                if (edit.Tag.Equals("Untagged")) {
                    GameObject found = GameObject.Find(edit.Name);
                    if (found == null) {
                        MapImprovementModBase.mls.LogError($"No Untagged object matched the name {edit.Name}! Skipping");
                        return 0;
                    }
                    if (found.tag.Equals(edit.Tag)) return ApplyEnum(found, edit, fireOffset);
                } else {
                    GameObject[] find = GameObject.FindGameObjectsWithTag(edit.Tag);
                    for (int j = 0; j < find.Length; j++) {
                        if (find[j].name.Equals(edit.Name)) return ApplyEnum(find[j], edit, fireOffset);
                    }
                }
            }
            return 0;
        }

        //Apply the edit enum
        static int ApplyEnum(GameObject find, MapImprovementModBase.Edits edit, int fireOffset)
        {
            if (find == null) return 0;
            bool global = edit.Global;
            if (edit.Do.Equals(MapImprovementModBase.EditEnums.IfFound)) {
                MapImprovementModBase.mls.LogInfo($"{find.name} was found, applying enum {edit.If.Do}");
                MapImprovementModBase.Edits onFound = new MapImprovementModBase.Edits(edit.If.Name, edit.If.Tag, edit.If.Do, edit.Postion, edit.Rotation, edit.Scale, edit.Global, edit.FireExitIndex);
                return FindObject(onFound, fireOffset);
            }
            switch (edit.Do) {
                case MapImprovementModBase.EditEnums.Move:
                    MapImprovementModBase.mls.LogInfo($"Moved {find.name} {(global ? $"globally" : $"locally")}");
                    if (global) find.transform.position = edit.Postion;
                    else find.transform.localPosition = edit.Postion;
                    return 0;
                case MapImprovementModBase.EditEnums.Rotate:
                    MapImprovementModBase.mls.LogInfo($"Rotated {find.name} {(global ? $"globally" : $"locally")}");
                    if (global) find.transform.eulerAngles = edit.Rotation;
                    else find.transform.localEulerAngles = edit.Rotation;
                    return 0;
                case MapImprovementModBase.EditEnums.Scale:
                    MapImprovementModBase.mls.LogInfo($"Scaled {find.name} {(global ? $"globally" : $"locally")}");
                    if (global) SetGlobalScale(find, edit.Scale);
                    else find.transform.localScale = edit.Scale;
                    return 0;
                case MapImprovementModBase.EditEnums.AllTransforms:
                    MapImprovementModBase.mls.LogInfo($"Transformed {find.name} {(global ? $"globally" : $"locally")}");
                    if (global) {
                        find.transform.position = edit.Postion;
                        find.transform.eulerAngles = edit.Rotation;
                        if (edit.Scale == default) return 0;
                        SetGlobalScale(find, edit.Scale);
                    } else {
                        find.transform.localPosition = edit.Postion;
                        find.transform.localEulerAngles = edit.Rotation;
                        if (edit.Scale == default) return 0;
                        find.transform.localScale = edit.Scale;
                    }
                    return 0;
                case MapImprovementModBase.EditEnums.Destroy:
                    MapImprovementModBase.mls.LogWarning($"Destroyed {find.name}");
                    GameObject.Destroy(find);
                    return 0;
                case MapImprovementModBase.EditEnums.FireExit:
                    MapImprovementModBase.mls.LogInfo($"New Fire Exit #{edit.FireExitIndex + fireOffset}");
                    GameObject fire = GameObject.Instantiate(find, find.transform.parent);
                    fire.name = "EntranceTeleport" + (edit.FireExitIndex + fireOffset);
                    if (global) {
                        fire.transform.position = edit.Postion;
                        fire.transform.eulerAngles = edit.Rotation;
                    } else {
                        fire.transform.localPosition = edit.Postion;
                        fire.transform.localEulerAngles = edit.Rotation;
                    }
                    if (fire.TryGetComponent<EntranceTeleport>(out EntranceTeleport script)) {
                        MapImprovementModBase.mls.LogInfo($"Added new entrance {(global ? $"globally" : $"locally")}");
                        script.entranceId = (edit.FireExitIndex + fireOffset);
                        return 1;
                    }
                    else return 0;
                case MapImprovementModBase.EditEnums.Clone:
                    GameObject clone = GameObject.Instantiate(find, find.transform.parent);
                    if (edit.FireExitIndex != 0) clone.name = $"{clone.name} {edit.FireExitIndex}";
                    MapImprovementModBase.mls.LogInfo($"Cloned {find.name} as {clone.name} {(global ? $"globally" : $"locally")}");
                    if (global) {
                        clone.transform.position = edit.Postion;
                        clone.transform.eulerAngles = edit.Rotation;
                        if (edit.Scale == default) return 0;
                        SetGlobalScale(clone, edit.Scale);
                    } else {
                        clone.transform.localPosition = edit.Postion;
                        clone.transform.localEulerAngles = edit.Rotation;
                        if (edit.Scale == default) return 0;
                        clone.transform.localScale = edit.Scale;
                    }
                    return 0;
                case MapImprovementModBase.EditEnums.Enable:
                    MapImprovementModBase.mls.LogInfo($"Enabled {find.name}");
                    find.SetActive(true);
                    return 0;
                case MapImprovementModBase.EditEnums.Disable:
                    MapImprovementModBase.mls.LogInfo($"Disabled {find.name}");
                    find.SetActive(false);
                    return 0;
                case MapImprovementModBase.EditEnums.Water:
                    if (find.TryGetComponent<Rigidbody>(out _)) {
                        MapImprovementModBase.mls.LogInfo($"Added water script to {find.name}");
                        QuicksandTrigger watertrigger = find.AddComponent<QuicksandTrigger>();
                        watertrigger.isWater = true;
                        watertrigger.audioClipIndex = 1;
                        watertrigger.movementHinderance = 0.6f;
                        watertrigger.sinkingSpeedMultiplier = 0.08f;
                    }
                    if (find.TryGetComponent<MeshRenderer>(out MeshRenderer render)) {
                        MapImprovementModBase.mls.LogInfo($"Added water material to {find.name}");
                        if (WaterMat != null) render.material = WaterMat;
                        else render.enabled = false;
                    }
                    return 0;
                case MapImprovementModBase.EditEnums.Reverb:
                    int dex = edit.FireExitIndex;
                    if (find.TryGetComponent<AudioReverbTrigger>(out AudioReverbTrigger change)) {
                        MapImprovementModBase.mls.LogInfo($"Adding reverb {MapImprovementModBase.ReverbNames[dex]} to {find.name}");
                        change.reverbPreset = MapImprovementModBase.Instance.reverbAssets[dex];
                        if (change.audioChanges == null) change.audioChanges = new switchToAudio[0];
                    } else {
                        MapImprovementModBase.mls.LogInfo($"Adding reverb {MapImprovementModBase.ReverbNames[dex]} & script to {find.name}");
                        AudioReverbTrigger add = find.AddComponent<AudioReverbTrigger>();
                        add.reverbPreset = MapImprovementModBase.Instance.reverbAssets[dex];
                        add.audioChanges = new switchToAudio[0];
                    }
                    return 0;
                case MapImprovementModBase.EditEnums.HasTrees:
                    MapImprovementModBase.mls.LogInfo($"Adding script to all {edit.Name} without one...");
                    GameObject[] trees = GameObject.FindGameObjectsWithTag(edit.Tag);
                    for (int t = 0; t < trees.Length; t++) {
                        if (trees[t].name.ToLower().Equals((edit.Name).ToLower())) {
                            if (!trees[t].TryGetComponent<Collider>(out _)) continue;
                            if (!trees[t].TryGetComponent<TerrainObstacleTrigger>(out _)) trees[t].AddComponent<TerrainObstacleTrigger>();
                        }
                    }
                    return 0;
                case MapImprovementModBase.EditEnums.IfFound:
                    //Never needed unless someone is dumb
                    MapImprovementModBase.mls.LogWarning($"What are you trying to do here?");
                    return 0;
                case MapImprovementModBase.EditEnums.Hazards:
                    MapImprovementModBase.mls.LogInfo($"Adding hazards to {find.name}");
                    if (!find.TryGetComponent<RandomMapObject>(out _)) {
                        RandomMapObject rmo = find.AddComponent<RandomMapObject>();
                        if (edit.FireExitIndex > 0) rmo.spawnRange = edit.FireExitIndex;
                    }
                    return 0;
                case MapImprovementModBase.EditEnums.KillTrigger:
                    MapImprovementModBase.mls.LogInfo($"Adding kill trigger to {find.name}");
                    KillLocalPlayer killer = find.AddComponent<KillLocalPlayer>();
                    InteractTrigger interact = find.AddComponent<InteractTrigger>();
                    //interact.onInteract.AddListener(killer.KillPlayer);
                    return 0;
                default:
                    MapImprovementModBase.mls.LogError($"Error regarding Edit Enum: Enum set to {edit.Do}");
                    return 0;
            }
        }

        //Set scale on global
        static void SetGlobalScale(GameObject find, Vector3 scale)
        {
            find.transform.localScale = Vector3.one;
            find.transform.localScale = new Vector3(scale.x / find.transform.lossyScale.x, scale.y / find.transform.lossyScale.y, scale.z / find.transform.lossyScale.z);
        }

        //Get moon index
        static int GetMoonIndex(string nameLevel)
        {
            //Check for planet name
            string[] level = nameLevel.Split(new[] { "level" }, System.StringSplitOptions.RemoveEmptyEntries);
            switch (level[0]) {
                case "experimentation":
                    return 0;
                case "assurance":
                    return 1;
                case "vow":
                    return 2;
                case "offense":
                    return 3;
                case "march":
                    return 4;
                case "adamance":
                    return 5;
                case "rend":
                    return 6;
                case "dine":
                    return 7;
                case "titan":
                    return 8;
                case "artifice":
                    return 9;
                case "embrion":
                    return 10;
                case "companybuilding":
                    return 11;
                default:
                    return MapImprovementModBase.Instance.Moons.FindIndex(x => x.Planet.ToLower().Equals(level[0]));
            }
        }

        //Find random map object spawners and add the
        static void OutsideHazards(SelectableLevel level)
        {
            RandomMapObject[] array = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();
            for (int i = 0; i < level.spawnableMapObjects.Length; i++) {
                for (int j = 0; j < array.Length; j++) {
                    if (!array[j].spawnablePrefabs.Contains(level.spawnableMapObjects[i].prefabToSpawn)) {
                        array[j].spawnablePrefabs.Add(level.spawnableMapObjects[i].prefabToSpawn);
                    }
                }
            }
        }
    }
}