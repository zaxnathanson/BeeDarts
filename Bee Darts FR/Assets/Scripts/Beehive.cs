using UnityEngine;

public class Beehive : MonoBehaviour
{

    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private SphereCollider sphereColl;

    [SerializeField] private GameObject beeDart;
    [SerializeField] private Transform beeSpawnPoint;

    private void Start()
    {
        sphereColl = GetComponent<SphereCollider>();
        sphereColl.radius = interactionRadius;
    }

    /*
    private void SpawnBee(bool toBeeOrNotToBee)
    {
        if (toBeeOrNotToBee)
        {
            Instantiate(beeDart, beeSpawnPoint);
        }
        else
        {
            Debug.Log("nah you cant do that ");
        }
    }
    */

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius / 2.0f);
    }
}
