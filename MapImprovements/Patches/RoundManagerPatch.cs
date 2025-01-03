﻿using HarmonyLib;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using System.Reflection;

namespace MapImprovements.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static Material WaterMat;
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void GetWaterMat()
        {
            WaterMat = GameObject.Find("TimeAndWeather").transform.GetChild(4).GetChild(1).GetComponent<MeshRenderer>().material;
        }

        [HarmonyPatch("SetChallengeFileRandomModifiers")]
        [HarmonyPrefix]
        static void ImproveCompany(RoundManager __instance)
        {
            if (!MapImprovementModBase.Instance.Configuration.ModEnabled) return;
            string nameLevel = __instance.currentLevel.name.ToLower().Trim();
            string[] level = nameLevel.Split(new[] { "level" }, System.StringSplitOptions.RemoveEmptyEntries);
            GameObject container = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (container == null) return;
            if (!level[0].Equals("companybuilding")) return;
            if (MapImprovementModBase.Instance.Moons[11].Adjustments == null || MapImprovementModBase.Instance.Moons[11].Adjustments.Count < 1) return;
            if (!MapImprovementModBase.Instance.Configuration.cfgMoons[11].Enabled) return;
            Mod(11, __instance.playersManager.randomMapSeed, container);
        }

        [HarmonyPatch("GenerateNewFloor")]
        [HarmonyPrefix]
        static void AddImprovements(RoundManager __instance)
        {
            //Exit if mod is disabled
            if (!MapImprovementModBase.Instance.Configuration.ModEnabled) return;
            //Check for planet name
            string nameLevel = __instance.currentLevel.name.ToLower().Trim();
            string[] level = nameLevel.Split(new[] { "level" }, System.StringSplitOptions.RemoveEmptyEntries);
            GameObject container = GameObject.FindGameObjectWithTag("MapPropsContainer");
            if (container == null) return;
            int moon;
            switch (level[0]) {
                case "experimentation":
                    moon = 0;
                    break;
                case "assurance":
                    moon = 1;
                    break;
                case "vow":
                    moon = 2;
                    break;
                case "offense":
                    moon = 3;
                    break;
                case "march":
                    moon = 4;
                    break;
                case "adamance":
                    moon = 5;
                    break;
                case "rend":
                    moon = 6;
                    break;
                case "dine":
                    moon = 7;
                    break;
                case "titan":
                    moon = 8;
                    break;
                case "artifice":
                    moon = 9;
                    break;
                case "embrion":
                    moon = 10;
                    break;
                case "companybuilding":
                    //Inaccessible, as the company does not call Generate floor
                    moon = 11;
                    break;
                default:
                    moon = MapImprovementModBase.Instance.Moons.FindIndex(x => x.Planet.ToLower().Equals(level[0]));
                    if (moon < 0) return;
                    break;
            }
            //Empty / Disabled; Exit
            if (MapImprovementModBase.Instance.Moons[moon].Adjustments == null || MapImprovementModBase.Instance.Moons[moon].Adjustments.Count < 1) return;
            if (!MapImprovementModBase.Instance.Configuration.cfgMoons[moon].Enabled) return;
            //Apply mod
            Mod(moon, __instance.playersManager.randomMapSeed, container);
            //Bake Navmesh for custom moons - Maybe also bake for no outside hazard spawns?
            if (moon > 11) {
                GameObject navigation = GameObject.FindGameObjectWithTag("OutsideLevelNavMesh");
                if (navigation != null) {
                    if (navigation.TryGetComponent<NavMeshSurface>(out NavMeshSurface build)) build.BuildNavMesh();
                }
            }
        }

        //Everything 
        static void Mod(int moon, int randomSeed, GameObject container)
        {
            //Check enum and randomize
            bool vanilla = MapImprovementModBase.Instance.Configuration.cfgMoons[moon].Vanilla;
            int fireOffset = 0;

            //Always spawning objects
            for (int i = 0; i < MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects.Length; i++) {
                if (MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects[i].Settings.Equals(ConfigControl.Setting.Always)) fireOffset += ApplyObject(moon, i, container.transform);
            }

            //Calculate the base spawn, exiting if the vanilla is selected / oob / disabled from spawning (but that should never happen)
            int index = RandomIndex(moon, randomSeed, vanilla);
            if (index < 0 || index >= MapImprovementModBase.Instance.Moons[moon].Adjustments.Count) return;
            if (MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects[index].Settings.Equals(ConfigControl.Setting.Disabled)) return;
            if (MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects[index].Settings.Equals(ConfigControl.Setting.Never)) return;
            fireOffset += ApplyObject(moon, index, container.transform);

            //New System
            switch (MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects[index].Settings) {
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
                default:
                    break;
            }
        }

        //Random from enabled improvements
        static int RandomIndex(int moon, int seed, bool vanilla)
        {
            //Use the random seed to determine which adjustment to spawn, if there are multiple
            byte mod = 0;
            for (int i = 0; i < MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects.Length; i++) {
                if (MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects[i].Settings.Equals(ConfigControl.Setting.Always)) continue;
                if (!MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects[i].Settings.Equals(ConfigControl.Setting.Disabled)) {
                    if (!MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects[i].Settings.Equals(ConfigControl.Setting.Never)) mod += (byte)Mathf.Pow(2, i);
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
            if (!MapImprovementModBase.Instance.Configuration.cfgMoons[moon].cfgObjects[index].Settings.Equals(ConfigControl.Setting.Disabled)) return (seed % 2) == 0;
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
            for (int i = 0; i < MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit.Count; i++) {
                //Search for inactive only when the task is to enable
                if (MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Do.Equals(MapImprovementModBase.EditEnums.Enable)) {
                    GameObject[] inactive = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(sr => !sr.gameObject.activeInHierarchy).ToArray();
                    for (int j = 0; j < inactive.Length; j++) {
                        if (inactive[j].tag.Equals(MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Tag)) {
                            _return += ApplyEnum(inactive[j], moon, index, i, fireOffset);
                            break;
                        }
                    }
                } else {
                    if (MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Tag.Equals("Untagged")) {
                        GameObject found = GameObject.Find(MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Name);
                        if (found.tag.Equals(MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Tag)) _return += ApplyEnum(found, moon, index, i, fireOffset);
                        continue;
                    }
                    GameObject[] find = GameObject.FindGameObjectsWithTag(MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Tag);
                    for (int j = 0; j < find.Length; j++) {
                        if (find[j].name.Equals(MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Name)) {
                            _return += ApplyEnum(find[j], moon, index, i, fireOffset);
                            break;
                        }
                    }
                }
            }
            return _return;
        }

        //Parse the enum
        static int ApplyEnum(GameObject find, int moon, int index, int i, int fireOffset)
        {
            if (find == null) return 0;
            switch (MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Do) {
                case MapImprovementModBase.EditEnums.Move:
                    MapImprovementModBase.mls.LogInfo($"Moved {find.name}");
                    find.transform.localPosition = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Postion;
                    return 0;
                case MapImprovementModBase.EditEnums.Rotate:
                    MapImprovementModBase.mls.LogInfo($"Rotated {find.name}");
                    find.transform.localEulerAngles = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Rotation;
                    return 0;
                case MapImprovementModBase.EditEnums.Scale:
                    MapImprovementModBase.mls.LogInfo($"Scaled {find.name}");
                    find.transform.localScale = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Scale;
                    return 0;
                case MapImprovementModBase.EditEnums.AllTransforms:
                    MapImprovementModBase.mls.LogInfo($"Transformed {find.name}");
                    find.transform.localPosition = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Postion;
                    find.transform.localEulerAngles = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Rotation;
                    if (MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Scale == default) return 0;
                    find.transform.localScale = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Scale;
                    return 0;
                case MapImprovementModBase.EditEnums.Destroy:
                    MapImprovementModBase.mls.LogWarning($"Destroyed {find.name}");
                    GameObject.Destroy(find);
                    return 0;
                case MapImprovementModBase.EditEnums.FireExit:
                    MapImprovementModBase.mls.LogInfo($"New Fire Exit");
                    GameObject fire = GameObject.Instantiate(find, find.transform.parent);
                    fire.name = "EntranceTeleport" + (MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].FireExitIndex + fireOffset);
                    fire.transform.localPosition = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Postion;
                    fire.transform.localEulerAngles = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Rotation;
                    if (fire.TryGetComponent<EntranceTeleport>(out EntranceTeleport script)) {
                        MapImprovementModBase.mls.LogInfo($"Added new entrance");
                        script.entranceId = (MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].FireExitIndex + fireOffset);
                        return 1;
                    } else return 0;
                case MapImprovementModBase.EditEnums.Clone:
                    MapImprovementModBase.mls.LogInfo($"Cloned {find.name}");
                    GameObject clone = GameObject.Instantiate(find, find.transform.parent);
                    clone.transform.localPosition = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Postion;
                    clone.transform.localEulerAngles = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Rotation;
                    if (MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Scale == default) return 0;
                    clone.transform.localScale = MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Scale;
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
                    MapImprovementModBase.mls.LogInfo($"Added reverb script to {find.name}");
                    return 0;
                case MapImprovementModBase.EditEnums.StoryLog:
                    MapImprovementModBase.mls.LogInfo($"New story log {find.name}");
                    return 0;
                case MapImprovementModBase.EditEnums.Bridge:
                    MapImprovementModBase.mls.LogInfo($"Breakable bridge {find.name}");
                    return 0;
                case MapImprovementModBase.EditEnums.HasTrees:
                    MapImprovementModBase.mls.LogInfo($"Adding script to all trees without one...");
                    GameObject[] trees = GameObject.FindGameObjectsWithTag("Wood");
                    for (int t = 0; t < trees.Length; t++) {
                        if (trees[t].name.ToLower().Equals("treebreaktrigger")) {
                            if (!trees[t].TryGetComponent<Collider>(out _)) continue;
                            if (!trees[t].TryGetComponent<TerrainObstacleTrigger>(out _)) trees[t].AddComponent<TerrainObstacleTrigger>();
                        }
                    }
                    return 0;
                default:
                    MapImprovementModBase.mls.LogError($"Error regarding Edit Enum: Enum set to {MapImprovementModBase.Instance.Moons[moon].Adjustments[index].Edit[i].Do}");
                    return 0;
            }
        }
    }
}