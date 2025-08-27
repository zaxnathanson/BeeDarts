using UnityEngine;

public class Balloon : MonoBehaviour
{
    [Header("Balloon Settings")]

    [SerializeField] private AudioClip balloonHit;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dart"))
        {
            if (balloonHit != null)
            {
                GameManager.Instance.PlaySFXWithRandomPitch(balloonHit, collision.transform.position, 1f, 0.85f, 1.15f);
            }
        }
    }
}