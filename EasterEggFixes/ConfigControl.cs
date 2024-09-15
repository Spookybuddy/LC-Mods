using BepInEx.Configuration;

namespace EggFixes
{
    class ConfigControl : SyncedInstance<ConfigControl>
    {
        public ConfigEntry<bool> cfgEnabled;
        public ConfigEntry<EasterEggFixesModBase.EggSettings> cfgExplodeWhen;
        public ConfigEntry<int> cfgChance;

        internal bool Enabled
        {
            get
            {
                if (cfgEnabled != null) return cfgEnabled.Value;
                return true;
            }
            set => cfgEnabled.Value = value;
        }

        internal EasterEggFixesModBase.EggSettings ExplodeWhen
        {
            get
            {
                if (cfgExplodeWhen == null) return (EasterEggFixesModBase.EggSettings)cfgExplodeWhen.DefaultValue;
                return cfgExplodeWhen.Value;
            }
            set => cfgExplodeWhen.Value = value;
        }

        internal int Chance
        {
            get
            {
                if (cfgChance.Value > 0 && cfgChance.Value < 100) return cfgChance.Value;
                return (int)cfgChance.DefaultValue;
            }
            set => cfgChance.Value = value;
        }

        public ConfigControl(ConfigFile cfg)
        {
            InitInstance(this);
            cfgEnabled = cfg.Bind("Eggs", "Enabled", true, "Turn the mod on.");
            cfgExplodeWhen = cfg.Bind("Eggs", "Explode When", EasterEggFixesModBase.EggSettings.ExplodeOnThrow, "When should the eggs explode?");
            cfgChance = cfg.Bind("Eggs", "Chance to Explode", 16, "The percent chance for the eggs to explode when set to 'ChanceToExplode'. \nRanges from 1 - 99");
        }
    }
}
