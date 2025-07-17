using UnityEngine;

public class RespawnPad : MonoBehaviour
{
    [Header("Respawn Pad Settings")]

    [SerializeField] private float forceMultiplier = 1f;
    [SerializeField] private float beeSpawnOffset = 0.5f;

    private void Update()
    {
        if (BeeManager.Instance.waterLevel <= transform.position.y)
        {
            BeeManager.Instance.RegisterRespawnPad(this);
        }
    }

    public void SpawnBee(GameObject beePrefab)
    {
        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.Normalize();

        GameObject bee = Instantiate(beePrefab, new Vector3(transform.position.x, transform.position.y + beeSpawnOffset, transform.position.z), Quaternion.identity);
        bee.GetComponent<Rigidbody>().AddForce(randomDirection * forceMultiplier, ForceMode.Impulse);
    }
}