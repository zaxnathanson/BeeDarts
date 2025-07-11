using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public struct Hex
{
    public int q;
    public int r;

    public Hex(int q, int r)
    {
        this.q = q;
        this.r = r;
    }
}

[ExecuteInEditMode]
public class HexRenderer : MonoBehaviour
{
    [Header("Grid Settings")]

    [Tooltip("The prefab of the hexagon")]
    [SerializeField] private GameObject hexPrefab;
    [Tooltip("The number of rings in the generated hex grid")]
    [SerializeField] private int hexGridRadius = 5;
    [Tooltip("The radius of each individual hexagon")]
    [SerializeField] private float hexRadius = 2;

    // serialized just for debug prupose
    [SerializeField] private int previousHexGridRadius;
    [SerializeField] private float previousHexRadius;

    [Header("Lifting Settings")]

    [Tooltip("The animation time for raising the hexagons in seconds")]
    [SerializeField] private float animTime = 1f;
    [Tooltip("How far down the hexagons are put when lowered")]
    [SerializeField] private float lowerAmount = 10f;
    [Tooltip("The strength of the DOTween animation")]
    [SerializeField] private float animStrength = 1.5f;

    [Header("Debug Values")]

    [SerializeField] private Transform debugTransform;
    [Tooltip("The radius horizontally that a specific transform will check for hexagons. Vertical height does not matter")]
    [SerializeField] private float debugRadius = 5f;

    private List<GameObject> allHexagons = new List<GameObject>();

    private void Start()
    {
        previousHexGridRadius = hexGridRadius;
        previousHexRadius = hexRadius;

        // only generate if in editor and no children
        if (!Application.isPlaying && transform.childCount == 0)
        {
            GenerateHexGrid(hexGridRadius);
        }

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Hexagon"))
        {
            allHexagons.Add(obj);
        }
    }

    //editor only onvalidate runs when an inspector value updates
    private void OnValidate()
    {
        if (hexGridRadius != previousHexGridRadius || hexRadius != previousHexRadius)
        {
            SmartRegenerateGrid();

            previousHexGridRadius = hexGridRadius;
            previousHexRadius = hexRadius;
        }
    }

    // regenerating only hexagons that do not have children attached to them
    private void SmartRegenerateGrid()
    {
        List<GameObject> hexesWithChildren = new List<GameObject>();
        List<Vector3> preservedPositions = new List<Vector3>();

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.transform.childCount > 0)
            {
                hexesWithChildren.Add(child);
                preservedPositions.Add(child.transform.position);
            }
        }

        for (int i = transform.childCount - 1; i > 0; i--)
        {
            // bruh moment ahhhh destroy call
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            };
        }

        GenerateHexGrid(hexGridRadius);

        // removing new hexes that overlap stored ones
        for (int i = 0; i < preservedPositions.Count; i++)
        {
            Vector3 preservedPos = preservedPositions[i];

            for (int j = transform.childCount - 1; j >= 0; j--)
            {
                GameObject newHex = transform.GetChild(j).gameObject;

                // if new hex is right around where stored hex is
                if (Vector3.Distance(newHex.transform.position, preservedPos) < 0.1f)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate(newHex);
                    };
                    break;
                }
            }

            if (i < hexesWithChildren.Count && hexesWithChildren[i] != null)
            {
                GameObject restoredHex = Instantiate(hexesWithChildren[i], preservedPos, Quaternion.identity, transform);
                restoredHex.name = hexesWithChildren[i].name;
            }
        }
    }

    // geenerating a hex grid radially from a center point. radius is how many rings outward
    private void GenerateHexGrid(float hexGridRadius)
    {
        // generating new hexes
        for (int q = -(int)(hexGridRadius); q <= hexGridRadius; q++)
        {
            int r1 = Mathf.Max(-(int)(hexGridRadius), -q - (int)(hexGridRadius));
            int r2 = Mathf.Min((int)(hexGridRadius), -q + (int)(hexGridRadius));

            for (int r = r1; r <= r2; r++)
            {
                Vector3 pos = HexToWorld(q, r, hexRadius);
                GameObject hex = Instantiate(hexPrefab, pos, Quaternion.identity, transform);
                //allHexagons.Add(hex);
            }
        }
    }

    private Vector3 HexToWorld(int q, int r, float hexRadius)
    {
        float x = hexRadius * 3f / 2f * q;
        float y = 0;
        float z = hexRadius * Mathf.Sqrt(3f) * (r + q / 2f);
        return new Vector3(x, y, z);
    }

    // lifting all hexagons to begin with, below the ythreshold
    public void LiftHexagonsBelowY(float yThreshold)
    {
        foreach (GameObject hex in allHexagons)
        {
            if (hex.transform.position.y < yThreshold)
            {
                Vector3 targetPos = hex.transform.position;
                targetPos.y = 0; //ground level is 0
                hex.transform.DOMove(targetPos, animTime);
            }
        }
    }

    // lowering hexagons in a sphere around a defined transform. no animation
    public void LowerHexagonsInSphere(Transform center, float radius)
    {
        foreach (GameObject hex in allHexagons)
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
    }

    //lifting hexagons in  sphere around defined transform. animated
    public void LiftHexagonsInSphere(Transform center, float radius)
    {
        foreach (GameObject hex in allHexagons)
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
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(debugTransform.position, debugRadius);
        Gizmos.DrawWireSphere(debugTransform.position, debugRadius);
    }

    [ContextMenu("Lift all Hexagons")]
    public void TestLiftBelowY()
    {
        LiftHexagonsBelowY(-5f);
    }

    [ContextMenu("Lower Hexagons")]
    public void TestLowerAtDebugTransform()
    {
        if (debugTransform != null)
        {
            LowerHexagonsInSphere(debugTransform, debugRadius);
        }
        else
        {
            Debug.LogWarning("Debug transform unassigned");
        }
    }

    [ContextMenu("Lift Hexagons in radius of transform")]
    public void TestLiftAtDebugTransform()
    {
        if (debugTransform != null)
        {
            LiftHexagonsInSphere(debugTransform, debugRadius);
        }
        else
        {
            Debug.LogWarning("Debug transform unassigned");
        }
    }
}
