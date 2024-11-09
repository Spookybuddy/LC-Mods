using HarmonyLib;
using UnityEngine;

namespace MaskedRagdoll.Patches
{
    [HarmonyPatch(typeof(SpikeRoofTrap))]
    class SpikeTrapPatch
    {
        [HarmonyPatch("OnTriggerStay")]
        [HarmonyPrefix]
        static bool Ragdoll(Collider other, SpikeRoofTrap __instance)
        {
            if (other.gameObject.CompareTag("Enemy")) {
                if (other.gameObject.TryGetComponent<EnemyAICollisionDetect>(out EnemyAICollisionDetect enemy)) {
                    if (enemy.mainScript.enemyType.enemyName.Equals("Masked") && !enemy.mainScript.isEnemyDead) {
                        enemy.mainScript.HitEnemyOnLocalClient(4, hitID: -217);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}