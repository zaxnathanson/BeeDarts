using System.Collections.Generic;
using UnityEngine;

public class TestRange : MonoBehaviour
{
    [Header("Line Renderer Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private int segments = 50;
    [SerializeField] private float width = 0.05f;
    [SerializeField] private float offsetOffGround = 0.05f;
    [SerializeField] private bool isRefreshing = true;

    [Header("References")]

    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] Material testMat;

    private LineRenderer lineRenderer;

    private void Start()
    {
        if (TryGetComponent<LineRenderer>(out LineRenderer lineR))
        {
            lineRenderer = lineR;
            lineRenderer.loop = true;
            lineRenderer.widthMultiplier = width;
            lineRenderer.material = testMat;
        }
        else
        {
            Debug.LogWarning("No line renderer bruh");
        }

        DrawRange();
    }

    private void Update()
    {
        if (isRefreshing)
        {
            DrawRange();
        }
    }

    private void DrawRange()
    {
        List<Vector3> validPoints = new List<Vector3>();

        for (int i = 0; i < segments; i++)
        {
            // jus calculating a circle
            float angle = 2 * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 worldPos = transform.position + new Vector3(x, 0, z);

            // raycast downward above to draw circle on ground
            if (Physics.Raycast(worldPos + Vector3.up * 10, Vector3.down, out RaycastHit hit, 20f, groundLayerMask))
            {
                validPoints.Add(hit.point + Vector3.up * offsetOffGround); // slight offset above ground
            }
        }

        lineRenderer.positionCount = validPoints.Count;
        lineRenderer.SetPositions(validPoints.ToArray());
    }
}
