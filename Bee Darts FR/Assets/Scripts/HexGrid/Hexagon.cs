using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Hexagon : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("The radius of the hexagon for grid snapping")]
    [SerializeField] private float hexRadius = 2f;

    [Header("Snapping")]
    [Tooltip("Enable/disable grid snapping")]
    [SerializeField] private bool snapToGrid = true;

    public float startingY = 0;
    private Vector3 lastPosition;
    private void Awake()
    {
        // for raising it back to
        startingY = transform.position.y;

        lastPosition = transform.position;
    }


#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying && snapToGrid)
        {
            SnapToGrid();
        }

    }
#endif

    private void SnapToGrid()
    {
        Vector3 currentPos = transform.position;

        // get world pos, figure out what axial coord its nearest to, and then getting world coords of that axial coord
        Hex hexCoords = WorldToHex(currentPos.x, currentPos.z, hexRadius);
        Vector3 snappedPos = HexToWorld(hexCoords.q, hexCoords.r, hexRadius);

        snappedPos.y = currentPos.y;

        transform.position = snappedPos;
    }

    // world space coords to axial coords
    private Hex WorldToHex(float x, float z, float hexRadius)
    {
        float q = (2f / 3f * x) / hexRadius;
        float r = (-1f / 3f * x + Mathf.Sqrt(3f) / 3f * z) / hexRadius;

        return HexRound(q, r);
    }

    // aixal coords to world space coorsd
    private Vector3 HexToWorld(int q, int r, float hexRadius)
    {
        float x = hexRadius * 3f / 2f * q;
        float y = 0;
        float z = hexRadius * Mathf.Sqrt(3f) * (r + q / 2f);
        return new Vector3(x, y, z);
    }

    //rounding out calculated hex coordinates for precision
    private Hex HexRound(float q, float r)
    {
        // im sorry this looks so unintelligible q r and s are the axialcoords
        float s = -q - r;

        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float q_diff = Mathf.Abs(rq - q);
        float r_diff = Mathf.Abs(rr - r);
        float s_diff = Mathf.Abs(rs - s);

        if (q_diff > r_diff && q_diff > s_diff)
        {
            rq = -rr - rs;
        }
        else if (r_diff > s_diff)
        {
            rr = -rq - rs;
        }

        return new Hex(rq, rr);
    }

    [ContextMenu("Snap to Grid")]
    public void ManualSnapToGrid()
    {
        SnapToGrid();
    }
}