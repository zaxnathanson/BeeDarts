using UnityEngine;

public class BeeManager : MonoBehaviour
{
    public static BeeManager Instance { get; private set; }

    [Header("Respawn Bee Variables")]

    [SerializeField] private Transform firstBeeSpawn;
    [SerializeField] private Transform secondBeeSpawn;

    [SerializeField] private GameObject spawnedDart;
    [SerializeField] private float forceMultiplier;

    [Header("Public Variables")]

    public int totalBeesUnlocked = 1;
    public int beesOutOfHive = 0;

    public bool firstFlower = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        totalBeesUnlocked = 1;
        beesOutOfHive = 0;
    }

    public void IncrementTotalBees(int bees)
    {
        totalBeesUnlocked += bees;
    }

    public void DecrementTotalBees(int bees)
    {
        totalBeesUnlocked -= bees;
    }

    public void IncrementBeesOutOfHive(int bees)
    {
        beesOutOfHive += bees;
    }

    public void DecrementBeesOutOfHive(int bees)
    {
        beesOutOfHive -= bees;
    }

    public void RespawnBee()
    {
        Vector3 randomDirection = Random.onUnitSphere;
        //randomDirection.y = Random.Range(0.5f, 1f);
        //randomDirection.Normalize();

        if (!firstFlower)
        {
            GameObject bee = Instantiate(spawnedDart, firstBeeSpawn);

            bee.GetComponent<Rigidbody>().AddForce(randomDirection * forceMultiplier, ForceMode.Impulse);
        }
        else
        {
            GameObject bee = Instantiate(spawnedDart, secondBeeSpawn);

            bee.GetComponent<Rigidbody>().AddForce(randomDirection * forceMultiplier, ForceMode.Impulse);
        }
    }
}
