using UnityEngine;

public class CompletedShot : MonoBehaviour
{
    [Header("Completed Shot Settings")]

    [SerializeField] private GameObject shotParticle;
    [SerializeField] private AudioClip cheerSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Undartable"))
        {
            Instantiate(shotParticle, transform.position, Quaternion.identity);
            GameManager.Instance.PlaySFX(cheerSound, transform.position, 1f);
        }
    }
}
