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
                                if (!Config.Instance.BaboonHawk) return true;
                                script.HitEnemyServerRpc(1, -1, true, -200);
                                return false;

                            case "Clay Surgeon":
                                if (!Config.Instance.Barber) return true;
                                script.HitEnemyServerRpc(4, -1, false, -201);
                                return false;

                            case "Butler Bees":
                                if (!Config.Instance.Hornets) return true;
                                script.HitEnemyServerRpc(1, -1, true, -202);
                                return false;
                            case "Red Locust Bees":
                                if (!Config.Instance.Bees) return true;
                                script.HitEnemyServerRpc(1, -1, true, -202);
                                return false;

                            case "Blob":
                                if (!Config.Instance.Slime) return true;
                                script.HitEnemyServerRpc(1, -1, false, -203);
                                return false;

                            case "Flowerman":
                                if (!Config.Instance.Bracken) return true;
                                //Break neck only when angry
                                if (component.mainScript.currentBehaviourStateIndex == 2) script.HitEnemyServerRpc(4, -1, false, -204);
                                return false;

                            case "Spring":
                                if (!Config.Instance.Coilhead) return true;
                                //Damage only when moving
                                if (component.mainScript.agent.speed > 0 && component.mainScript.currentBehaviourStateIndex == 1) script.HitEnemyServerRpc(3, -1, false, -205);
                                return false;

                            case "MouthDog":
                                if (!Config.Instance.EyelessDog) return true;
                                script.HitEnemyServerRpc(4, -1, false, -206);
                                return false;
                            case "Bush Wolf":
                                if (!Config.Instance.KidnapperFox) return true;
                                script.HitEnemyServerRpc(1, -1, true, -207);
                                return false;
                            case "Girl":
                                if (!Config.Instance.GhostGirl) return true;
                                //Kill only when chasing
                                if (component.mainScript.currentBehaviourStateIndex == 1) script.HitEnemyServerRpc(4, -1, false, -208);
                                return false;

                            case "ForestGiant":
                                if (!Config.Instance.ForestGiant) return true;
                                //Not stunned & not burning
                                if (component.mainScript.stunNormalizedTimer > 0 || component.mainScript.currentBehaviourStateIndex == 2) return false;
                                script.HitEnemyServerRpc(4, -1, false, -209);
                                return false;

                            case "Jester":
                                if (!Config.Instance.Jester) return true;
                                //Kill only when opened
                                if (component.mainScript.currentBehaviourStateIndex == 2) script.HitEnemyServerRpc(4, -1, false, -210);
                                return false;

                            case "Maneater":
                                if (!Config.Instance.Maneater) return true;
                                //Only kill when adult & leaping
                                if (component.mainScript.agent.speed > 0 || component.mainScript.currentBehaviourStateIndex == 3) script.HitEnemyServerRpc(4, -1, false, -211);
                                return false;

                            case "Nutcracker":
                                if (!Config.Instance.Nutcracker) return true;
                                //Only kick when walking
                                NutcrackerEnemyAI nut = component.mainScript as NutcrackerEnemyAI;
                                if (!nut.patrol.inProgress) return false;
                                script.HitEnemyServerRpc(4, -1, false, -212);
                                return false;

                            case "Puffer":
                                if (!Config.Instance.SporeLizard) return true;
                                script.HitEnemyServerRpc(1, -1, false, -213);
                                return false;

                            case "Bunker Spider":
                                if (!Config.Instance.Spider) return true;
                                script.HitEnemyServerRpc(2, -1, true, -214);
                                return false;

                            case "Crawler":
                                if (!Config.Instance.Thumper) return true;
                                script.HitEnemyServerRpc(1, -1, false, -215);
                                return false;

                            case "Earth Leviathan":
                                if (!Config.Instance.EarthLeviathan) return true;
                                script.HitEnemyServerRpc(4, -1, false, -216);
                                return false;

                            case "Masked":
                                return false;

                            case "Hoarding bug":
                                if (!Config.Instance.Lootbug) return true;
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