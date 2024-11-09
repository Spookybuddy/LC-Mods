using BepInEx.Configuration;

namespace MapImprovements
{
    internal class ConfigControl : SyncedInstance<ConfigControl>
    {
        public ConfigEntry<bool> cfgModEnabled;
        private ConfigEntry<Setting> cfgGuide;
        public MoonConfig[] cfgMoons;
        public enum Setting
        {
            Enabled,
            Disabled,
            Always,
            CombineFirst,
            CombineSecond,
            CombineThird,
            CombineAll,
            RandomFirst,
            RandomSecond,
            RandomThird,
            RandomAny
        }
        internal bool ModEnabled
        {
            get => cfgModEnabled.Value;
            set => cfgModEnabled.Value = value;
        }
        public struct MoonConfig
        {
            public ConfigEntry<bool> cfgEnabled;
            public ConfigEntry<bool> cfgIncludeDefault;
            public ObjectConfig[] cfgObjects;
            internal bool Enabled
            {
                get => cfgEnabled.Value;
                set => cfgEnabled.Value = value;
            }
            internal bool Vanilla
            {
                get => cfgIncludeDefault.Value;
                set => cfgIncludeDefault.Value = value;
            }
        }
        public struct ObjectConfig
        {
            public ConfigEntry<Setting> cfgSetting;
            internal Setting Settings
            {
                get {
                    if (cfgSetting != null) return cfgSetting.Value;
                    else return (Setting)cfgSetting.DefaultValue;
                }
                set => cfgSetting.Value = value;
            }
        }

        public ConfigControl(ConfigFile cfg)
        {
            InitInstance(this);
            cfgMoons = new MoonConfig[MapImprovementModBase.Instance.Moons.Count];
            cfgModEnabled = cfg.Bind("Basics", "Enable Map Improvements", true, "Turns the mod on.");
            cfgGuide = cfg.Bind("Basics", "Guide to Dropdown:", Setting.Enabled, "Enabled: Object can spawn.\nDiabled: Object cannot spawn.\nAlways: Object will always spawn.\nCombine 1/2/3: Spawns the 1/2/3 object alongside this one.\nCombineAll: Spawns all other objects.\nRandom 1/2/3: 50% to spawn the 1/2/3 object as well.\nRandomAny: 50% to spawn any other object.");
            for (int i = 0; i < cfgMoons.Length; i++) {
                if (MapImprovementModBase.Instance.Moons[i].Adjustments == null || MapImprovementModBase.Instance.Moons[i].Adjustments.Count < 1) continue;
                string name = MapImprovementModBase.Instance.Moons[i].Planet;
                name = name[0].ToString().ToUpper() + name.Substring(1);
                cfgMoons[i].cfgEnabled = cfg.Bind(name, "Enabled", true, "Enable improvements spawning on " + name);
                cfgMoons[i].cfgIncludeDefault = cfg.Bind(name, "Vanilla", true, "Adds chance for the vanilla " + name + " to spawn.");
                cfgMoons[i].cfgObjects = new ObjectConfig[MapImprovementModBase.Instance.Moons[i].Adjustments.Count];
                for (int j = 0; j < MapImprovementModBase.Instance.Moons[i].Adjustments.Count; j++) {
                    if (MapImprovementModBase.Instance.Moons[i].Adjustments[j].Object == null) continue;
                    cfgMoons[i].cfgObjects[j].cfgSetting = cfg.Bind(name, MapImprovementModBase.Instance.Moons[i].Adjustments[j].Object.name, Setting.Enabled, MapImprovementModBase.Instance.Moons[i].Adjustments[j].Description);
                }
            }
            MapImprovementModBase.mls.LogInfo($"Generated Config file for all loaded objects.");
        }
    }
}