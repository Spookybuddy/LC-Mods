using HarmonyLib;
using UnityEngine;

namespace Breakables.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPatch("SpawnOutsideHazards")]
        [HarmonyPostfix]
        static void AddTriggers()
        {
            GameObject[] snowmen = GameObject.FindGameObjectsWithTag("Snowman");
            for (int i = 0; i < snowmen.Length; i++) {
                if (!snowmen[i].name.Contains("Snowman")) continue;
                BreakableSnowmenModBase.mls.LogInfo($"{snowmen[i].name} is now breakable");
                if (snowmen[i].name.Equals("Snowman(Clone)")) GameObject.Instantiate(BreakableSnowmenModBase.Instance.trigger, snowmen[i].transform);
                else GameObject.Instantiate(BreakableSnowmenModBase.Instance.tallTrigger, snowmen[i].transform);
            }
        }
    }
}