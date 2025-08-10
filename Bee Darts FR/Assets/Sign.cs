using UnityEngine;

public class Sign : MonoBehaviour
{
    [Header("Sign Break Settings")]

    [SerializeField] private float breakForce = 5f;
    [SerializeField] private float torqueForce = 1f;

    [Header("Audio Settings")]

    [SerializeField] private AudioClip woodBreak;
    [SerializeField] private float audioVolume = 1f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dart"))
        {
            if (rb == null)
            {
                Debug.LogWarning("No rigidbody on tha sign bro.");
                return;
            }

            // ignore any additional collisions after initial
            Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Dart"));

            if (woodBreak != null)
            {
                GameManager.Instance.PlaySFXWithRandomPitch(woodBreak, collision.transform.position, audioVolume, 0.85f, 1.15f);
            }

            BreakSign(collision);
        }
    }

    private void BreakSign(Collision collision)
    {
        // adding rigidbody
        rb.isKinematic = false;

        Vector3 forceDirection = (transform.position - collision.gameObject.transform.position).normalized;

        // randomized torque for funny break effect
        Vector3 randomTorque = new Vector3(
            Random.Range(-torqueForce, torqueForce),
            Random.Range(-torqueForce, torqueForce),
            Random.Range(-torqueForce, torqueForce)
        ) * breakForce;

        // making break force independent of bee throwing strength
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(forceDirection * breakForce, ForceMode.Impulse);
        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }
}