using HarmonyLib;
using System.Collections.Generic;

namespace CustomOutsideObjects.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPatch(StartOfRound __instance)
        {
            CustomOutsideModBase.mls.LogWarning($"Finding all levels...");
            //On startup perform the same checks for all the selectable levels as a safety precaution
            for (int x = 0; x < __instance.levels.Length; x++) {
                SelectableLevel currentLevel = __instance.levels[x];
                string namePlanet = currentLevel.PlanetName.ToLower().Trim();
                //string nameIcon = currentLevel.levelIconString.ToLower().Trim();
                string nameLevel = currentLevel.name.ToLower().Trim();
                string[] planet = namePlanet.Split(' ');
                //string[] icon = nameIcon.Split(new[] { "map" }, System.StringSplitOptions.RemoveEmptyEntries);
                string[] level = nameLevel.Split(new[] { "level" }, System.StringSplitOptions.RemoveEmptyEntries);
                //CustomOutsideModBase.mls.LogWarning(namePlanet + " / " + nameIcon + " / " + nameLevel);
                //Modified custom object list accounting for enabled & blacklist
                List<SpawnableOutsideObjectWithRarity> modifiedList = new List<SpawnableOutsideObjectWithRarity>();
                for (int valid = 0; valid < CustomOutsideModBase.loadedInjectableOutsideObjects.Count; valid++) {
                    //Disabled object is skipped immediately
                    if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Enabled) continue;
                    //Base game moons enabled
                    switch (level[0]) {
                        case "experimentation":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Exp) continue;
                            break;
                        case "assurance":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Ass) continue;
                            break;
                        case "vow":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Vow) continue;
                            break;
                        case "offense":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Off) continue;
                            break;
                        case "march":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Mar) continue;
                            break;
                        case "adamance":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Ada) continue;
                            break;
                        case "rend":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Ren) continue;
                            break;
                        case "dine":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Din) continue;
                            break;
                        case "titan":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Tit) continue;
                            break;
                        case "embrion":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Emb) continue;
                            break;
                        case "artifice":
                            if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].Art) continue;
                            break;
                        case "companybuilding":
                            continue;
                        default:
                            //Custom Moons
                            for (int i = 0; i < CustomOutsideModBase.Instance.Configuration.allObjects[valid].cfgCustomMoons.Length; i++) {
                                //Check both planet name as well as level name
                                for (int p = 0; p < planet.Length; p++) {
                                    if (CustomOutsideModBase.Instance.Configuration.allObjects[valid].cfgCustomMoons[i].MoonName.ToLower().Equals(planet[p])) {
                                        if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].cfgCustomMoons[i].Custom) goto CONTINUELOOP;
                                    }
                                }
                                if (CustomOutsideModBase.Instance.Configuration.allObjects[valid].cfgCustomMoons[i].MoonName.ToLower().Equals(level[0])) {
                                    if (!CustomOutsideModBase.Instance.Configuration.allObjects[valid].cfgCustomMoons[i].Custom) goto CONTINUELOOP;
                                }
                            }
                            break;
                    }
                    //If the object is already added, skip it
                    if (modifiedList.Contains(CustomOutsideModBase.loadedInjectableOutsideObjects[valid])) continue;
                    //If the object is already in the level's object list, skip it
                    for (int i = 0; i < currentLevel.spawnableOutsideObjects.Length; i++) {
                        //Compare objects directly, and also compare object names as a double check
                        if (currentLevel.spawnableOutsideObjects[i].Equals(CustomOutsideModBase.loadedInjectableOutsideObjects[valid])) goto CONTINUELOOP;
                        if (currentLevel.spawnableOutsideObjects[i].spawnableObject.name.ToLower().Equals(CustomOutsideModBase.loadedInjectableOutsideObjects[valid].spawnableObject.name.ToLower())) goto CONTINUELOOP;
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
                SpawnableOutsideObjectWithRarity[] currentLevelOutsideObjects = currentLevel.spawnableOutsideObjects;
                for (int map = 0; map < currentLevelOutsideObjects.Length; map++) modifiedList.Add(currentLevelOutsideObjects[map]);
                //Return the modified list to the current level, accounting for existing objects as well as the config settings
                currentLevel.spawnableOutsideObjects = modifiedList.ToArray();
                __instance.levels[x] = currentLevel;
                //Mark planets as preset to skip checking on every round
                switch (level[0]) {
                    case "experimentation":
                        CustomOutsideModBase.Set_Experimentation = true;
                        break;
                    case "assurance":
                        CustomOutsideModBase.Set_Assurance = true;
                        break;
                    case "vow":
                        CustomOutsideModBase.Set_Vow = true;
                        break;
                    case "offense":
                        CustomOutsideModBase.Set_Offense = true;
                        break;
                    case "march":
                        CustomOutsideModBase.Set_March = true;
                        break;
                    case "adamance":
                        CustomOutsideModBase.Set_Adamance = true;
                        break;
                    case "rend":
                        CustomOutsideModBase.Set_Rend = true;
                        break;
                    case "dine":
                        CustomOutsideModBase.Set_Dine = true;
                        break;
                    case "titan":
                        CustomOutsideModBase.Set_Titan = true;
                        break;
                    case "embrion":
                        CustomOutsideModBase.Set_Embrion = true;
                        break;
                    case "artifice":
                        CustomOutsideModBase.Set_Artifice = true;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}