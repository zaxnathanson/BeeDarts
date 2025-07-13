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

    [Tooltip("The list to be used for context menu debug functions")]
    [SerializeField] private List<GameObject> debugSelectedHexes;
    [Tooltip("The transform to be used for the context menu debug functions (for spheres)")]
    [SerializeField] private Transform debugTransform;
    [Tooltip("The radius horizontally that a specific transform will check for hexagons. Vertical height does not matter")]
    [SerializeField] private float debugRadius = 5f;

    [SerializeField] private List<GameObject> hexagonsToLower = new();

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

        LowerHexagonsAtStart();
    }


    // lowering hexagons in a sphere around a defined transform. no animation

/*    public void LowerHexagonsInSphere(Transform center, float radius)
    {
        foreach (GameObject hex in hexagonsToLower)
        {
            // if distance to transform is within radius
            float distance = Vector3.Distance(hex.transform.position, center.position);

            // lower hex
            if (distance <= radius)
            {
                Vector3 targetPos = hex.transform.position;
                targetPos.y = -lowerAmount;
                hex.transform.position = targetPos;
            }
        }
    }*/

    //lifting hexagons in sphere around defined transform. animated
/*    public void LiftHexagonsInSphere(Transform center, float radius)
    {
        foreach (GameObject hex in hexagonsToLower)
        {
            Vector3 hexPos = hex.transform.position;
            Vector3 centerPos = center.position;

            // calculating only horizontal distance
            float distance = Vector2.Distance(new Vector2(hexPos.x, hexPos.z), new Vector2(centerPos.x, centerPos.z));

            if (distance <= radius)
            {
                Vector3 targetPos = hex.transform.position;
                targetPos.y = 0;

                // punchy, overshooting animation type for hexagons coming up
                hex.transform.DOMove(targetPos, animTime).SetEase(Ease.OutBack, animStrength);
            }
        }
    }*/

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
        foreach (GameObject hex in hexagonsToLower)
        {
            Vector3 targetPos = hex.transform.position;
            targetPos.y = -lowerAmount;
            hex.transform.position = targetPos;
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
            targetPos.y = 0;

            // punchy, overshooting animation type for hexagons coming up
            hex.transform.DOMove(targetPos, animTime).SetEase(Ease.OutBack, animStrength);
            yield return new WaitForSeconds(plunkTime);
        }
    }

    private void OnDrawGizmos()
    {
        if (debugTransform != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawSphere(debugTransform.position, debugRadius);
            Gizmos.DrawWireSphere(debugTransform.position, debugRadius);
        }
    }

/*    [ContextMenu("Lower Hexagons in sphere")]
    public void TestLowerWithSphere()
    {
        if (debugTransform != null)
        {
            LowerHexagonsInSphere(debugTransform, debugRadius);
        }
        else
        {
            Debug.LogWarning("Debug transform unassigned");
        }
    }*/

/*    [ContextMenu("Lift Hexagons in sphere")]
    public void TestLiftWithSphere()
    {
        if (debugTransform != null)
        {
            LiftHexagonsInSphere(debugTransform, debugRadius);
        }
        else
        {
            Debug.LogWarning("Debug transform unassigned");
        }
    }*/

    [ContextMenu("Lower Hexagons in list")]
    public void TestLowerInList()
    {
        if (debugSelectedHexes.Count > 0)
        {
            LowerHexagonsInList(debugSelectedHexes);
        }
        else
        {
            Debug.LogWarning("Debug list is empty");
        }
    }

    [ContextMenu("Lift Hexagons in list")]
    public void TestLiftInList()
    {
        if (debugSelectedHexes.Count > 0)
        {
            LiftHexagonsInList(debugSelectedHexes);
        }
        else
        {
            Debug.LogWarning("Debug list is empty");
        }
    }

    [ContextMenu("Remove Selected Hexagons from Lowering List")]
    public void RemoveSelectedHexagons()
    {
        foreach (GameObject hex in Selection.gameObjects)
        {
            hexagonsToLower.Remove(hex);
        }
        hexagonsToLower.TrimExcess();
    }
}