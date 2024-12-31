using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;

namespace MaskedRagdoll.Patches
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPatch
    {
        internal static Vector3 recoil;
        internal static int ragdollID;
        internal static Transform stickPoint;
        internal static GameObject outfit;

        static readonly Vector3 skip = new Vector3(999, 999, 999);

        //Calculate velocity if killed
        [HarmonyPatch("HitEnemy")]
        [HarmonyPrefix]
        static void GetHitVelocity(MaskedPlayerEnemy __instance, int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
        {
            //Special hit sfx for puffer & Thumper lol
            if (hitID == -213 || hitID == -215) PlaySound(__instance, 2, true);

            //On killing blow
            if (force >= __instance.enemyHP && !__instance.isEnemyDead) {
                //Mimicking outfit
                outfit = Mimicking(__instance);

                //Resets
                ragdollID = 0;
                stickPoint = null;
                recoil = Vector3.zero;
                EnemyAI component = null;

                //Sync for best results
                if (__instance.IsOwner) __instance.SyncPositionToClients();

                //Use specific hit IDs to indicate recoil & SFX
                switch (hitID) {
                    case 1:
                        //Shovel
                        RagdollModBase.mls.LogInfo($"Masked bonked!");
                        if (__instance.IsOwner) playerWhoHit.SyncBodyPositionWithClients();
                        recoil = ((__instance.serverPosition - playerWhoHit.serverPlayerPosition).normalized + Vector3.up) * 1.6f;
                        break;
                    case 5:
                        //Knife
                        RagdollModBase.mls.LogInfo($"Masked stabbed!");
                        if (__instance.IsOwner) playerWhoHit.SyncBodyPositionWithClients();
                        recoil = ((__instance.serverPosition - playerWhoHit.serverPlayerPosition).normalized * 0.5f) + Vector3.up;
                        break;
                    case -200:
                        //Baboon
                        RagdollModBase.mls.LogInfo($"Masked pierced by Baboon Hawk!");
                        component = FindNearbyEnemies(__instance, "Baboon hawk");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = ((__instance.serverPosition - component.serverPosition).normalized + new Vector3(0, 5, 0)) * 1.5f;
                        }
                        PlaySound(__instance, 0, true);
                        break;
                    case -201:
                        //Barber
                        RagdollModBase.mls.LogInfo($"Masked got a haircut!");
                        component = FindNearbyEnemies(__instance, "Clay Surgeon");
                        if (component != null) component.creatureAnimator.SetTrigger("snip");
                        recoil = new Vector3(0, 9, 0);
                        PlaySound(__instance, 1);
                        ragdollID = 1;
                        break;
                    case -202:
                        //Bees/Hornets
                        RagdollModBase.mls.LogInfo($"Masked is allergic!");
                        component = FindNearbyEnemies(__instance, "Butler Bees");
                        if (component == null) component = FindNearbyEnemies(__instance, "Red Locust Bees");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = (__instance.serverPosition - component.serverPosition).normalized + new Vector3(0, 2.5f, 0);
                        }
                        break;
                    case -203:
                        //Blob
                        RagdollModBase.mls.LogInfo($"Masked digested by Blob!");
                        recoil = Vector3.up;
                        PlaySound(__instance, 3);
                        ragdollID = 4;
                        break;
                    case -204:
                        //Bracken
                        RagdollModBase.mls.LogInfo($"Masked neck was broken!");
                        recoil = -(__instance.transform.forward + Vector3.up);
                        PlaySound(__instance, 4);
                        break;
                    case -205:
                        //Coilhead
                        RagdollModBase.mls.LogInfo($"Masked left this mortal coil!");
                        component = FindNearbyEnemies(__instance, "Spring");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = (__instance.serverPosition - component.serverPosition + Vector3.up) * (component.agent.speed + 1);
                        }
                        ragdollID = 3;
                        break;
                    case -206:
                        //Dog
                        RagdollModBase.mls.LogInfo($"Masked eaten by Dog!");
                        component = FindNearbyEnemies(__instance, "MouthDog");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = ((__instance.serverPosition - component.serverPosition).normalized + Vector3.up) * 7 + Vector3.up;
                        }
                        PlaySound(__instance, 5, true);
                        break;
                    case -207:
                        //Fox
                        RagdollModBase.mls.LogInfo($"Masked was outfoxed!");
                        recoil = Vector3.up * 5.5f;
                        PlaySound(__instance, 6, true);
                        ragdollID = 2;
                        break;
                    case -208:
                        //Ghost
                        RagdollModBase.mls.LogInfo($"Masked was haunted!");
                        recoil = Vector3.up;
                        ragdollID = 2;
                        break;
                    case -209:
                        //Giant
                        RagdollModBase.mls.LogInfo($"Masked grabbed by Giant!");
                        ForestGiantAI giant = FindNearbyEnemies(__instance, "ForestGiant", 5f) as ForestGiantAI;
                        if (giant.IsOwner) giant.GrabPlayerServerRpc(-5);
                        recoil = skip;
                        break;
                    case -210:
                        //Jester
                        RagdollModBase.mls.LogInfo($"Masked mauled by Jester!");
                        component = FindNearbyEnemies(__instance, "Jester");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = (__instance.serverPosition - component.serverPosition) * Mathf.Max(component.agent.speed, 1.5f);
                        }
                        PlaySound(__instance, 8);
                        break;
                    case -211:
                        //Maneater
                        RagdollModBase.mls.LogInfo($"Masked Maneaten!");
                        component = FindNearbyEnemies(__instance, "Maneater");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = ((__instance.serverPosition - component.serverPosition).normalized + Vector3.down) * Mathf.Max(component.agent.speed, 1.3f);
                        }
                        break;
                    case -212:
                        //Nut
                        RagdollModBase.mls.LogInfo($"Masked punted by Nutcracker!");
                        component = FindNearbyEnemies(__instance, "Nutcracker");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = (__instance.serverPosition - component.serverPosition + Vector3.up).normalized * 23 + Vector3.up;
                        }
                        PlaySound(__instance, 9);
                        break;
                    case -213:
                        //Puffer
                        RagdollModBase.mls.LogInfo($"Masked chewed on by Spore Lizard!");
                        component = FindNearbyEnemies(__instance, "Puffer");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = __instance.serverPosition - component.serverPosition + Vector3.up;
                        }
                        break;
                    case -214:
                        //Spider
                        RagdollModBase.mls.LogInfo($"Masked eaten by Spider!");
                        component = FindNearbyEnemies(__instance, "Bunker Spider");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = (__instance.serverPosition - component.serverPosition + Vector3.up) * 5;
                        }
                        PlaySound(__instance, 11, true);
                        break;
                    case -215:
                        //Thumper
                        RagdollModBase.mls.LogInfo($"Masked mauled by Thumper!");
                        component = FindNearbyEnemies(__instance, "Crawler");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = (__instance.serverPosition - component.serverPosition) * Mathf.Max(component.agent.speed, 1.5f);
                        }
                        break;
                    case -216:
                        //Worm
                        RagdollModBase.mls.LogInfo($"Masked eaten by Earth Leviathan!");
                        recoil = Vector3.up * 111;
                        break;
                    case -217:
                        //Spikes
                        RagdollModBase.mls.LogInfo($"Masked crushed!");
                        SpikeRoofTrap spikeTrap = FindSpikeTrap(__instance);
                        if (spikeTrap != null) {
                            Vector3 point = new Vector3(__instance.serverPosition.x, spikeTrap.stickingPointsContainer.position.y, __instance.serverPosition.z);
                            GameObject stick = GameObject.Instantiate(spikeTrap.deadBodyStickingPointPrefab, point, Quaternion.Euler(__instance.serverRotation), spikeTrap.stickingPointsContainer);
                            stickPoint = stick.transform;
                        }
                        ragdollID = 5;
                        break;
                    case -218:
                        //Turret
                        RagdollModBase.mls.LogInfo($"Masked shot!");
                        recoil = (__instance.serverPosition - FindNearbyTurret(__instance)).normalized * 28 + new Vector3(0, 3.5f, 0);
                        break;
                    case -219:
                        //Giant fall over
                        RagdollModBase.mls.LogInfo($"Giant fell on Masked!");
                        ForestGiantAI giant2 = FindNearbyEnemies(__instance, "ForestGiant", 5f) as ForestGiantAI;
                        if (giant2 != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = Vector3.Scale((__instance.serverPosition - giant2.deathFallPosition.position).normalized, new Vector3(24, 0.2f, 24)) + Vector3.up;
                        }
                        break;
                    case -220:
                        //Car
                        if (playerWhoHit.inVehicleAnimation) {
                            if (playerWhoHit.physicsParent.TryGetComponent<VehicleController>(out VehicleController car)) {
                                RagdollModBase.mls.LogInfo($"Masked ran over!");
                                Vector3 vel = Vector3.Scale(car.averageVelocity, new Vector3(1f, 0f, 1f));
                                recoil = (__instance.serverPosition - car.syncedPosition).normalized * Mathf.Min(Mathf.Pow(vel.magnitude, 2) / 2 + 3, 35) + (Vector3.up * vel.magnitude);
                            }
                        }
                        break;
                    case -221:
                        //Lootbug
                        RagdollModBase.mls.LogInfo($"Masked was looted!");
                        component = FindNearbyEnemies(__instance, "Hoarding bug");
                        if (component != null) {
                            if (component.IsOwner) component.SyncPositionToClients();
                            recoil = (__instance.serverPosition - component.serverPosition).normalized * (component.agent.speed + 0.2f) + Vector3.down;
                        }
                        break;
                    case -222:
                        //Radmech torch
                        //RadMechAI robot = FindNearbyEnemies(__instance, "RadMech") as RadMechAI;
                        //Add patch to hijack grab and torch animation
                        break;
                    default:
                        //Other: Gun, Explosion, etc
                        float expDis = Vector3.Distance(__instance.serverPosition, RagdollModBase.Instance.lastExplodePos);

                        //Use the distance from the hit to determine the force applied
                        if (playerWhoHit != null) {
                            RagdollModBase.mls.LogInfo($"Masked gunned down!");
                            if (__instance.IsOwner) playerWhoHit.SyncBodyPositionWithClients();
                            expDis = Vector3.Distance(__instance.serverPosition, playerWhoHit.transform.position);
                            recoil = (__instance.serverPosition - playerWhoHit.transform.position).normalized * (force * ((expDis + 15) / (expDis + 1))) + (Vector3.up * 2);
                        } else if (expDis < 4.5f) {
                            RagdollModBase.mls.LogInfo($"Masked exploded! @ " + RagdollModBase.Instance.lastExplodePos);
                            recoil = (__instance.serverPosition - RagdollModBase.Instance.lastExplodePos).normalized * (26 / (expDis - 6.5f) + 39) + (Vector3.up * 3);
                        } else {
                            //Catch for unknown recoil
                            RagdollModBase.mls.LogWarning($"Masked killed???");
                            recoil = (-__instance.transform.forward + Vector3.up) * (force * 2);
                        }
                        break;
                }
                RagdollModBase.mls.LogWarning($"Recoil: " + recoil);
            }
        }

        //Spawn a ragdoll on death
        [HarmonyPatch("KillEnemy")]
        [HarmonyPostfix]
        static void MaskedRagdoll(MaskedPlayerEnemy __instance, ref bool ___enemyEnabled, bool destroy = false)
        {
            if (destroy) return;

            //Disable mesh to pretend the ragdoll is the Masked
            ___enemyEnabled = false;
            __instance.SetVisibilityOfMaskedEnemy();
            __instance.EnableEnemyMesh(false, true);
            foreach (Collider c in __instance.gameObject.GetComponentsInChildren<Collider>()) c.enabled = false;

            //Recoil being blank will be used as the indication of unique ragdoll
            if (recoil == skip) return;

            //Ragdoll
            if (__instance.IsOwner) __instance.SyncPositionToClients();
            GameObject body = Object.Instantiate(RagdollModBase.ragdolls[ragdollID], __instance.serverPosition + Vector3.up, Quaternion.Euler(__instance.serverRotation), __instance.transform);
            if (stickPoint != null) {
                body.transform.parent = stickPoint;
                body.transform.localEulerAngles = new Vector3(77, 90, 0);
                body.transform.localPosition = default;
            } else body.transform.localPosition = Vector3.up;

            //Copy Skin
            if (body.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer skin)) skin.material = __instance.rendererLOD0.material;

            //Move props to ragdoll
            if (outfit != null) {
                outfit.transform.parent = body.transform;
                outfit.transform.localPosition = new Vector3(0, 0.02f, -0.01f);
                outfit.transform.localEulerAngles = new Vector3(10.984f, 0, 0);
                MoveAccessories(body, outfit);
            }

            //Set proper mask
            LODGroup[] masks = body.transform.GetComponentsInChildren<LODGroup>();
            if (!(__instance.maskTypes[0].activeSelf ^ __instance.maskTypes[1].activeSelf)) masks[0].gameObject.SetActive(true);
            else for (int i = 0; i < 2; i++) masks[i].gameObject.SetActive(__instance.maskTypes[i].activeSelf);
            if (!RagdollModBase.Instance.Configuration.Masked) for (int i = 0; i < masks.Length; i++) masks[i].gameObject.SetActive(false);

            //Ragdoll velocity
            Rigidbody[] limbs = body.GetComponentsInChildren<Rigidbody>();
            if (RagdollModBase.Instance.Configuration.Multiplier != 1 && recoil != default) {
                RagdollModBase.mls.LogInfo($"Multiplying ragdoll force.");
                recoil *= RagdollModBase.Instance.Configuration.Multiplier;
            }
            for (int i = 0; i < limbs.Length; i++) limbs[i].AddForce(recoil, ForceMode.VelocityChange);
        }

        //Returns clone of spine and all children
        private static GameObject Mimicking(MaskedPlayerEnemy masked)
        {
            GameObject find = masked.gameObject.transform.Find("ScavengerModel/metarig/spine").gameObject;
            if (find == null) return null;
            GameObject parent = GameObject.Instantiate(find, null);
            parent.transform.localScale = find.transform.lossyScale;
            return parent;
        }

        //Move the accessories found onto the ragdoll
        private static void MoveAccessories(GameObject ragdoll, GameObject props)
        {
            //Ragdoll transforms
            Transform root = ragdoll.transform.Find("spine.001");
            Transform spine = root.Find("spine.002");
            Transform chest = spine.Find("spine.003");
            Transform head = ragdoll.transform.Find("Head");
            if (head == null) head = chest.Find("spine.004");
            //arm.X_upper, arm.X_lower, hand.X, DecapitatedLegs

            //Outfit transforms
            Transform pRoot = props.transform.Find("spine.001");
            Transform pSpine = pRoot.Find("spine.002");
            Transform pChest = pSpine.Find("spine.003");
            Transform pHead = pChest.Find("spine.004");

            //Move presumably all props to ragdoll
            for (int h = pHead.childCount - 1; h >= 0; h--) {
                if (pHead.GetChild(h).name.Contains("(Clone)")) pHead.GetChild(h).SetParent(head);
            }
            for (int c = pChest.childCount - 1; c >= 0; c--) {
                if (pChest.GetChild(c).name.Equals("LevelSticker") || pChest.GetChild(c).name.Equals("BetaBadge") || pChest.GetChild(c).name.Contains("(Clone)")) pChest.GetChild(c).SetParent(chest);
            }
            for (int s = pSpine.childCount - 1; s >= 0; s--) {
                if (pSpine.GetChild(s).name.Contains("(Clone)")) pSpine.GetChild(s).SetParent(spine);
            }
            for (int r = pRoot.childCount - 1; r >= 0; r--) {
                if (pHead.GetChild(r).name.Contains("(Clone)")) pRoot.GetChild(r).SetParent(root);
            }
            for (int b = props.transform.childCount - 1; b >= 0; b--) {
                if (props.transform.GetChild(b).name.Contains("(Clone)")) props.transform.GetChild(b).SetParent(ragdoll.transform);
            }

            //Remove spawned outfit
            GameObject.Destroy(outfit);
            outfit = null;
        }

        //Play enemy sounds - Stops playing sounds when dies???
        private static void PlaySound(MaskedPlayerEnemy __instance, int ID, bool transmit = false)
        {
            __instance.creatureSFX.PlayOneShot(RagdollModBase.soundClips[ID]);
            if (transmit) WalkieTalkie.TransmitOneShotAudio(__instance.creatureSFX, RagdollModBase.soundClips[ID]);
        }

        //Return the nearest enemy of matching name to use as the attacker transform
        private static EnemyAI FindNearbyEnemies(MaskedPlayerEnemy masked, string match, float radiusOverride = 3.5f)
        {
            RaycastHit[] nearby = Physics.SphereCastAll(masked.serverPosition + Vector3.up, radiusOverride, Vector3.down, 2.5f, 524288);
            float nearest = 15;
            EnemyAI enemy = null;
            for (int i = 0; i < nearby.Length; i++) {
                if (nearby[i].transform.TryGetComponent<EnemyAICollisionDetect>(out EnemyAICollisionDetect collider)) {
                    if (masked.isEnemyDead || collider.mainScript.isEnemyDead) continue;
                    if (collider.mainScript.enemyType.enemyName.Equals(match)) {
                        if (Vector3.Distance(masked.serverPosition, collider.mainScript.serverPosition) < nearest) {
                            nearest = Vector3.Distance(masked.serverPosition, collider.mainScript.serverPosition);
                            enemy = collider.mainScript;
                        }
                    }
                }
            }
            if (enemy == null) RagdollModBase.mls.LogError($"Failed to find nearby " + match);
            return enemy;
        }

        //Return first turret position found that is firing and in LOS
        private static Vector3 FindNearbyTurret(MaskedPlayerEnemy masked)
        {
            RaycastHit[] nearby = Physics.SphereCastAll(masked.serverPosition, 15, Vector3.down, 1, 2097152);
            for (int i = 0; i < nearby.Length; i++) {
                if (nearby[i].transform.TryGetComponent<Turret>(out Turret turret)) {
                    if (turret.turretMode.Equals(TurretMode.Firing) || turret.turretMode.Equals(TurretMode.Berserk)) {
                        if (!Physics.Linecast(turret.aimPoint.position, masked.serverPosition, StartOfRound.Instance.collidersAndRoomMask)) return turret.transform.position;
                    }
                }
            }
            return default;
        }

        //Find spike trap above masked
        private static SpikeRoofTrap FindSpikeTrap(MaskedPlayerEnemy masked)
        {
            RaycastHit[] nearby = Physics.BoxCastAll(masked.serverPosition - Vector3.up, Vector3.one, Vector3.up, default, 3, 8192, QueryTriggerInteraction.Collide);
            for (int i = 0; i < nearby.Length; i++) {
                if (nearby[i].transform.TryGetComponent<SpikeRoofTrap>(out SpikeRoofTrap trap)) return trap;
            }
            return null;
        }
    }
}