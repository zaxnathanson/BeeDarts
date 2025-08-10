using UnityEngine;
using System.Collections.Generic;

public class ResetCans : MonoBehaviour
{
    [Header("Particle Settings")]

    [SerializeField] private float particleYOffset = 7f;
    [SerializeField] private GameObject winParticle;

    private List<Transform> cans = new List<Transform>();

    private List<Vector3> canPositions = new List<Vector3>();
    private List<Quaternion> canRotations = new List<Quaternion>();

    private int numCansFallen = 0;

    private void Awake()
    {
        // get all can colliders
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            cans.Add(transform.GetChild(i).GetChild(0));
        }

        // get all added positions
        for (int i = 0; i < cans.Count; i++)
        {
            canPositions.Add(cans[i].position);
            canRotations.Add(cans[i].rotation);

            cans[i].position = new Vector3(cans[i].position.x, cans[i].position.y - 15f, cans[i].position.z);
        }
    }

    private void Update()
    {
        if (numCansFallen == cans.Count)
        {
            if (winParticle != null)
            {
                Vector3 particleSpawn = new Vector3(transform.position.x, transform.position.y + particleYOffset, transform.position.z);
                Instantiate(winParticle, particleSpawn, Quaternion.identity);
            }

            numCansFallen = 0;
        }
    }

    public void ReturnCans()
    {
        Debug.Log("returning cans");

        for (int i = 0; i < cans.Count; i++)
        {
            cans[i].gameObject.GetComponent<Rigidbody>().isKinematic = true;

            cans[i].position = canPositions[i];
            cans[i].rotation = canRotations[i];

            cans[i].gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Undartable"))
        {
            numCansFallen++;
        }
    }
}