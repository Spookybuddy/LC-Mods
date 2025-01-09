using HarmonyLib;
using System.Collections.Generic;

namespace CustomOutsideObjects.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
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
            for (int valid = 0; valid < CustomOutsideModBase.loadedInjectableOutsideObjects.Count; valid++) {
                //Disabled object is skipped immediately
                if (!ConfigControl.Instance.allObjects[valid].Enabled) continue;
                //Base game moons enabled
                switch (level[0]) {
                    case "experimentation":
                        if (!ConfigControl.Instance.allObjects[valid].Exp) continue;
                        break;
                    case "assurance":
                        if (!ConfigControl.Instance.allObjects[valid].Ass) continue;
                        break;
                    case "vow":
                        if (!ConfigControl.Instance.allObjects[valid].Vow) continue;
                        break;
                    case "offense":
                        if (!ConfigControl.Instance.allObjects[valid].Off) continue;
                        break;
                    case "march":
                        if (!ConfigControl.Instance.allObjects[valid].Mar) continue;
                        break;
                    case "adamance":
                        if (!ConfigControl.Instance.allObjects[valid].Ada) continue;
                        break;
                    case "rend":
                        if (!ConfigControl.Instance.allObjects[valid].Ren) continue;
                        break;
                    case "dine":
                        if (!ConfigControl.Instance.allObjects[valid].Din) continue;
                        break;
                    case "titan":
                        if (!ConfigControl.Instance.allObjects[valid].Tit) continue;
                        break;
                    case "embrion":
                        if (!ConfigControl.Instance.allObjects[valid].Emb) continue;
                        break;
                    case "artifice":
                        if (!ConfigControl.Instance.allObjects[valid].Art) continue;
                        break;
                    case "companybuilding":
                        return;
                    default:
                        //Custom Moons
                        for (int i = 0; i < ConfigControl.Instance.allObjects[valid].cfgCustomMoons.Length; i++) {
                            for (int p = 0; p < planet.Length; p++) {
                                if (ConfigControl.Instance.allObjects[valid].cfgCustomMoons[i].MoonName.ToLower().Equals(planet[p])) {
                                    if (!ConfigControl.Instance.allObjects[valid].cfgCustomMoons[i].Custom) goto CONTINUELOOP;
                                }
                            }
                            if (ConfigControl.Instance.allObjects[valid].cfgCustomMoons[i].MoonName.ToLower().Equals(level[0])) {
                                if (!ConfigControl.Instance.allObjects[valid].cfgCustomMoons[i].Custom) goto CONTINUELOOP;
                            }
                        }
                        break;
                }
                //If the object is already queued up, skip it
                if (modifiedList.Contains(CustomOutsideModBase.loadedInjectableOutsideObjects[valid])) continue;
                //If the object is already in the level's object list, skip it
                for (int i = 0; i < ___currentLevel.spawnableOutsideObjects.Length; i++) {
                    if (___currentLevel.spawnableOutsideObjects[i].Equals(CustomOutsideModBase.loadedInjectableOutsideObjects[valid])) goto CONTINUELOOP;
                    if (___currentLevel.spawnableOutsideObjects[i].spawnableObject.name.ToLower().Equals(CustomOutsideModBase.loadedInjectableOutsideObjects[valid].spawnableObject.name.ToLower())) goto CONTINUELOOP;
                }
                //Add item to list if it has made it this far
                modifiedList.Add(CustomOutsideModBase.loadedInjectableOutsideObjects[valid]);
                CustomOutsideModBase.mls.LogInfo($"Added {CustomOutsideModBase.loadedInjectableOutsideObjects[valid].spawnableObject.name} as potential spawns.");
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