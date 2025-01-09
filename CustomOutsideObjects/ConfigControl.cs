using BepInEx.Configuration;

namespace CustomOutsideObjects
{
    internal class ConfigControl : SyncedInstance<ConfigControl>
    {
        public struct PerObjectConfig {
            public ConfigEntry<bool> cfgEnabled;
            public ConfigEntry<bool> cfgExp;
            public ConfigEntry<bool> cfgAss;
            public ConfigEntry<bool> cfgVow;
            public ConfigEntry<bool> cfgOff;
            public ConfigEntry<bool> cfgMar;
            public ConfigEntry<bool> cfgAda;
            public ConfigEntry<bool> cfgRen;
            public ConfigEntry<bool> cfgDin;
            public ConfigEntry<bool> cfgTit;
            public ConfigEntry<bool> cfgEmb;
            public ConfigEntry<bool> cfgArt;
            public CustomMoonConfig[] cfgCustomMoons;
            internal bool Enabled {
                get {
                    if (cfgEnabled.Value) return true;
                    return false;
                }
                set => cfgEnabled.Value = value;
            }
            internal bool Exp
            {
                get {
                    if (cfgExp != null) return cfgExp.Value;
                    return false;
                }
                set {
                    if (cfgExp != null) cfgExp.Value = value;
                }
            }
            internal bool Ass
            {
                get {
                    if (cfgAss != null) return cfgAss.Value;
                    return false;
                }
                set {
                    if (cfgAss != null) cfgAss.Value = value;
                }
            }
            internal bool Vow
            {
                get {
                    if (cfgVow != null) return cfgVow.Value;
                    return false;
                }
                set {
                    if (cfgVow != null) cfgVow.Value = value;
                }
            }
            internal bool Off
            {
                get
                {
                    if (cfgOff != null) return cfgOff.Value;
                    return false;
                }
                set {
                    if (cfgOff != null) cfgOff.Value = value;
                }
            }
            internal bool Mar
            {
                get {
                    if (cfgMar != null) return cfgMar.Value;
                    return false;
                }
                set {
                    if (cfgMar != null) cfgMar.Value = value;
                }
            }
            internal bool Ada
            {
                get {
                    if (cfgAda != null) return cfgAda.Value;
                    return false;
                }
                set {
                    if (cfgAda != null) cfgAda.Value = value;
                }
            }
            internal bool Ren
            {
                get {
                    if (cfgRen != null) return cfgRen.Value;
                    return false;
                }
                set {
                    if (cfgRen != null) cfgRen.Value = value;
                }
            }
            internal bool Din
            {
                get {
                    if (cfgDin != null) return cfgDin.Value;
                    return false;
                }
                set {
                    if (cfgDin != null) cfgDin.Value = value;
                }
            }
            internal bool Tit
            {
                get {
                    if (cfgTit != null) return cfgTit.Value;
                    return false;
                }
                set {
                    if (cfgTit != null) cfgTit.Value = value;
                }
            }
            internal bool Emb
            {
                get {
                    if (cfgEmb != null) return cfgEmb.Value;
                    return false;
                }
                set {
                    if (cfgEmb != null) cfgEmb.Value = value;
                }
            }
            internal bool Art
            {
                get {
                    if (cfgArt != null) return cfgArt.Value;
                    return false;
                }
                set {
                    if (cfgArt != null) cfgArt.Value = value;
                }
            }
        }
        public struct CustomMoonConfig
        {
            public string MoonName;
            public ConfigEntry<bool> cfgCustom;
            internal bool Custom
            {
                get {
                    if (cfgCustom != null) return cfgCustom.Value;
                    return false;
                }
                set {
                    if (cfgCustom != null) cfgCustom.Value = value;
                }
            }
        }
        public struct Tagged
        {
            internal bool[] valid;
        }
        private readonly bool usingCustomMoons;
        public PerObjectConfig[] allObjects;

        public ConfigControl(ConfigFile cfg)
        {
            InitInstance(this);
            //Bools to check tag types
            Tagged tags = new Tagged { valid = new bool[CustomOutsideModBase.tags.Length] };
            //Custom moon checking enabled
            if (CustomOutsideModBase.customMoonList != null || CustomOutsideModBase.customMoonList.Count > 0) {
                usingCustomMoons = true;
                CustomOutsideModBase.mls.LogInfo($"Added custom moons to configs.");
            } else {
                usingCustomMoons = false;
                CustomOutsideModBase.mls.LogInfo($"No custom moons were added to the config.");
            }
            //Create a new array of settings for each object loaded
            allObjects = new PerObjectConfig[CustomOutsideModBase.loadedInjectableOutsideObjects.Count];
            for (int i = 0; i < CustomOutsideModBase.loadedInjectableOutsideObjects.Count; i++) {
                for (int b = 0; b < tags.valid.Length; b++) tags.valid[b] = false;
                //List object's tags and what moons are valid for it to spawn on
                for (int t = 0; t < CustomOutsideModBase.loadedInjectableOutsideObjects[i].spawnableObject.spawnableFloorTags.Length; t++) {
                    for (int u = 0; u < CustomOutsideModBase.tags.Length; u++) {
                        if (CustomOutsideModBase.loadedInjectableOutsideObjects[i].spawnableObject.spawnableFloorTags[t].Equals(CustomOutsideModBase.tags[u])) {
                            tags.valid[u] = true;
                            break;
                        }
                    }
                }
                //Section is based on the object name, with an enabled setting appearing for every valid moon based on tags
                string name = CustomOutsideModBase.loadedInjectableOutsideObjects[i].spawnableObject.name;
                allObjects[i].cfgEnabled = cfg.Bind(name, "Enabled", true, "Prevent " + name + " from spawning entirely.");
                if (tags.valid[0]) allObjects[i].cfgExp = cfg.Bind(name, "Spawn on Experimentation", true, "Can spawn on Experimentation.");
                if (tags.valid[0]) allObjects[i].cfgAss = cfg.Bind(name, "Spawn on Assurance", true, "Can spawn on Assurance.");
                if (tags.valid[1] || tags.valid[4]) allObjects[i].cfgVow = cfg.Bind(name, "Spawn on Vow", true, "Can spawn on Vow.");
                if (tags.valid[0]) allObjects[i].cfgOff = cfg.Bind(name, "Spawn on Offense", true, "Can spawn on Offense.");
                if (tags.valid[1]) allObjects[i].cfgMar = cfg.Bind(name, "Spawn on March", true, "Can spawn on March.");
                if (tags.valid[1] || tags.valid[3]) allObjects[i].cfgAda = cfg.Bind(name, "Spawn on Adamance", true, "Can spawn on Adamance.");
                if (tags.valid[2]) allObjects[i].cfgRen = cfg.Bind(name, "Spawn on Rend", true, "Can spawn on Rend.");
                if (tags.valid[2]) allObjects[i].cfgDin = cfg.Bind(name, "Spawn on Dine", true, "Can spawn on Dine.");
                if (tags.valid[2] || tags.valid[4] || tags.valid[5]) allObjects[i].cfgTit = cfg.Bind(name, "Spawn on Titan", true, "Can spawn on Titan.");
                if (tags.valid[3] || tags.valid[4]) allObjects[i].cfgEmb = cfg.Bind(name, "Spawn on Embrion", true, "Can spawn on Embrion.");
                if (tags.valid[1] || tags.valid[4] || tags.valid[5] || tags.valid[6]) allObjects[i].cfgArt = cfg.Bind(name, "Spawn on Artifice", true, "Can spawn on Artifice.");
                //Custom moons section
                if (usingCustomMoons) {
                    allObjects[i].cfgCustomMoons = new CustomMoonConfig[CustomOutsideModBase.customMoonList.Count];
                    for (int c = 0; c < CustomOutsideModBase.customMoonList.Count; c++) {
                        string moon = CustomOutsideModBase.customMoonList[c];
                        moon = char.ToUpper(moon[0]) + moon.Substring(1);
                        allObjects[i].cfgCustomMoons[c].MoonName = moon;
                        allObjects[i].cfgCustomMoons[c].cfgCustom = cfg.Bind(name, "Spawn on " + moon, true, "Can spawn on " + moon + ". \n\nNOTE!\nThis does not mean the Object will be able spawn, as the Object's tags may not match the environment's.");
                    }
                }
            }
        }
    }
}