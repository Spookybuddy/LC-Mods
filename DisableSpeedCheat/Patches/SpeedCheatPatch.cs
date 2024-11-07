using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine.InputSystem;

namespace SpeedCheat.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class SpeedCheatPatch
    {
        //Called before method is called, and returns false to skip the base, preventing the opening of the HUD
        [HarmonyPatch("SpeedCheat_performed")]
        [HarmonyPrefix]
        static bool PreventOpenMenu()
        {
            if (SpeedCheatBase.Disabled) SpeedCheatBase.mls.LogWarning($"Prevented opening Speed Cheat Menu");
            return !SpeedCheatBase.Disabled;
        }

        //For rebinding we need to actually try
        [HarmonyPatch("OnEnable")]
        [HarmonyPrefix]
        static void ChangeCheatBinding()
        {
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("SpeedCheat").ApplyBindingOverride(0, SpeedCheatBase.Binding);
            SpeedCheatBase.mls.LogWarning($"Attempted to change Cheat binding to {SpeedCheatBase.Binding}");
        }
    }
}