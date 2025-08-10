using UnityEngine;

public class Can : MonoBehaviour
{
    [Header("Can Settings")]

    [SerializeField] private AudioClip canTink;

    public bool canDown = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dart"))
        {
            if (canTink != null)
                GameManager.Instance.PlaySFXWithRandomPitch(canTink, collision.transform.position, 1f, 0.75f, 1.25f);
        }
    }
}
