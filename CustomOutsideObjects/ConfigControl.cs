using System.Collections.Generic;
using BepInEx.Configuration;

namespace CustomOutsideObjects
{
    internal class ConfigControl : SyncedInstance<ConfigControl>
    {
        public ObjectConfig[] objects;
        public struct ObjectConfig
        {
            public ConfigEntry<bool> cfgEnabled;
            public ConfigEntry<string> cfgWhitelist;
            public ConfigEntry<string> cfgTagOverride;
            public bool Enabled {
                get {
                    return cfgEnabled.Value;
                }
                set {
                    cfgEnabled.Value = value;
                }
            }
            public string Moons {
                get {
                    if (cfgWhitelist == null) return (string)cfgWhitelist.DefaultValue;
                    else return cfgWhitelist.Value;
                }
                set {
                    cfgWhitelist.Value = value;
                }
            }
            public string NewTags {
                get {
                    if (cfgTagOverride == null) return (string)cfgTagOverride.DefaultValue;
                    else return cfgTagOverride.Value;
                }
                set {
                    cfgTagOverride.Value = value;
                }
            }
        }
        private List<string> moons = new List<string>();

        public ConfigControl(ConfigFile cfg)
        {
            InitInstance(this);
            //Custom moon checking enabled
            string names = "Base game:\nExperimentation, Assurance, Vow, Offense, March, Adamance, Rend, Dine, Titan, Embrion, Artifice";
            /*
            if (CustomOutsideModBase.Instance.customMoonList != null || CustomOutsideModBase.Instance.customMoonList.Count > 0) {
                CustomOutsideModBase.mls.LogInfo($"Added custom moons to configs.");
                names += "\n\nFound custom moons:\n";
                for (int i = 0; i < CustomOutsideModBase.Instance.customMoonList.Count - 1; i++) names += CustomOutsideModBase.Instance.customMoonList[i] + ", ";
                names += CustomOutsideModBase.Instance.customMoonList[CustomOutsideModBase.Instance.customMoonList.Count - 1];
            }
            */
            _ = cfg.Bind("-Valid Options", "Valid Moon Names", "", names);
            _ = cfg.Bind("-Valid Options", "Valid Tag Names", "", "Common tags:\nGravel, Grass, Snow, Rock, Concrete, Catwalk, Wood\n\nOther valid tags:\nUntagged, Metal, Carpet, Puddle, Aluminum");
            objects = new ObjectConfig[CustomOutsideModBase.Instance.loadedInjectableOutsideObjects.Count];
            for (int i = 0; i < objects.Length; i++) {
                string name = CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[i].spawnableObject.name;
                string defaultTags = string.Join(", ", CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[i].spawnableObject.spawnableFloorTags);
                moons.Clear();
                for (int j = 0; j < CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[i].spawnableObject.spawnableFloorTags.Length; j++) {
                    switch (CustomOutsideModBase.Instance.loadedInjectableOutsideObjects[i].spawnableObject.spawnableFloorTags[j]) {
                        case "Gravel":
                            //Exp Ass Off
                            if (!moons.Contains("Experimentation")) moons.Add("Experimentation");
                            if (!moons.Contains("Assurance")) moons.Add("Assurance");
                            if (!moons.Contains("Offense")) moons.Add("Offense");
                            break;
                        case "Grass":
                            //Vow Mar Ada Art
                            if (!moons.Contains("Vow")) moons.Add("Vow");
                            if (!moons.Contains("March")) moons.Add("March");
                            if (!moons.Contains("Adamance")) moons.Add("Adamance");
                            if (!moons.Contains("Artifice")) moons.Add("Artifice");
                            break;
                        case "Snow":
                            //Ren Din Tit
                            if (!moons.Contains("Rend")) moons.Add("Rend");
                            if (!moons.Contains("Dine")) moons.Add("Dine");
                            if (!moons.Contains("Titan")) moons.Add("Titan");
                            break;
                        case "Rock":
                            //Ada Emb
                            if (!moons.Contains("Adamance")) moons.Add("Adamance");
                            if (!moons.Contains("Embrion")) moons.Add("Embrion");
                            break;
                        case "Concrete":
                            //Exp Vow Tit Emb Art
                            if (!moons.Contains("Experimentation")) moons.Add("Experimentation");
                            if (!moons.Contains("Vow")) moons.Add("Vow");
                            if (!moons.Contains("Titan")) moons.Add("Titan");
                            if (!moons.Contains("Embrion")) moons.Add("Embrion");
                            if (!moons.Contains("Artifice")) moons.Add("Artifice");
                            break;
                        case "Catwalk":
                            //Exp Tit Art
                            if (!moons.Contains("Experimentation")) moons.Add("Experimentation");
                            if (!moons.Contains("Titan")) moons.Add("Titan");
                            if (!moons.Contains("Artifice")) moons.Add("Artifice");
                            break;
                        case "Wood":
                            //Art
                            if (!moons.Contains("Artifice")) moons.Add("Artifice");
                            break;
                        default:
                            //Tags must be spelt correctly. Add in all game tags found on moons
                            break;
                    }
                }
                string defaultMoons = string.Join(", ", moons.ToArray());
                objects[i].cfgEnabled = cfg.Bind(name, "Enabled", true, $"{name} can spawn.");
                objects[i].cfgWhitelist = cfg.Bind(name, "Spawn on Moons", defaultMoons, "What moons the object can spawn on.\nDefault moons match the Default Tags. Separate moon names with a comma.");
                objects[i].cfgTagOverride = cfg.Bind(name, "Spawn on Tags", defaultTags, "What surfaces the object can spawn on. Separate tags with a comma.");
            }
        }
    }
}