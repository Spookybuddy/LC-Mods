using HarmonyLib;
using UnityEngine;

namespace MaskedRagdoll.Patches
{
    [HarmonyPatch(typeof(VehicleController))]
    class VehiclePatch
    {
        [HarmonyPatch("CarReactToObstacle")]
        [HarmonyPrefix]
        static bool HitMasked(ref VehicleController __instance, Vector3 vel, Vector3 position, Vector3 impulse, CarObstacleType type, float obstacleSize = 1f, EnemyAI enemyScript = null)
        {
            if (type.Equals(CarObstacleType.Enemy) && enemyScript != null && enemyScript.enemyType.enemyName.Equals("Masked")) {
				vel = Vector3.Scale(vel, new Vector3(1f, 0f, 1f));
				__instance.mainRigidbody.AddForceAtPosition(Vector3.up * __instance.torqueForce, position, ForceMode.VelocityChange);
				if (vel.magnitude > 1) {
                    int damage = (int)(vel.magnitude * 1.05f);
                    enemyScript.HitEnemyOnLocalClient(damage, playerWhoHit: __instance.currentDriver, hitID: -220);
                    __instance.PlayCollisionAudio(position, 5, Mathf.Clamp01(vel.magnitude / 4));
                }
				return false;
            }
            return true;
        }
    }
}