using UnityEngine;

public class Clouds : MonoBehaviour
{
    [SerializeField] private float cloudSpeed;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // clouds travel in negative z direction
        rb.linearVelocity = new Vector3(0, 0, -cloudSpeed);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 1f);
    }
}
