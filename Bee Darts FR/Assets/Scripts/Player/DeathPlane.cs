using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    [Header("Respawn Settings")]
    
    private Vector3 playerRespawnPoint;
    private GameObject player;
    private GameObject deathBox;

    private void Awake()
    {
        player = GameObject.Find("Player");
        deathBox = GameObject.Find("PrisonParent");

        playerRespawnPoint = player.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            deathBox.transform.GetChild(0).gameObject.SetActive(true);

            deathBox.GetComponent<FadingPrison>().StartFadeOut();

            player.transform.position = playerRespawnPoint;
        }
    }
}
