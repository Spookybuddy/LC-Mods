using UnityEngine;
using System.Collections.Generic;

namespace CustomOutsideObjects
{
    public class BreakObject : MonoBehaviour
    {
        public List<GameObject> prefabs = new List<GameObject>();
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<VehicleController>(out VehicleController car)) {
                if (car.averageVelocity.magnitude > 5f) {
                    //Particle Effects
                    if (prefabs.Count > 0) {
                        int rng = (prefabs.Count > 1 ? ((int)(100 * Mathf.Abs(transform.position.y)) % prefabs.Count) : 0);
                        if (prefabs[rng] != null && rng < prefabs.Count && rng > -1) GameObject.Instantiate(prefabs[rng], transform.parent.position, default, RoundManager.Instance.mapPropsContainer.transform);
                        else CustomOutsideModBase.mls.LogError($"{rng} was either out of range or null!");
                    }
                    //Destroy
                    Destroy(transform.parent.gameObject);
                    car.CarReactToObstacle((car.mainRigidbody.position - transform.position).normalized, transform.position, Vector3.zero, CarObstacleType.Object);
                }
            }
        }
    }
}