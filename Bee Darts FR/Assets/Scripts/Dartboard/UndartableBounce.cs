using UnityEngine;

public class UndartableBounce : MonoBehaviour
{
    [Header("Bounce Settings")]

    [SerializeField] private float bounceForce = 10f;

    private Rigidbody dartBody;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dart"))
        {
            Vector3 dartLastVelocity = collision.gameObject.GetComponent<Dart>().lastVelocity;

            dartBody = collision.gameObject.GetComponent<Rigidbody>();

            float forwardSpeed = Vector3.Dot(dartLastVelocity, collision.gameObject.transform.forward);
            // the bounced vector of the object
            Vector3 reflectedVel = Vector3.Reflect(collision.gameObject.transform.forward, collision.contacts[0].normal);
            // adding current speed to the bounced vector

            reflectedVel *= forwardSpeed;
            reflectedVel *= bounceForce;

            dartBody.linearVelocity = reflectedVel;
        }
    }
}
