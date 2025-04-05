using UnityEngine;
using System.Collections.Generic;

namespace Breakables
{
    public class BreakSnowman : MonoBehaviour
    {
        public List<GameObject> prefabs = new List<GameObject>();
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<VehicleController>(out VehicleController car)) {
                if (car.averageVelocity.magnitude > 3) {
                    if (prefabs.Count > 0) {
                        int rng = (prefabs.Count > 1 ? ((int)(100 * Mathf.Abs(transform.position.y)) % prefabs.Count) : 0);
                        if (prefabs[rng] != null && rng < prefabs.Count && rng > -1) {
                            GameObject particles = GameObject.Instantiate(prefabs[rng], transform.parent.position, default, RoundManager.Instance.mapPropsContainer.transform);
                            if (transform.parent.name.Equals("SnowmanTall(Clone)")) particles.transform.localScale = new Vector3(1, 1.5f, 1);
                        } else BreakableSnowmenModBase.mls.LogError($"{rng} was either out of range or null!");
                    }
                    Destroy(transform.parent.gameObject);
                    car.CarReactToObstacle((car.mainRigidbody.position - transform.position).normalized, transform.position, Vector3.zero, CarObstacleType.Object);
                }
            }
        }
    }
}