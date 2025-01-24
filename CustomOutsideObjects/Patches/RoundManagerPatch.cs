using HarmonyLib;
using System.Collections.Generic;

namespace CustomOutsideObjects.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void ModifyTags()
        {
            for (int i = 0; i < CustomOutsideModBase.Instance.loadedInjectableOutsideObjects.Count; i++) {
                CustomOutsideModBase.mls.LogInfo($"Updating {CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[i].spawnableObject.name}'s tags.");
                string[] tags = ConfigControl.Instance.objects[i].NewTags.Split(new[] { ", ", ",", " " }, System.StringSplitOptions.RemoveEmptyEntries);
                CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[i].spawnableObject.spawnableFloorTags = new string[tags.Length];
                for (int t = 0; t < tags.Length; t++) CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[i].spawnableObject.spawnableFloorTags[t] = tags[t];
            }
        }

        [HarmonyPatch("SpawnOutsideHazards")]
        [HarmonyPrefix]
        static void OutsideHazardPatch(ref SelectableLevel ___currentLevel)
        {
            string namePlanet = ___currentLevel.PlanetName.ToLower().Trim();
            string nameLevel = ___currentLevel.name.ToLower().Trim();
            string[] planet = namePlanet.Split(' ');
            string[] level = nameLevel.Split(new[] { "level", "selectable" }, System.StringSplitOptions.RemoveEmptyEntries);
            CustomOutsideModBase.mls.LogWarning($"Planet {level[0]}.");
            //Modified custom object list accounting for enabled & blacklist
            List<SpawnableOutsideObjectWithRarity> modifiedList = new List<SpawnableOutsideObjectWithRarity>();
            for (int valid = 0; valid < CustomOutsideModBase.Instance.loadedInjectableOutsideObjects.Count; valid++) {
                //Disabled object is skipped immediately
                if (!ConfigControl.Instance.objects[valid].Enabled) continue;
                //If matching name is found, skip ahead to add it to the list
                string[] moons = ConfigControl.Instance.objects[valid].Moons.Split(new[] {", ", ",", " "}, System.StringSplitOptions.RemoveEmptyEntries);
                for (int m = 0; m < moons.Length; m++) {
                    if (moons[m].ToLower().Equals(level[0])) goto ADDOBJECT;
                    for (int p = 0; p < planet.Length; p++) {
                        if (moons[m].ToLower().Equals(planet[p])) goto ADDOBJECT;
                    }
                }
                //no matches found
                continue;
                //If the object is already queued up, skip it
            ADDOBJECT:
                if (modifiedList.Contains(CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[valid])) continue;
                //If the object is already in the level's object list, skip it
                for (int i = 0; i < ___currentLevel.spawnableOutsideObjects.Length; i++) {
                    if (___currentLevel.spawnableOutsideObjects[i].Equals(CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[valid])) goto CONTINUELOOP;
                    if (___currentLevel.spawnableOutsideObjects[i].spawnableObject.name.ToLower().Equals(CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[valid].spawnableObject.name.ToLower())) goto CONTINUELOOP;
                }
                //Add item to list if it has made it this far
                modifiedList.Add(CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[valid]);
                CustomOutsideModBase.mls.LogInfo($"Added {CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[valid].spawnableObject.name} as potential spawns.");
                continue;
            //GOTO for continuing original loop
            CONTINUELOOP:
                continue;
            }
            //Add the current level's map objects to the injection list
            SpawnableOutsideObjectWithRarity[] currentLevelOutsideObjects = ___currentLevel.spawnableOutsideObjects;
            for (int map = 0; map < currentLevelOutsideObjects.Length; map++) modifiedList.Add(currentLevelOutsideObjects[map]);
            ___currentLevel.spawnableOutsideObjects = modifiedList.ToArray();
            //Print objects for debugging
            for (int i = 0; i < ___currentLevel.spawnableOutsideObjects.Length; i++) {
                CustomOutsideModBase.mls.LogInfo($"#{i}: {___currentLevel.spawnableOutsideObjects[i].spawnableObject.name}");
            }
        }
    }
}