using BepInEx.Configuration;

namespace MapImprovements
{
    internal class ConfigControl : SyncedInstance<ConfigControl>
    {
        public ConfigEntry<bool> cfgModEnabled;
        public MoonConfig[] cfgMoons;
        public enum Setting
        {
            Enabled,
            Disabled,
            Always,
            Never,
            RandomA,
            RandomB,
            RandomC,
            RandomAny,
            RandomAll,
            CombineA,
            CombineB,
            CombineC,
            CombineAll
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
            public ConfigEntry<bool> cfgOverrideRarities;
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
            internal bool OverrideOdds
            {
                get => cfgOverrideRarities.Value;
                set => cfgOverrideRarities.Value = value;
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
            cfgModEnabled = cfg.Bind("! Basics !", "Enable Map Improvements", true, "Turns the mod on.");
            _ = cfg.Bind("! Basics !", "Guide to Dropdown:", Setting.Enabled, "Enabled: Object can spawn.\nDiabled: Object cannot spawn.\nAlways: Object always spawns.\nNever: Object can only be spawned when Combined/Randomed from another Object.\nCombine A/B/C: Spawns Object A/B/C too.\nCombineAll: Spawns all other Objects.\nRandom A/B/C: 50% to spawn Object A/B/C too.\nRandomAny: 50% to spawn any other Object.\nRandomAll: 50% chance to spawn each other Object");
            for (int i = 0; i < cfgMoons.Length; i++) {
                if (MapImprovementModBase.Instance.Moons[i].Adjustments == null || MapImprovementModBase.Instance.Moons[i].Adjustments.Count < 1) continue;
                string name = MapImprovementModBase.Instance.Moons[i].Planet;
                name = name[0].ToString().ToUpper() + name.Substring(1);
                cfgMoons[i].cfgEnabled = cfg.Bind(name, "Enabled", true, $"Enable improvements spawning on {name}");
                cfgMoons[i].cfgIncludeDefault = cfg.Bind(name, "Vanilla", true, $"Adds chance for the original {name} to spawn.");
                cfgMoons[i].cfgOverrideRarities = cfg.Bind(name, "Flat Odds", false, "Overrides the individual settings and gives even odds to every possible combination. Disable Vanilla if you do not want to include it.");
                cfgMoons[i].cfgObjects = new ObjectConfig[MapImprovementModBase.Instance.Moons[i].Adjustments.Count];
                for (int j = 0; j < MapImprovementModBase.Instance.Moons[i].Adjustments.Count; j++) {
                    if (MapImprovementModBase.Instance.Moons[i].Adjustments[j].Object == null) continue;
                    cfgMoons[i].cfgObjects[j].cfgSetting = cfg.Bind(name, MapImprovementModBase.Instance.Moons[i].Adjustments[j].Object.name.Replace('_', ' '), MapImprovementModBase.Instance.Moons[i].Adjustments[j].Default, MapImprovementModBase.Instance.Moons[i].Adjustments[j].Description);
                }
            }
            MapImprovementModBase.mls.LogInfo($"Generated Config file for all loaded objects.");
        }
    }
}