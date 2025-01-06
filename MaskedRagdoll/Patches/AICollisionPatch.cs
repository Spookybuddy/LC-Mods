using HarmonyLib;
using UnityEngine;

namespace MaskedRagdoll.Patches
{
    [HarmonyPatch(typeof(EnemyAICollisionDetect))]
    internal class AICollisionPatch
    {
        [HarmonyPatch("OnTriggerStay")]
        [HarmonyPrefix]
        static bool MaskedDetect(Collider other, EnemyAICollisionDetect __instance)
        {
            if (!other.CompareTag("Enemy") && !other.CompareTag("Untagged")) return true;

            if (!Config.Instance.EnemyCollision) return true;

            //Masked hit another enemy
            if (other.gameObject.TryGetComponent<EnemyAICollisionDetect>(out EnemyAICollisionDetect component)) {
                //Dead or both same creature
                if (__instance.mainScript.isEnemyDead || component.mainScript.isEnemyDead || __instance.mainScript.enemyType.enemyName.Equals(component.mainScript.enemyType.enemyName)) return false;

                //Check masked hitting another enemy
                if (component.mainScript != __instance.mainScript) {
                    if (__instance.mainScript.enemyType.enemyName.Equals("Masked")) {
                        //Use stun timer as i-frames
                        if (__instance.mainScript.stunNormalizedTimer > 0) return false;

                        //Only owner will pass enemy collisions
                        if (!__instance.mainScript.IsOwner) return false;

                        MaskedPlayerEnemy script = __instance.mainScript as MaskedPlayerEnemy;
                        //Check what entity the masked hit
                        switch (component.mainScript.enemyType.enemyName) {
                            case "Baboon hawk":
                                script.HitEnemyServerRpc(1, -1, true, -200);
                                return false;

                            case "Clay Surgeon":
                                script.HitEnemyServerRpc(4, -1, false, -201);
                                return false;

                            case "Butler Bees":
                            case "Red Locust Bees":
                                script.HitEnemyServerRpc(1, -1, true, -202);
                                return false;

                            case "Blob":
                                script.HitEnemyServerRpc(1, -1, false, -203);
                                return false;

                            case "Flowerman":
                                //Break neck only when angry
                                if (component.mainScript.currentBehaviourStateIndex == 2) script.HitEnemyServerRpc(4, -1, false, -204);
                                return false;

                            case "Spring":
                                //Damage only when moving
                                if (component.mainScript.agent.speed > 0 && component.mainScript.currentBehaviourStateIndex == 1) script.HitEnemyServerRpc(3, -1, false, -205);
                                return false;

                            case "MouthDog":
                                script.HitEnemyServerRpc(4, -1, false, -206);
                                return false;

                            case "Bush Wolf":
                                script.HitEnemyServerRpc(1, -1, true, -207);
                                return false;

                            case "Girl":
                                //Kill only when chasing
                                if (component.mainScript.currentBehaviourStateIndex == 1) script.HitEnemyServerRpc(4, -1, false, -208);
                                return false;

                            case "ForestGiant":
                                //Not stunned & not burning
                                if (component.mainScript.stunNormalizedTimer > 0 || component.mainScript.currentBehaviourStateIndex == 2) return false;
                                script.HitEnemyServerRpc(4, -1, false, -209);
                                return false;

                            case "Jester":
                                //Kill only when opened
                                if (component.mainScript.currentBehaviourStateIndex == 2) script.HitEnemyServerRpc(4, -1, false, -210);
                                return false;

                            case "Maneater":
                                //Only kill when adult & leaping
                                if (component.mainScript.agent.speed > 0 || component.mainScript.currentBehaviourStateIndex == 3) script.HitEnemyServerRpc(4, -1, false, -211);
                                return false;

                            case "Nutcracker":
                                //Only kick when walking
                                NutcrackerEnemyAI nut = component.mainScript as NutcrackerEnemyAI;
                                if (!nut.patrol.inProgress) return false;
                                script.HitEnemyServerRpc(4, -1, false, -212);
                                return false;

                            case "Puffer":
                                script.HitEnemyServerRpc(1, -1, false, -213);
                                return false;

                            case "Bunker Spider":
                                script.HitEnemyServerRpc(2, -1, true, -214);
                                return false;

                            case "Crawler":
                                script.HitEnemyServerRpc(1, -1, false, -215);
                                return false;

                            case "Earth Leviathan":
                                script.HitEnemyServerRpc(4, -1, false, -216);
                                return false;

                            case "Masked":
                                return false;

                            case "Hoarding bug":
                                if (component.mainScript.currentBehaviourStateIndex == 2) script.HitEnemyServerRpc(1, -1, false, -221);
                                return false;

                            case "RadMech":
                                //Torch masked?
                                //script.HitEnemyOnLocalClient(4, hitID: -222);
                                return false;

                            default:
                                return true;
                        }
                    }
                }
            } 
            return true;
        }
    }
}