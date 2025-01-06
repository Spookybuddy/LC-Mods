using HarmonyLib;
using UnityEngine;

namespace MaskedRagdoll.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    class LandminePatch
    {
        [HarmonyPatch("SpawnExplosion")]
        [HarmonyPrefix]
        static void FindMine(Vector3 explosionPosition, bool spawnExplosionEffect = false, float killRange = 1f, float damageRange = 1f, int nonLethalDamage = 50, float physicsForce = 0f, GameObject overridePrefab = null, bool goThroughCar = false)
        {
            //Track the last explodion position to use as detection when enemy is hit
            for (int i = RagdollModBase.Instance.lastExplodePos.Length - 1; i > 0; i--) RagdollModBase.Instance.lastExplodePos[i] = RagdollModBase.Instance.lastExplodePos[i - 1];
            RagdollModBase.Instance.lastExplodePos[0] = explosionPosition;
        }
    }
}