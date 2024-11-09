using HarmonyLib;

namespace Costumes.Patches
{
    [HarmonyPatch(typeof(FlowerSnakeEnemy))]
    class TulipPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void PutOnCostume(FlowerSnakeEnemy __instance)
        {
            if (CostumesModBase.Tulipsnake.Value) UnityEngine.GameObject.Instantiate(CostumesModBase.costumes[1], __instance.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0));
        }
    }
}