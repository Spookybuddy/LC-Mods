using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

public class CustomKillTrigger : NetworkBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<PlayerControllerB>(out PlayerControllerB component)) {
            if (component == GameNetworkManager.Instance.localPlayerController && !component.isPlayerDead) {
                GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.down, true, CauseOfDeath.Gravity);
            }
        }
    }
}