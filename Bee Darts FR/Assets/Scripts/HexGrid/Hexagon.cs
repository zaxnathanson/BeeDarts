using UnityEngine;
using UnityEngine.Rendering;


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