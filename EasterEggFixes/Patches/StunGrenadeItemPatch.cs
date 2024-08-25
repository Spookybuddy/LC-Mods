using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace EggFixes.Patches
{
    [HarmonyPatch(typeof(StunGrenadeItem))]
    internal class StunGrenadeItemPatch
    {
        //Set the explode chance to -1 to prevent random explosions
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartFix(StunGrenadeItem __instance)
        {
            if (__instance.chanceToExplode == 16) {
                __instance.chanceToExplode = -1;
                EasterEggFixesModBase.mls.LogInfo($"Fixed an Egg");
            }
        }

        //Set the explode chance to -2 to indicate item has been thrown
        [HarmonyPatch("ItemActivate")]
        [HarmonyPostfix]
        static void ThrowFix(StunGrenadeItem __instance)
        {
            if (__instance.chanceToExplode == -1) {
                __instance.chanceToExplode = -2;
                EasterEggFixesModBase.mls.LogWarning($"Threw Egg!");
            }
        }

        //On ground hit check for -2 to explode, otherwise revert to -1
        [HarmonyPatch("OnHitGround")]
        [HarmonyPostfix]
        static void ExplodeFix(StunGrenadeItem __instance, ref bool ___hasCollided, ref bool ___gotExplodeOnThrowRPC, ref PlayerControllerB ___playerThrownBy)
        {
			if (__instance.hasExploded) return;
            if (__instance.chanceToExplode < -1) {
                if (___hasCollided || __instance.hasHitGround) {
                    EasterEggFixesModBase.mls.LogInfo($"BOOM!");
                    ___gotExplodeOnThrowRPC = true;
                    __instance.hasExploded = true;
                    if (__instance.spawnDamagingShockwave) Landmine.SpawnExplosion(__instance.transform.position + Vector3.up * 0.2f, spawnExplosionEffect: false, 0.5f, 3f, 40, 45f);
                    Transform parent = ((!__instance.isInElevator) ? RoundManager.Instance.mapPropsContainer.transform : StartOfRound.Instance.elevatorTransform);
                    Object.Instantiate(__instance.stunGrenadeExplosion, __instance.transform.position, Quaternion.identity, parent);
                    __instance.itemAudio.PlayOneShot(__instance.explodeSFX);
                    WalkieTalkie.TransmitOneShotAudio(__instance.itemAudio, __instance.explodeSFX);
                    if (__instance.DestroyGrenade) __instance.DestroyObjectInHand(___playerThrownBy); //Object.Destroy(__instance.gameObject);
                } else {
                    EasterEggFixesModBase.mls.LogError($"Failed to impact. Resetting Egg.");
                    __instance.chanceToExplode = -1;
                }
            }
        }

        //Pickup resets to -1
        [HarmonyPatch("EquipItem")]
        [HarmonyPostfix]
        static void EquipFix(StunGrenadeItem __instance)
        {
            if (__instance.chanceToExplode < 100) {
                __instance.chanceToExplode = -1;
                EasterEggFixesModBase.mls.LogInfo($"Picked up an Egg");
            }
        }
    }
}