using BepInEx.Configuration;

namespace STFixes
{
    class ConfigControl : SyncedInstance<ConfigControl>
    {
        public ConfigEntry<bool> cfgModEnabled;
        public ConfigEntry<bool> cfgSTEnabled;
        public ConfigEntry<STFixModBase.EnumOptions> cfgTypes;
        public ConfigEntry<bool> cfgClamp;
        public ConfigEntry<float> cfgMin;
        public ConfigEntry<float> cfgMax;
        public ConfigEntry<bool> cfgScan;
        public ConfigEntry<bool> cfgMove;
        public ConfigEntry<int> cfgRange;
        internal bool Mod
        {
            get
            {
                if (cfgModEnabled.Value) return true;
                return false;
            }
            set => cfgModEnabled.Value = value;
        }
        internal bool Traps
        {
            get
            {
                if (cfgSTEnabled.Value) return true;
                return false;
            }
            set => cfgSTEnabled.Value = value;
        }
        internal STFixModBase.EnumOptions Types
        {
            get
            {
                if (cfgTypes == null) return (STFixModBase.EnumOptions)cfgTypes.DefaultValue;
                return cfgTypes.Value;
            }
            set => cfgTypes.Value = value;
        }
        internal bool Clamp
        {
            get
            {
                if (cfgClamp.Value) return true;
                return false;
            }
            set => cfgClamp.Value = value;
        }
        internal float Minimum
        {
            get
            {
                if (cfgMin.Value > 0.7f) return cfgMin.Value;
                return (float)cfgMin.DefaultValue;
            }
            set => cfgMin.Value = value;
        }
        internal float Maximum
        {
            get
            {
                if (cfgMax.Value > cfgMin.Value) return cfgMax.Value;
                return (float)cfgMax.DefaultValue;
            }
            set => cfgMax.Value = value;
        }
        internal bool Scans
        {
            get
            {
                if (cfgScan.Value) return true;
                return false;
            }
            set => cfgScan.Value = value;
        }
        internal bool Move
        {
            get
            {
                if (cfgMove.Value) return true;
                return false;
            }
            set => cfgMove.Value = value;
        }
        internal int ScanRange
        {
            get
            {
                if (cfgRange.Value > 1) return cfgRange.Value;
                return (int)cfgRange.DefaultValue;
            }
            set => cfgRange.Value = value;
        }

        public ConfigControl(ConfigFile cfg)
        {
            InitInstance(this);
            cfgModEnabled = cfg.Bind("Enabled", "Mod Enabled", true, "Turn the mod on.");
            cfgSTEnabled = cfg.Bind("Enabled", "Traps Enabled", true, "When disabled turns the spike traps into harmless decor.");
            cfgTypes = cfg.Bind("Enabled", "Trap types", STFixModBase.EnumOptions.Both, "What types of traps should spawn.");
            cfgClamp = cfg.Bind("Clamp", "Enabled", true, "Enable the slam rate limits listed below. \n\nWhen disabled the spike traps will use the values they spawned with.");
            cfgMin = cfg.Bind("Clamp", "Minimum", 1.5f, "The fastest slam interval. \n\nBase game's quickest possible is 0.71 sec.");
            cfgMax = cfg.Bind("Clamp", "Maximum", 25f, "The slowest slam interval. \n\nBase game's slowest possible is 26.15 sec.");
            cfgScan = cfg.Bind("Scan Node", "Enabled", true, "Add a scanable node to the spike traps, similar to landmines and turrets. \n\nBase game does not have a scan node.");
            cfgMove = cfg.Bind("Scan Node", "Move with spikes", true, "The scan node will move with the spikes. Otherwise it will remain in a fixed position.");
            cfgRange = cfg.Bind("Scan Node", "Range", 9, "The max distance the scan node can be scanned.");
        }
    }
}