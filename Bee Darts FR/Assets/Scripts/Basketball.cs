using UnityEngine;
public class Basketball : MonoBehaviour
{
    [Header("Shot Settings")]

    [SerializeField] private AudioClip hitSound;

    [SerializeField] private float shotForce = 15f;
    [SerializeField] private float arcHeight = 5f;
    [SerializeField] private float spinForce = 10f;

    private Rigidbody rb;

    private bool hasBeenShot = false;
    private bool dartCollisionDisabled = false;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        // get rigidbody component
        rb = GetComponent<Rigidbody>();
        
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // only shoot once per basketball
        if (!hasBeenShot && collision.gameObject.CompareTag("Dart"))
        {
            ShootBasketball(collision);

            // ignore dart after initial collision
            Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Dart"));

            hasBeenShot = true;
            dartCollisionDisabled = true;
        }
        // if ball has been shot and hits something that isnt a dart reenable dart collision
        else if (hasBeenShot && dartCollisionDisabled && !collision.gameObject.CompareTag("Dart"))
        {
            Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Dart"), false);
            dartCollisionDisabled = false;
        }

        GameManager.Instance.PlaySFXWithRandomPitch(hitSound, transform.position, 1f, 0.8f, 1.2f);
    }

    private void ShootBasketball(Collision collision)
    {
        // get direction from dart to basketball
        Vector3 hitDirection = (transform.position - collision.transform.position).normalized;
        // create arc trajectory
        Vector3 shootDirection = new Vector3(hitDirection.x, 0, hitDirection.z).normalized;
        Vector3 velocity = shootDirection * shotForce + Vector3.up * arcHeight;

        // kill any existing velocity and apply force
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = velocity;

        // add forward spin in direction of travel
        Vector3 spinAxis = Vector3.Cross(shootDirection, Vector3.up).normalized;
        rb.angularVelocity = spinAxis * spinForce;
    }

    public void ResetBall()
    {
        hasBeenShot = false;
        dartCollisionDisabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // reset position and rotation
        transform.position = startPosition;
        transform.rotation = startRotation;

        // reenable collision with dart layer
        Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Dart"), false);
    }

    [ContextMenu("Reset Ball")]
    private void ResetBallContextMenu()
    {
        ResetBall();
    }
}