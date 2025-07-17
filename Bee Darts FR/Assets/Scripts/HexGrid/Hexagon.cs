using UnityEngine;
using System.Collections.Generic;

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

    [Header("Side Settings")]

    [SerializeField] private float bounceForce;
    [SerializeField] private float verticalThreshold;

    [Header("Decoration Settings")]
    public Sprite[] grassSprites;
    public Sprite[] flowerSprites;
    public float minDistance = 0.3f;
    public int maxAttempts = 30;
    public float grassChance = 0.7f;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public string sortingLayer = "Default";
    public int sortingOrder = 1;

    public float startingY = 0;

    private void Awake()
    {
        // for raising it back to
        startingY = transform.position.y;
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

    private void Start()
    {
        if (Application.isPlaying)
        {
            // wait a frame to ensure hexagons are positioned first
            StartCoroutine(DelayedGeneration());
        }
    }

    System.Collections.IEnumerator DelayedGeneration()
    {
        yield return null; // wait one frame
        GenerateDecorations();
    }

    [ContextMenu("Clear All Decorations")]
    private void ClearDecorations()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.name.StartsWith("Decoration"))
            {
#if UNITY_EDITOR
                DestroyImmediate(child);
#else
                Destroy(child);
#endif
            }
        }
    }

    private void GenerateDecorations()
    {
        if (grassSprites.Length == 0 && flowerSprites.Length == 0) return;

        List<Vector2> points = PoissonDiskSampling();

        foreach (Vector2 point in points)
        {
            // raycast down to find surface height
            Vector3 worldPoint = transform.TransformPoint(new Vector3(point.x, 10f, point.y));

            // ignore water layer
            int layerMask = ~LayerMask.GetMask("Water");

            if (Physics.Raycast(worldPoint, Vector3.down, out RaycastHit hit, 100f, layerMask))
            {
                if (hit.collider.CompareTag("Hexagon"))
                {
                    Vector3 localPos = transform.InverseTransformPoint(hit.point);
                    CreateDecoration(localPos);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dart"))
        {
            ContactPoint contact = GetBestContactPoint(collision);

            Vector3 groundNormal = contact.normal;

            // if on the side of a ground block, respawn the bee
            if (IsVerticalCollision(groundNormal))
            {
                // complicated and terribly coded handliing of a dart bouncing off of the ground layer
                Dart dart = collision.gameObject.GetComponent<Dart>();
                dart.ChangeDartState(Dart.DartStates.THROWN);

                Vector3 dartLastVelocity = dart.lastVelocity;

                dart.HandleGroundSideCollision();

                Rigidbody dartBody = collision.gameObject.GetComponent<Rigidbody>();
                dartBody.isKinematic = false;

                float forwardSpeed = Vector3.Dot(dartLastVelocity, collision.gameObject.transform.forward);
                // the bounced vector of the object
                Vector3 reflectedVel = Vector3.Reflect(collision.gameObject.transform.forward, collision.contacts[0].normal);
                // adding current speed to the bounced vector

                reflectedVel *= forwardSpeed;
                reflectedVel *= bounceForce;

                dartBody.linearVelocity = reflectedVel;
            }
        }
    }

    // aswesome poisson sampling method on the internet for better distribution patterns.
    private List<Vector2> PoissonDiskSampling()
    {
        List<Vector2> points = new List<Vector2>();
        List<Vector2> activeList = new List<Vector2>();

        // start with random point in hex
        Vector2 firstPoint = GetRandomPointInHex();
        points.Add(firstPoint);
        activeList.Add(firstPoint);

        while (activeList.Count > 0)
        {
            int randomIndex = Random.Range(0, activeList.Count);
            Vector2 currentPoint = activeList[randomIndex];
            bool found = false;

            // try to find valid point around current point
            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 newPoint = GetRandomPointAround(currentPoint);

                if (IsValidPoint(newPoint, points))
                {
                    points.Add(newPoint);
                    activeList.Add(newPoint);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                activeList.RemoveAt(randomIndex);
            }
        }

        return points;
    }

    private Vector2 GetRandomPointInHex()
    {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float distance = Random.Range(0f, hexRadius * 0.8f);
        return new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
    }

    private Vector2 GetRandomPointAround(Vector2 center)
    {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float distance = Random.Range(minDistance, minDistance * 2f);
        return center + new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
    }

    private bool IsValidPoint(Vector2 point, List<Vector2> existingPoints)
    {
        // check if point is inside hex bounds
        if (Vector2.Distance(Vector2.zero, point) > hexRadius * 0.8f)
            return false;

        // check distance to existing points
        foreach (Vector2 existingPoint in existingPoints)
        {
            if (Vector2.Distance(point, existingPoint) < minDistance)
                return false;
        }

        return true;
    }

    private void CreateDecoration(Vector3 localPosition)
    {
        GameObject decoration = new GameObject("Decoration");
        decoration.transform.SetParent(transform);
        decoration.transform.localPosition = localPosition;

        SpriteRenderer renderer = decoration.AddComponent<SpriteRenderer>();

        // choose sprite type
        if (Random.Range(0f, 1f) < grassChance && grassSprites.Length > 0)
        {
            renderer.sprite = grassSprites[Random.Range(0, grassSprites.Length)];
        }
        else if (flowerSprites.Length > 0)
        {
            renderer.sprite = flowerSprites[Random.Range(0, flowerSprites.Length)];
        }

        // offset sprite up by half its height
        if (renderer.sprite != null)
        {
            float spriteHeight = renderer.sprite.bounds.size.y;
            decoration.transform.localPosition += Vector3.up * (spriteHeight * 0.5f);
        }

        // random rotation and scale
        decoration.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        float scale = Random.Range(minScale, maxScale);
        decoration.transform.localScale = new Vector3(scale, scale, scale);

        // sorting layer settings
        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = sortingOrder;
    }

    // making sure contact point is the good one
    private ContactPoint GetBestContactPoint(Collision coll)
    {
        ContactPoint bestContact = coll.contacts[0];

        if (coll.contacts.Length > 1)
        {
            float bestDistance = Vector3.Distance(bestContact.point, transform.position);

            for (int i = 1; i < coll.contacts.Length; i++)
            {
                float distance = Vector3.Distance(coll.contacts[i].point, transform.position);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestContact = coll.contacts[i];
                }
            }
        }

        return bestContact;
    }

    private bool IsVerticalCollision(Vector3 normal)
    {
        return Mathf.Abs(normal.y) < verticalThreshold;
    }

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