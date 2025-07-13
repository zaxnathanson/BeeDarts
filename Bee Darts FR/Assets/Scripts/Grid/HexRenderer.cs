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

    // serialized just for debug purpose
    [SerializeField] private int previousHexGridRadius;
    [SerializeField] private float previousHexRadius;

    private void Start()
    {
        previousHexGridRadius = hexGridRadius;
        previousHexRadius = hexRadius;

        // only generate if in editor and no children
        if (!Application.isPlaying && transform.childCount == 0)
        {
            GenerateHexGrid(hexGridRadius);
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

    // generating a hex grid radially from a center point. radius is how many rings outward
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

    [ContextMenu("Generate Hex Grid")]
    public void GenerateGrid()
    {
        GenerateHexGrid(hexGridRadius);
    }
}