using BepInEx.Configuration;
using UnityEngine;

namespace MaskedRagdoll
{
    class Config : SyncedInstance<Config>
    {
        public ConfigEntry<float> cfgMultiplier;
        public ConfigEntry<bool> cfgMasks;
        public ConfigEntry<bool> cfgCollide;
        public ConfigEntry<bool> cfgBaboon;
        public ConfigEntry<bool> cfgBarber;
        public ConfigEntry<bool> cfgBees;
        public ConfigEntry<bool> cfgBracken;
        public ConfigEntry<bool> cfgCoilhead;
        public ConfigEntry<bool> cfgEarthLeviathan;
        public ConfigEntry<bool> cfgEyelessDog;
        public ConfigEntry<bool> cfgGhost;
        public ConfigEntry<bool> cfgGiant;
        public ConfigEntry<bool> cfgGiantDie;
        public ConfigEntry<bool> cfgHoardingBug;
        public ConfigEntry<bool> cfgHornets;
        public ConfigEntry<bool> cfgJester;
        public ConfigEntry<bool> cfgKidnapperFox;
        public ConfigEntry<bool> cfgManeater;
        public ConfigEntry<bool> cfgNutcracker;
        public ConfigEntry<bool> cfgSlime;
        public ConfigEntry<bool> cfgSpider;
        public ConfigEntry<bool> cfgSpore;
        public ConfigEntry<bool> cfgThumper;
        public ConfigEntry<bool> cfgTurret;

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
        internal bool BaboonHawk
        {
            get => cfgBaboon.Value;
            set => cfgBaboon.Value = value;
        }
        internal bool Barber
        {
            get => cfgBarber.Value;
            set => cfgBarber.Value = value;
        }
        internal bool Bees
        {
            get => cfgBees.Value;
            set => cfgBees.Value = value;
        }
        internal bool Bracken
        {
            get => cfgBracken.Value;
            set => cfgBracken.Value = value;
        }
        internal bool Coilhead
        {
            get => cfgCoilhead.Value;
            set => cfgCoilhead.Value = value;
        }
        internal bool EarthLeviathan
        {
            get => cfgEarthLeviathan.Value;
            set => cfgEarthLeviathan.Value = value;
        }
        internal bool EyelessDog
        {
            get => cfgEyelessDog.Value;
            set => cfgEyelessDog.Value = value;
        }
        internal bool GhostGirl
        {
            get => cfgGhost.Value;
            set => cfgGhost.Value = value;
        }
        internal bool ForestGiant
        {
            get => cfgGiant.Value;
            set => cfgGiant.Value = value;
        }
        internal bool GiantFall
        {
            get => cfgGiantDie.Value;
            set => cfgGiantDie.Value = value;
        }
        internal bool Lootbug
        {
            get => cfgHoardingBug.Value;
            set => cfgHoardingBug.Value = value;
        }
        internal bool Hornets
        {
            get => cfgHornets.Value;
            set => cfgHornets.Value = value;
        }
        internal bool Jester
        {
            get => cfgJester.Value;
            set => cfgJester.Value = value;
        }
        internal bool KidnapperFox
        {
            get => cfgKidnapperFox.Value;
            set => cfgKidnapperFox.Value = value;
        }
        internal bool Maneater
        {
            get => cfgManeater.Value;
            set => cfgManeater.Value = value;
        }
        internal bool Nutcracker
        {
            get => cfgNutcracker.Value;
            set => cfgNutcracker.Value = value;
        }
        internal bool Slime
        {
            get => cfgSlime.Value;
            set => cfgSlime.Value = value;
        }
        internal bool Spider
        {
            get => cfgSpider.Value;
            set => cfgSpider.Value = value;
        }
        internal bool SporeLizard
        {
            get => cfgSpore.Value;
            set => cfgSpore.Value = value;
        }
        internal bool Thumper
        {
            get => cfgThumper.Value;
            set => cfgThumper.Value = value;
        }
        internal bool Turret
        {
            get => cfgTurret.Value;
            set => cfgTurret.Value = value;
        }

        public Config(ConfigFile cfg)
        {
            InitInstance(this);
            cfgMasks = cfg.Bind("Masked Ragdolls", "Masks Enabled", true, "Masks will appear on the ragdolls.");
            cfgMultiplier = cfg.Bind("Masked Ragdolls", "Multiplier", 1.0f, "Multiplies the force applied to the masked ragdolls upon death. \n\nClamped to +/- 10");

            //Enemy toggles
            cfgCollide = cfg.Bind("Collide With Enemy", "All Enemy Collision", true, "Masked will take damage from other enemies.");
            cfgBaboon = cfg.Bind("Collide With Enemy", "Baboon Hawk", true, "Masked will take damage from Baboon Hawks.");
            cfgBarber = cfg.Bind("Collide With Enemy", "Barber", true, "Masked will be cut by Barbers.");
            cfgBracken = cfg.Bind("Collide With Enemy", "Bracken", true, "Masked will take damage from Brackens.");
            cfgBees = cfg.Bind("Collide With Enemy", "Circuit Bees", true, "Masked will take damage from Circuit Bees.");
            cfgCoilhead = cfg.Bind("Collide With Enemy", "Coilhead", true, "Masked will take damage from Coilheads when they move.");
            cfgEarthLeviathan = cfg.Bind("Collide With Enemy", "Earth Leviathan", true, "Masked be launched when hit by Earth Leviathans.");
            cfgEyelessDog = cfg.Bind("Collide With Enemy", "Eyeless Dog", true, "Masked will take damage from Eyeless Dogs.");
            cfgGiant = cfg.Bind("Collide With Enemy", "Forest Giant", true, "Masked will be eaten by Forest Giants.");
            cfgGiantDie = cfg.Bind("Collide With Enemy", "Forest Giant Falling", true, "Masked will die from the Forest Giant when it falls over.");
            cfgGhost = cfg.Bind("Collide With Enemy", "Ghost Girl", true, "Masked will take damage from Ghost Girl when she chases.");
            cfgHoardingBug = cfg.Bind("Collide With Enemy", "Hoarding Bug", true, "Masked will take damage from Hoarding Bug when they attack.");
            cfgHornets = cfg.Bind("Collide With Enemy", "Hornets", true, "Masked will take damage from Hornets.");
            cfgJester = cfg.Bind("Collide With Enemy", "Jester", true, "Masked will take damage from Jesters when popped.");
            cfgKidnapperFox = cfg.Bind("Collide With Enemy", "Kidnapper Fox", true, "Masked will take damage from the Kidnapper Fox.");
            cfgManeater = cfg.Bind("Collide With Enemy", "Maneater", true, "Masked will take damage from Maneaters when they lunge.");
            cfgNutcracker = cfg.Bind("Collide With Enemy", "Nutcracker", true, "Masked will die when kicked by Nutcrackers.");
            cfgSlime = cfg.Bind("Collide With Enemy", "Slime", true, "Masked will take damage from Slimes.");
            cfgSpider = cfg.Bind("Collide With Enemy", "Spider", true, "Masked will take damage from Spiders.");
            cfgSpore = cfg.Bind("Collide With Enemy", "Spore Lizard", true, "Masked will take damage from Spore Lizards.");
            cfgThumper = cfg.Bind("Collide With Enemy", "Thumper", true, "Masked will take damage from Thumpers.");
            cfgTurret = cfg.Bind("Collide With Enemy", "Turrets", true, "Masked will take damage from Turret's gunshots.");
        }
    }
}