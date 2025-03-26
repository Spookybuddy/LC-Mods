using HarmonyLib;
using UnityEngine;

namespace STFixes.Patches
{
    [HarmonyPatch(typeof(SpikeRoofTrap))]
    internal class SpikeRoofTrapPatch
    {
        internal static readonly Vector3 moving = new Vector3(-0.48f, 0, -0.378f);
        internal static readonly Vector3 still = new Vector3(0, 3, 1.8f);

        //Mod disabled/Traps disabled primary check. Saves on resources if script is deleted first
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void BeginningFix(SpikeRoofTrap __instance)
        {
            //Mod Disabled
            if (!ConfigControl.Instance.Mod) {
                STFixModBase.mls.LogWarning("Mod disabled.");
                return;
            }

            //Traps disabled
            if (!ConfigControl.Instance.Traps) {
                Object.Destroy(__instance);
                STFixModBase.mls.LogInfo("Removed script from spike trap.");
                return;
            }

            //Audio fix
            if (STFixModBase.soundFix != null) {
                //`__instance.spikeTrapAudio.clip = STFixModBase.soundFix;
                __instance.spikeTrapAudio.loop = false;
                PlayAudioAnimationEvent soundfix = __instance.GetComponentInParent<PlayAudioAnimationEvent>();
                soundfix.audioClip2 = STFixModBase.soundFix;
                soundfix.audioClip3 = STFixModBase.soundFix;
            }
        }

        //Secondary checks, Types, Intervals, and Scan nodes
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartFix(SpikeRoofTrap __instance, ref float ___slamInterval, ref bool ___slamOnIntervals)
        {
            //Mod Disabled
            if (!ConfigControl.Instance.Mod) return;

            //Traps disabled (Safety net)
            if (!ConfigControl.Instance.Traps) {
                Object.Destroy(__instance);
                return;
            }

            //Limit trap type
            if (!ConfigControl.Instance.Types.Equals(STFixModBase.EnumOptions.Both)) {
                STFixModBase.mls.LogInfo("Changing trap to " + ConfigControl.Instance.Types.ToString());
                ___slamOnIntervals = (ConfigControl.Instance.Types.Equals(STFixModBase.EnumOptions.IntervalOnly));
            }

            //Clamp the intervals to the config range
            if (!ConfigControl.Instance.Clamp || ConfigControl.Instance.Types.Equals(STFixModBase.EnumOptions.DetectionOnly)) goto SCAN;

            //Logging changed intervals to both specify and notify. Done beforehand to show old value
            if (___slamInterval < ConfigControl.Instance.Minimum) STFixModBase.mls.LogInfo($"Raised interval from {___slamInterval} sec to {ConfigControl.Instance.Minimum} sec.");
            if (___slamInterval > ConfigControl.Instance.Maximum) STFixModBase.mls.LogInfo($"Lowered interval from {___slamInterval} sec to {ConfigControl.Instance.Maximum} sec.");
            ___slamInterval = Mathf.Clamp(___slamInterval, ConfigControl.Instance.Minimum, ConfigControl.Instance.Maximum);

        SCAN:
            //Spawn a scan node on the trap
            if (!ConfigControl.Instance.Scans) return;

            //Scan node position
            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //Scan node parent and position changes depending on if it moves with the trap or not
            if (ConfigControl.Instance.Move) {
                node.transform.parent = __instance.stickingPointsContainer;
                node.transform.localPosition = moving;
            } else {
                node.transform.parent = __instance.laserEye;
                node.transform.localPosition = still;
            }

            //Scan node settings
            node.tag = "DoNotSet";
            node.layer = 22;
            Object.Destroy(node.GetComponent<MeshFilter>());
            Object.Destroy(node.GetComponent<MeshRenderer>());

            //Scan node script variables
            ScanNodeProperties settings = node.AddComponent<ScanNodeProperties>();
            settings.maxRange = ConfigControl.Instance.ScanRange;
            settings.minRange = 1;
            settings.requiresLineOfSight = true;
            settings.headerText = "Spike Trap";
            settings.subText = "";
            settings.scrapValue = 0;
            settings.creatureScanID = -1;
            settings.nodeType = 1;

            STFixModBase.mls.LogInfo("Added scan node to spike trap.");
        }

        //Terminal disable fix
        [HarmonyPatch("ToggleSpikesEnabledLocalClient")]
        [HarmonyPostfix]
        static void EnableFix(SpikeRoofTrap __instance, bool enabled)
        {
            __instance.transform.parent.GetChild(3).gameObject.SetActive(enabled);
        }
    }
}