using UnityEngine;

public class Grab : MonoBehaviour
{
    [Header("Grab Settings")]

    [SerializeField] private float grabRange = 5f;
    [SerializeField] private LayerMask dartableLayers;
    [SerializeField] private LayerMask ignoredLayers;

    [Header("Visual Settings")]

    [SerializeField] private Color defaultReticleColor = Color.white;
    [SerializeField] private Gradient hoverGradient;
    [SerializeField] private float gradientCycleTime = 2f;

    // cached references
    private Camera mainCamera;
    private LayerMask finalMask;
    private float gradientTime;
    private Dart hoveredDart;

    // raycast optimization
    private const float RAYCAST_INTERVAL = 0.1f;
    private float nextRaycastTime;

    private void Awake()
    {
        // cache references
        mainCamera = Camera.main;

        // calculate final layer mask
        finalMask = dartableLayers & ~ignoredLayers;
    }

    private void Update()
    {
        // update gradient time
        gradientTime = (gradientTime + Time.deltaTime / gradientCycleTime) % 1f;

        // check if we can grab
        if (!CanAttemptGrab())
        {
            ResetReticle();
            return;
        }

        // throttle raycasts for performance
        if (Time.time >= nextRaycastTime)
        {
            nextRaycastTime = Time.time + RAYCAST_INTERVAL;
            CheckForGrabbableDart();
        }

        // handle grab input
        if (hoveredDart != null && Input.GetMouseButtonDown(1))
        {
            AttemptGrab();
        }
    }

    // check if we can attempt to grab
    private bool CanAttemptGrab()
    {
        return DartThrowing.Instance != null &&
               !DartThrowing.Instance.HasDart &&
               !DartThrowing.Instance.IsGrabbing;
    }

    // check for grabbable dart
    private void CheckForGrabbableDart()
    {
        // perform raycast
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, grabRange, finalMask))
        {
            // check if hit object has dart component
            Dart dart = hit.transform.GetComponent<Dart>();

            if (dart != null && IsGrabbable(dart))
            {
                SetHoveredDart(dart);
            }
            else
            {
                ClearHoveredDart();
            }
        }
        else
        {
            ClearHoveredDart();
        }
    }

    // check if dart is grabbable
    private bool IsGrabbable(Dart dart)
    {
        return dart.CurrentState != Dart.DartState.THROWN &&
               dart.CurrentState != Dart.DartState.HELD;
    }

    // set hovered dart
    private void SetHoveredDart(Dart dart)
    {
        if (hoveredDart != dart)
        {
            hoveredDart = dart;
        }

        // update reticle color
        UpdateReticleColor(hoverGradient.Evaluate(gradientTime));
    }

    // clear hovered dart
    private void ClearHoveredDart()
    {
        if (hoveredDart != null)
        {
            hoveredDart = null;
            ResetReticle();
        }
    }

    // attempt to grab hovered dart
    private void AttemptGrab()
    {
        if (hoveredDart != null && DartThrowing.Instance != null)
        {
            DartThrowing.Instance.PickupDart(hoveredDart);
            hoveredDart = null;
            ResetReticle();
        }
    }

    // reset reticle to default color
    private void ResetReticle()
    {
        UpdateReticleColor(defaultReticleColor);
    }

    // update reticle color
    private void UpdateReticleColor(Color color)
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ChangeReticleColor(color);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // visualize grab range
        if (mainCamera == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * grabRange);
    }
}