using HarmonyLib;

namespace MaskedRagdoll.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatch
    {
        //Hide the bodies
        [HarmonyPatch("EnableEnemyMesh")]
        [HarmonyPostfix]
        static void HideDeadBodies(EnemyAI __instance)
        {
            if (__instance.isEnemyDead && __instance.enemyType.enemyName.Equals("Masked")) {
                for (int i = 0; i < __instance.skinnedMeshRenderers.Length; i++) __instance.skinnedMeshRenderers[i].gameObject.SetActive(false);
                for (int j = 0; j < __instance.meshRenderers.Length; j++) __instance.meshRenderers[j].gameObject.SetActive(false);
            }
        }
    }
}