using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HexManager : MonoBehaviour
{
    public static HexManager Instance { get; private set; }

    [Header("Lifting Settings")]

    [Tooltip("The animation time for raising the hexagons in seconds")]
    [SerializeField] private float animTime = 1f;
    [Tooltip("How far down the hexagons are put when lowered")]
    [SerializeField] private float lowerAmount = 10f;
    [Tooltip("The strength of the DOTween animation")]
    [SerializeField] private float animStrength = 1.5f;
    [Tooltip("Amount of time between raising each hexagon")]
    [SerializeField] private float plunkTime = 0.05f;

    [Header("Debug Values")]

    [Tooltip("The hex length to be used for hex grid snapping. Will calculate radius for you.")]
    [SerializeField] private float hexRadius;

    [SerializeField] private List<GameObject> hexagonsNotToLower = new();

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
        LowerHexagonsAtStart();
    }

    // lowering hexagons in list, no naimation
    public void LowerHexagonsInList(List<GameObject> selectedHexes)
    {
        foreach (GameObject hex in selectedHexes)
        {
            Vector3 targetPos = hex.transform.position;
            targetPos.y = -lowerAmount;
            hex.transform.position = targetPos;
        }
    }

    void LowerHexagonsAtStart()
    {
        GameObject[] hexagons = GameObject.FindGameObjectsWithTag("Hexagon");
        foreach (GameObject hex in hexagons)
        {
            if (!hexagonsNotToLower.Contains(hex))
            {
                Vector3 targetPos = hex.transform.position;
                targetPos.y = -lowerAmount;
                hex.transform.position = targetPos;
            }
            
        }
    }

    // lift hexagons in list, plunk plunk plunk animation
    public void LiftHexagonsInList(List<GameObject> selectedHexes)
    {
        StartCoroutine(PlunkDelay(selectedHexes));
    }

    public IEnumerator PlunkDelay(List<GameObject> selectedHexes)
    {
        foreach (GameObject hex in selectedHexes)
        {
            Vector3 targetPos = hex.transform.position;

            // making sure hexagon is on hexagon because of that chud ass error
            if (hex.TryGetComponent<Hexagon>(out Hexagon hexComponent))
            {
                targetPos.y = hexComponent.startingY;

                hex.transform.DOMove(targetPos, animTime).SetEase(Ease.OutBack, animStrength);
                yield return new WaitForSeconds(plunkTime);
            }
            else
            {
                Debug.LogWarning($"{hex.name} does not have hexagon script");
            }

            // punchy, overshooting animation type for hexagons coming up
            hex.transform.DOMove(targetPos, animTime).SetEase(Ease.OutBack, animStrength);
            yield return new WaitForSeconds(plunkTime);
        }
    }
}