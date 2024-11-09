using HarmonyLib;

namespace Costumes.Patches
{
    [HarmonyPatch(typeof(HoarderBugAI))]
    internal class LootbugPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void PutOnCostume(HoarderBugAI __instance)
        {
            if (CostumesModBase.Lootbug.Value) UnityEngine.GameObject.Instantiate(CostumesModBase.costumes[0], __instance.transform.GetChild(2).GetChild(2).GetChild(0).GetChild(0).GetChild(2));
        }
    }
}