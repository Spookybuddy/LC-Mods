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
            //Skip if level has been preset in start of round
            switch (level[0]) {
                case "experimentation":
                    if (CustomOutsideModBase.Set_Experimentation) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "assurance":
                    if (CustomOutsideModBase.Set_Assurance) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "vow":
                    if (CustomOutsideModBase.Set_Vow) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "offense":
                    if (CustomOutsideModBase.Set_Offense) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "march":
                    if (CustomOutsideModBase.Set_March) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "adamance":
                    if (CustomOutsideModBase.Set_Adamance) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "rend":
                    if (CustomOutsideModBase.Set_Rend) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "dine":
                    if (CustomOutsideModBase.Set_Dine) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "titan":
                    if (CustomOutsideModBase.Set_Titan) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "embrion":
                    if (CustomOutsideModBase.Set_Embrion) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "artifice":
                    if (CustomOutsideModBase.Set_Artifice) { CustomOutsideModBase.mls.LogInfo($"Planet {level[0]} was pre-loaded."); return; }
                    break;
                case "companybuilding":
                    return;
                default:
                    break;
            }
            CustomOutsideModBase.mls.LogWarning($"Planet {level[0]} was not pre-loaded. Adding assets now.");
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
                        return;
                    default:
                        //Custom Moons
                        for (int i = 0; i < CustomOutsideModBase.Instance.Configuration.allObjects[valid].cfgCustomMoons.Length; i++) {
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