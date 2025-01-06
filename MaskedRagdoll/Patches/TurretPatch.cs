using HarmonyLib;
using UnityEngine;

namespace MaskedRagdoll.Patches
{
    [HarmonyPatch(typeof(Turret))]
    class TurretPatch
    {
        //Raycast out to hit enemies, dealing damage if Masked is hit
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void Gunfire(ref Turret __instance, ref float ___turretInterval)
        {
            if (!Config.Instance.MiscKilling) return;
            if (__instance.turretMode.Equals(TurretMode.Firing) || __instance.turretMode.Equals(TurretMode.Berserk)) {
                if (___turretInterval >= 0.21f) {
                    Ray lineOfFire = new Ray(__instance.aimPoint.position, __instance.aimPoint.forward);
                    if (Physics.Raycast(lineOfFire, out RaycastHit hit, 30f, 524288)) {
                        MaskedPlayerEnemy masked = null;
                        if (hit.transform.TryGetComponent<MaskedPlayerEnemy>(out MaskedPlayerEnemy mask)) masked = mask;
                        if (hit.transform.TryGetComponent<EnemyAICollisionDetect>(out EnemyAICollisionDetect collider)) masked = collider.mainScript as MaskedPlayerEnemy;
                        if (masked != null) {
                            if (masked.enemyType.enemyName.Equals("Masked") && !masked.isEnemyDead) masked.HitEnemyOnLocalClient(2, hitID: -218);
                        }
                    }
                }
            }
        }
    }
}