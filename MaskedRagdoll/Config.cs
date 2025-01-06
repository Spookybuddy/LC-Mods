using BepInEx.Configuration;
using UnityEngine;

namespace MaskedRagdoll
{
    class Config : SyncedInstance<Config>
    {
        public ConfigEntry<float> cfgMultiplier;
        public ConfigEntry<bool> cfgMasks;
        public ConfigEntry<bool> cfgCollide;
        public ConfigEntry<bool> cfgMisc;
        internal float Multiplier
        {
            get {
                if (cfgMultiplier != null) return Mathf.Clamp((float)cfgMultiplier.Value, -10, 10);
                else return (float)cfgMultiplier.DefaultValue;
            }
            set => cfgMultiplier.Value = Mathf.Clamp(value, -10, 10);
        }
        internal bool Masked
        {
            get => cfgMasks.Value;
            set => cfgMasks.Value = value;
        }
        internal bool EnemyCollision
        {
            get => cfgCollide.Value;
            set => cfgCollide.Value = value;
        }
        internal bool MiscKilling
        {
            get => cfgMisc.Value;
            set => cfgMisc.Value = value;
        }

        public Config(ConfigFile cfg)
        {
            InitInstance(this);
            cfgMasks = cfg.Bind("Masked Ragdolls", "Masks Enabled", true, "Masks will appear on the ragdolls.");
            cfgCollide = cfg.Bind("Masked Ragdolls", "Enemy Collision", true, "Masked will take damage from other enemies.");
            cfgMisc = cfg.Bind("Masked Ragdolls", "Miscellaneous Damage", true, "Masked will take damage from Turrets & Giants falling over.");
            cfgMultiplier = cfg.Bind("Masked Ragdolls", "Multiplier", 1.0f, "Multiplies the force applied to the masked ragdolls upon death. \n\nClamped to +/- 10");
        }
    }
}