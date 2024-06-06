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
            //ConfigControl config = STFixModBase.Instance.Configuration
            ConfigControl config = ConfigControl.Instance;

            //Mod Disabled
            if (!config.Mod) {
                STFixModBase.mls.LogWarning("Mod disabled.");
                return;
            }

            //Traps disabled
            if (!config.Traps) {
                Object.Destroy(__instance);
                STFixModBase.mls.LogInfo("Removed script from spike trap.");
                return;
            }
        }

        //Secondary checks, Types, Intervals, and Scan nodes
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartFix(SpikeRoofTrap __instance, ref float ___slamInterval, ref bool ___slamOnIntervals)
        {
            //ConfigControl config = STFixModBase.Instance.Configuration
            ConfigControl config = ConfigControl.Instance;

            //Mod Disabled
            if (!config.Mod) return;

            //Traps disabled (Safety net)
            if (!config.Traps) {
                Object.Destroy(__instance);
                return;
            }

            //Limit trap type
            if (!config.Types.Equals(STFixModBase.EnumOptions.Both)) {
                STFixModBase.mls.LogInfo("Changing trap to " + config.Types.ToString());
                ___slamOnIntervals = (config.Types.Equals(STFixModBase.EnumOptions.IntervalOnly));
            }

            //Clamp the intervals to the config range
            if (!config.Clamp || config.Types.Equals(STFixModBase.EnumOptions.DetectionOnly)) goto SCAN;

            //Logging changed intervals to both specify and notify. Done beforehand to show old value
            if (___slamInterval < config.Minimum) STFixModBase.mls.LogInfo($"Raised interval from " + ___slamInterval + "sec to " + config.Minimum + "sec.");
            if (___slamInterval > config.Maximum) STFixModBase.mls.LogInfo($"Lowered interval from " + ___slamInterval + "sec to " + config.Maximum + "sec.");
            ___slamInterval = Mathf.Clamp(___slamInterval, config.Minimum, config.Maximum);

        SCAN:
            //Spawn a scan node on the trap
            if (!config.Scans) return;

            //Scan node position
            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //Scan node parent and position changes depending on if it moves with the trap or not
            if (config.Move) {
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
            settings.maxRange = config.ScanRange;
            settings.minRange = 1;
            settings.requiresLineOfSight = true;
            settings.headerText = "Spike Trap";
            settings.subText = "";
            settings.scrapValue = 0;
            settings.creatureScanID = -1;
            settings.nodeType = 1;

            STFixModBase.mls.LogInfo("Added scan node to spike trap.");
        }
    }
}