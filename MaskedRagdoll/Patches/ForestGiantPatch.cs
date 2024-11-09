using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;
using System.Reflection;
using System.Collections;

namespace MaskedRagdoll.Patches
{
    [HarmonyPatch(typeof(ForestGiantAI))]
    class ForestGiantPatch
    {
		[HarmonyPatch("AnimationEventA")]
		[HarmonyPrefix]
		static void FallOver(ref ForestGiantAI __instance)
        {
			RaycastHit[] array = Physics.SphereCastAll(__instance.deathFallPosition.position, 2.7f, __instance.deathFallPosition.forward, 4, 524288);
			for (int i = 0; i < array.Length; i++) {
				if (array[i].transform.TryGetComponent<MaskedPlayerEnemy>(out MaskedPlayerEnemy masked)) masked.HitEnemy(4, null, true, -219);
			}
		}

        [HarmonyPatch("GrabPlayerClientRpc")]
        [HarmonyPrefix]
        static bool OutOfBoundsCatch(int playerId, Vector3 enemyPosition, int enemyYRot, ref ForestGiantAI __instance)
        {
			//using an ID of -5 to indicate a masked was grabbed, catch it, and use the custom function
            if (playerId == -5) {
                RagdollModBase.mls.LogWarning($"Giant grabbed Masked!");
                MethodInfo method = typeof(ForestGiantAI).GetMethod("BeginEatPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
                var parameters = new object[] { null, enemyPosition, enemyYRot };
                method.Invoke(__instance, parameters);
            }
            return playerId != -5;
        }

        [HarmonyPatch("BeginEatPlayer")]
        [HarmonyPrefix]
        static bool GrabbedMasked(PlayerControllerB playerBeingEaten, Vector3 enemyPosition, int enemyYRot, ref ForestGiantAI __instance, ref Coroutine ___eatPlayerCoroutine, ref bool ___inEatingPlayerAnimation)
        {
            if (playerBeingEaten == null) {
                if (___eatPlayerCoroutine != null) __instance.StopCoroutine(___eatPlayerCoroutine);
				___inEatingPlayerAnimation = true;
                ___eatPlayerCoroutine = __instance.StartCoroutine(EatPlayerOverride(__instance, enemyPosition, enemyYRot));
                return false;
            } else return true;
        }

		//Custom IEnum that follows the animation procedures of the base IEnum without nay of the player references
		private static IEnumerator EatPlayerOverride(ForestGiantAI __instance, Vector3 enemyPosition, int enemyYRot)
		{
			__instance.creatureAnimator.SetTrigger("EatPlayer");
			__instance.inSpecialAnimation = true;
			Vector3 startPosition = __instance.serverPosition;
			Quaternion startRotation = __instance.transform.rotation;
			for (int i = 0; i < 10; i++) {
				__instance.transform.position = Vector3.Lerp(startPosition, enemyPosition, (float)i / 10f);
				__instance.transform.rotation = Quaternion.Lerp(startRotation, Quaternion.Euler(__instance.transform.eulerAngles.x, enemyYRot, __instance.transform.eulerAngles.z), (float)i / 10f);
				yield return new WaitForSeconds(0.01f);
			}
			__instance.transform.position = enemyPosition;
			__instance.transform.rotation = Quaternion.Euler(__instance.transform.eulerAngles.x, enemyYRot, __instance.transform.eulerAngles.z);
			__instance.serverRotation = __instance.transform.eulerAngles;
			yield return new WaitForSeconds(0.2f);
			__instance.inSpecialAnimation = false;
			yield return new WaitForSeconds(4.4f);
			__instance.creatureVoice.Stop();
			__instance.inSpecialAnimationWithPlayer = null;
			if (__instance.IsOwner) {
				if (__instance.CheckLineOfSightForPlayer(50f, 15) != null) _ = __instance.chasingPlayer;
				else __instance.SwitchToBehaviourState(0);
			}

			//Stop kill animation called to resume action
			MethodInfo method = typeof(ForestGiantAI).GetMethod("StopKillAnimation", BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(__instance, null);
		}
	}
}