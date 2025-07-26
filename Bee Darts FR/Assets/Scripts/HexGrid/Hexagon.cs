using UnityEngine;
using System.Collections.Generic;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Hexagon : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float hexRadius = 2f;

    [Header("Snapping")]
    [SerializeField] private bool snapToGrid = true;

    [Header("Side Settings")]
    [SerializeField] private float bounceForce;
    [SerializeField] private float verticalThreshold;

    [Header("Decoration Settings")]
    [SerializeField] private bool doGenerateDecorations;

    [Header("Decoration Prefabs")]
    public GameObject[] grassPrefabs;
    public GameObject[] flowerPrefabs;

    [Header("Distribution Settings")]
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
        if (Application.isPlaying && doGenerateDecorations)
        {
            StartCoroutine(DelayedGeneration());
        }
    }

    IEnumerator DelayedGeneration()
    {
        yield return new WaitForSecondsRealtime(0.05f);
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
        // no prefabs no party
        if (grassPrefabs.Length == 0 && flowerPrefabs.Length == 0) return;

        List<Vector2> points = PoissonDiskSampling();
        int layerMask = ~LayerMask.GetMask("Water");

        foreach (Vector2 point in points)
        {
            // raycast from above
            Vector3 worldPoint = transform.TransformPoint(new Vector3(point.x, 10f, point.y));

            if (Physics.Raycast(worldPoint, Vector3.down, out RaycastHit hit, 100f, layerMask))
            {
                if (hit.collider.CompareTag("Hexagon"))
                {
                    CreateDecoration(hit.point);
                }
            }
        }
    }

    private void CreateDecoration(Vector3 worldPosition)
    {
        GameObject decoration = null;
        bool isGrass = Random.Range(0f, 1f) < grassChance;

        // pick prefab
        if (isGrass && grassPrefabs.Length > 0)
        {
            decoration = Instantiate(grassPrefabs[Random.Range(0, grassPrefabs.Length)]);
        }
        else if (!isGrass && flowerPrefabs.Length > 0)
        {
            decoration = Instantiate(flowerPrefabs[Random.Range(0, flowerPrefabs.Length)]);
        }

        if (decoration == null) return;

        // setup decoration
        decoration.name = "Decoration";
        decoration.transform.SetParent(transform);
        decoration.transform.position = worldPosition;

        // apply scale
        float scale = Random.Range(minScale, maxScale);
        decoration.transform.localScale = Vector3.one * scale;

        // get renderer bounds after everything is set
        SpriteRenderer renderer = decoration.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // use bounds.extents.y for half height from pivot
            float halfHeight = renderer.bounds.extents.y;
            decoration.transform.position = worldPosition + Vector3.up * halfHeight;

            // sorting settings
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = sortingOrder + Random.Range(-1, 2); // slight variation to avoid overlap
        }

        // random rotation
        decoration.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dart"))
        {
            ContactPoint contact = GetBestContactPoint(collision);

            if (IsVerticalCollision(contact.normal))
            {
                // bounce the dart
                Dart dart = collision.gameObject.GetComponent<Dart>();
                dart.ChangeState(Dart.DartState.THROWN);

                Vector3 dartLastVelocity = dart.lastVelocity;
                dart.HandleGroundSideCollision();

                Rigidbody dartBody = collision.gameObject.GetComponent<Rigidbody>();
                dartBody.isKinematic = false;

                float forwardSpeed = Vector3.Dot(dartLastVelocity, collision.gameObject.transform.forward);
                Vector3 reflectedVel = Vector3.Reflect(collision.gameObject.transform.forward, contact.normal);

                dartBody.linearVelocity = reflectedVel * forwardSpeed * bounceForce;
            }
        }
    }

    // poisson disk sampling for nice distribution
    private List<Vector2> PoissonDiskSampling()
    {
        List<Vector2> points = new List<Vector2>();
        List<Vector2> activeList = new List<Vector2>();

        // start in center
        Vector2 firstPoint = Vector2.zero;
        points.Add(firstPoint);
        activeList.Add(firstPoint);

        while (activeList.Count > 0)
        {
            int randomIndex = Random.Range(0, activeList.Count);
            Vector2 currentPoint = activeList[randomIndex];
            bool found = false;

            // try to find valid neighbor
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

    private Vector2 GetRandomPointAround(Vector2 center)
    {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float distance = Random.Range(minDistance, minDistance * 2f);
        return center + new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
    }

    private bool IsValidPoint(Vector2 point, List<Vector2> existingPoints)
    {
        // inside hex bounds
        if (point.magnitude > hexRadius * 0.8f)
            return false;

        // check distance to others
        foreach (Vector2 existingPoint in existingPoints)
        {
            if (Vector2.Distance(point, existingPoint) < minDistance)
                return false;
        }

        return true;
    }

    // get closest contact point
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
        Hex hexCoords = WorldToHex(currentPos.x, currentPos.z, hexRadius);
        Vector3 snappedPos = HexToWorld(hexCoords.q, hexCoords.r, hexRadius);
        snappedPos.y = currentPos.y;
        transform.position = snappedPos;
    }

    // world to hex coords
    private Hex WorldToHex(float x, float z, float hexRadius)
    {
        float q = (2f / 3f * x) / hexRadius;
        float r = (-1f / 3f * x + Mathf.Sqrt(3f) / 3f * z) / hexRadius;
        return HexRound(q, r);
    }

    // hex to world coords
    private Vector3 HexToWorld(int q, int r, float hexRadius)
    {
        float x = hexRadius * 3f / 2f * q;
        float y = 0;
        float z = hexRadius * Mathf.Sqrt(3f) * (r + q / 2f);
        return new Vector3(x, y, z);
    }

    // round hex coordinates
    private Hex HexRound(float q, float r)
    {
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

    [ContextMenu("Regenerate Decorations")]
    public void RegenerateDecorations()
    {
        ClearDecorations();
        if (Application.isPlaying)
        {
            StartCoroutine(DelayedGeneration());
        }
    }
}