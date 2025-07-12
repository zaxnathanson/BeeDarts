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
    [SerializeField] private GameObject hexPrefab;

    [Header("Grid Settings")]

    [SerializeField] private int hexGridRadius = 5;
    [SerializeField] private float hexRadius = 2;
    [SerializeField] private float random;


    private void Awake()
    {
        GenerateHexGrid(hexGridRadius);
    }

    // geenerating a hex grid radially from a center point. radius is how many rings outward
    private void GenerateHexGrid(float hexGridRadius)
    {
        //looping over axial q coordinates from -radius to radius
        for (int q = -(int)(hexGridRadius); q <= hexGridRadius; q++)
        {
            int r1 = Mathf.Max(-(int)(hexGridRadius), -q - (int)(hexGridRadius));
            int r2 = Mathf.Min((int)(hexGridRadius), -q + (int)(hexGridRadius));

            for (int r = r1; r <= r2; r++)
            {
                Vector3 pos = HexToWorld(q, r, hexRadius);
                Instantiate(hexPrefab, pos, Quaternion.identity, transform);
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
}
