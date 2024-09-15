using HarmonyLib;

namespace EggFixes.Patches
{
    [HarmonyPatch(typeof(StunGrenadeItem))]
    internal class StunGrenadeItemPatch
    {
        //Set the explode chance to -1 to prevent random explosions at start
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void StartFix(StunGrenadeItem __instance)
        {
            if (!ConfigControl.Instance.Enabled) return;
            if (__instance.chanceToExplode != 100) {
                switch (ConfigControl.Instance.ExplodeWhen) {
                    case EasterEggFixesModBase.EggSettings.AlwaysExplode:
                        __instance.chanceToExplode = 111;
                        EasterEggFixesModBase.mls.LogInfo($"Eggs Always Explode");
                        break;
                    case EasterEggFixesModBase.EggSettings.ChanceToExplode:
                        __instance.chanceToExplode = ConfigControl.Instance.Chance;
                        EasterEggFixesModBase.mls.LogInfo($"Eggs Sometimes Explode");
                        break;
                    default:
                        __instance.chanceToExplode = -1;
                        EasterEggFixesModBase.mls.LogInfo($"Fixed an Egg");
                        break;
                }
            }
        }

        //Pickup also resets the egg
        [HarmonyPatch("EquipItem")]
        [HarmonyPostfix]
        static void EquipFix(StunGrenadeItem __instance)
        {
            if (!ConfigControl.Instance.Enabled) return;
            if (__instance.chanceToExplode != 100) {
                switch (ConfigControl.Instance.ExplodeWhen) {
                    case EasterEggFixesModBase.EggSettings.AlwaysExplode:
                        __instance.chanceToExplode = 111;
                        EasterEggFixesModBase.mls.LogInfo($"Egg will Always Explode");
                        break;
                    case EasterEggFixesModBase.EggSettings.ChanceToExplode:
                        __instance.chanceToExplode = ConfigControl.Instance.Chance;
                        EasterEggFixesModBase.mls.LogInfo($"Egg will explode " + ConfigControl.Instance.Chance + "%");
                        break;
                    default:
                        __instance.chanceToExplode = -1;
                        EasterEggFixesModBase.mls.LogInfo($"Picked up an Egg");
                        break;
                }
                __instance.SetExplodeOnThrowServerRpc();
            }
        }
    }
}