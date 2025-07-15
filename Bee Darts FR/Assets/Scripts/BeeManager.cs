using TMPro;
using UnityEngine;

public class BeeManager : MonoBehaviour
{
    public static BeeManager Instance { get; private set; }

    [Header("Player Points Settings")]

    public int playerPoints = 0;
    public string pointsSuffix = " million points";

    [Header("Respawn Bee Variables")]

    [SerializeField] private Transform firstBeeSpawn;
    [SerializeField] private Transform secondBeeSpawn;

    [SerializeField] private GameObject spawnedDart;
    [SerializeField] private float forceMultiplier;

    [Header("Public Variables")]

    public int totalBeesUnlocked = 1;
    public int beesOutOfHive = 0;

    public bool firstFlower = false;

    [Header("References")]

    [SerializeField] private TextMeshProUGUI pointsText;

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

        pointsText = GameObject.Find("UI Canvas").transform.Find("Points").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        UpdatePointsUI();
    }

    private void Start()
    {
        totalBeesUnlocked = 1;
        beesOutOfHive = 0;
    }

    public void IncrementPoints(int points)
    {
        playerPoints += points;
    }

    public void DecrementPoints(int points)
    {
        playerPoints -= points;
    }

    public void RespawnBee()
    {
        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.Normalize();

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

    private void UpdatePointsUI()
    {
        if (playerPoints == 0)
        {
            pointsText.text = "";
        }
        else
        {
            pointsText.text = playerPoints.ToString() + pointsSuffix;
        }
    }
}
