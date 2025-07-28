using UnityEngine;

public class CloudRespawner : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cloud"))
        {
            // reset collider is on one side, just teleport to opposite position
            other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, -transform.position.z);
        }
    }
}
