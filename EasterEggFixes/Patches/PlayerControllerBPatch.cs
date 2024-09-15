using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using Unity.Netcode;
using System.Reflection;

namespace EggFixes.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("DiscardHeldObject")]
        [HarmonyPrefix]
        static bool ThrowPrep(bool placeObject, NetworkObject parentObjectTo, Vector3 placePosition, PlayerControllerB __instance)
        {
            //Only need this code for ExplodeOnThrow. The others are set on EquipItem
            if (!ConfigControl.Instance.Enabled || ConfigControl.Instance.ExplodeWhen != EasterEggFixesModBase.EggSettings.ExplodeOnThrow) return true;
            
            //Check if held item is an Egg
            if (__instance.currentlyHeldObjectServer.itemProperties.itemName.Equals("Easter egg")) {
                
                //Check if the Egg has been thrown
                if (placeObject && parentObjectTo == null && placePosition != default) {
                    EasterEggFixesModBase.mls.LogWarning("Egg Thrown!");

                    //If thrown call the required components from the original function
                    __instance.playerBodyAnimator.SetBool("cancelHolding", value: true);
                    __instance.playerBodyAnimator.SetTrigger("Throw");
                    HUDManager.Instance.itemSlotIcons[__instance.currentItemSlot].enabled = false;
                    HUDManager.Instance.holdingTwoHandedItem.enabled = false;

                    //Call the throw function with custom parameter to mark it as being thrown
                    placePosition = ((!__instance.isInElevator) ? StartOfRound.Instance.propsContainer.InverseTransformPoint(placePosition) : StartOfRound.Instance.elevatorTransform.InverseTransformPoint(placePosition));
					__instance.SetObjectAsNoLongerHeld(__instance.isInElevator, __instance.isInHangarShipRoom, placePosition, __instance.currentlyHeldObjectServer, (int)__instance.transform.localEulerAngles.y);
					__instance.currentlyHeldObjectServer.DiscardItemOnClient();
                    //__instance.ThrowObjectServerRpc(__instance.currentlyHeldObjectServer.gameObject.GetComponent<NetworkObject>(), __instance.isInElevator, __instance.isInHangarShipRoom, placePosition, floorYRot);
                    
                    //Reflection invocation of private method with custom var
                    MethodInfo method = typeof(PlayerControllerB).GetMethod("ThrowObjectServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);
                    NetworkObjectReference grabbedObject = __instance.currentlyHeldObjectServer.gameObject.GetComponent<NetworkObject>();
                    var parameters = new object[] { grabbedObject, __instance.isInElevator, __instance.isInHangarShipRoom, placePosition, -211 };
                    method.Invoke(__instance, parameters);
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch("ThrowObjectServerRpc")]
        [HarmonyPrefix]
        static void ThrowFix(NetworkObjectReference grabbedObject, bool droppedInElevator, bool droppedInShipRoom, Vector3 targetFloorPosition, int floorYRot, PlayerControllerB __instance)
        {
            //Only need this code for ExplodeOnThrow. The others are set on EquipItem
            if (__instance.currentlyHeldObjectServer == null || !ConfigControl.Instance.Enabled || ConfigControl.Instance.ExplodeWhen != EasterEggFixesModBase.EggSettings.ExplodeOnThrow) return;
            
            //Check if held item is an Egg
            if (__instance.currentlyHeldObjectServer.itemProperties.itemName.Equals("Easter egg")) {
                //If specially marked update the egg to explode
                if (floorYRot == -211) {
                    EasterEggFixesModBase.mls.LogInfo("BOOM!");
                    if (__instance.currentlyHeldObjectServer.TryGetComponent(out StunGrenadeItem item)) {
                        item.chanceToExplode = 111;
                        item.SetExplodeOnThrowServerRpc();
                    } else {
                        //The egg doesnt have an egg script???
                        EasterEggFixesModBase.mls.LogError("Threw an egg that did not have the StunGrenadeItem script!");
                    }
                } else EasterEggFixesModBase.mls.LogInfo("Dropped Egg!");
            }
        }
    }
}