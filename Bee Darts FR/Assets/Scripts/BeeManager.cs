using TMPro;
using UnityEngine;
using DG.Tweening;

public class BeeManager : MonoBehaviour
{
    public static BeeManager Instance { get; private set; }

    [Header("Player Points Settings")]

    public int playerPoints = 0;
    private int previousPlayerPoints = -1;
    public string pointsSuffix = " MILLION POINTS";
    private Color originalColor;
    [SerializeField] private Color colorToPunch;
    [SerializeField] private float punchPower;
    [SerializeField] private float punchDuration;
    [SerializeField] private int punchVibrato;

    [SerializeField] Gradient hoverGradient;
    [SerializeField] private float gradientSpeed;
    float gradientTime;

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
        originalColor = pointsText.color;
    }

    private void Update()
    {
        gradientTime += (Time.deltaTime / 2) * gradientSpeed;
        if (gradientTime > 1)
        {
            gradientTime -= 1;
        }

        UpdatePointsUI();
    }

    private void Start()
    {
        totalBeesUnlocked = 1;
        beesOutOfHive = 0;
    }

    public void IncrementPoints(int points)
    {
        previousPlayerPoints = playerPoints;
        playerPoints += points;
    }

    public void DecrementPoints(int points)
    {
        previousPlayerPoints = playerPoints;
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
        Material mat = pointsText.fontSharedMaterial;

        mat.SetColor(ShaderUtilities.ID_GlowColor, hoverGradient.Evaluate(gradientTime));

        if (playerPoints != previousPlayerPoints)
        {
            if (playerPoints == 0)
            {
                pointsText.text = "";
            }
            else
            {
                pointsText.text = playerPoints.ToString() + pointsSuffix;

                // punch scale
                pointsText.transform.DOPunchScale(Vector3.one * punchPower, punchDuration, punchVibrato, 1f);
                // fade in color and fade out to original
                pointsText.DOColor(colorToPunch, 0.15f).SetEase(Ease.OutQuad).OnComplete(() => {pointsText.DOColor(originalColor, 0.15f).SetEase(Ease.InQuad);});
            }

            previousPlayerPoints = playerPoints;
        }
    }
}
