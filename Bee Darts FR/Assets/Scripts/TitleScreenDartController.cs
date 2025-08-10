using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TitleScreenDartController : MonoBehaviour
{
    [Header("Dart Settings")]

    [SerializeField] private GameObject dartPrefab;

    [SerializeField] private float dartSpeed = 50f;
    [SerializeField] private float dartLifetime = 5f;
    [SerializeField] private float cameraDistance = 1.25f;

    [SerializeField] public float fireDelay = 0.5f;
    [SerializeField] public int maxDarts = 50;

    [Header("Reticle Settings")]

    [SerializeField] private Image reticle;
    [SerializeField] private float reticleDistance = 30f;
    [SerializeField] private LayerMask aimLayerMask = -1;

    private Camera mainCamera;
    private float lastFireTime;
    private bool canFire = true;
    private List<GameObject> activeDarts = new List<GameObject>();
    private Vector3 currentAimPosition;

    private GameUIManager uiManager;

    private void Start()
    {
        mainCamera = Camera.main;

        uiManager = GameUIManager.Instance;

        if (uiManager == null)
        {
            Debug.LogWarning("GameUIManager not found");
        }
    }

    private void Update()
    {
        UpdateAiming();
        HandleInput();
        CleanupOldDarts();

        reticle.transform.position = Input.mousePosition;
    }

    private void UpdateAiming()
    {
        // mouse to world
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, aimLayerMask))
        {
            currentAimPosition = hit.point;
        }
        else
        {
            currentAimPosition = ray.GetPoint(reticleDistance);
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButton(0) && canFire && Time.time - lastFireTime >= fireDelay)
        {
            FireDart();
            lastFireTime = Time.time;
            StartCoroutine(FireCooldown());
        }
    }

    private void FireDart()
    {
        if (dartPrefab == null)
        {
            Debug.LogError("No dart prefab assigned!");
            return;
        }

        // limiting total darts out
        if (activeDarts.Count >= maxDarts)
        {
            if (activeDarts.Count > 0 && activeDarts[0] != null)
            {
                Destroy(activeDarts[0]);
                activeDarts.RemoveAt(0);
            }
        }

        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * cameraDistance;
        Vector3 direction = (currentAimPosition - spawnPosition).normalized;

        GameObject newDart = Instantiate(dartPrefab, spawnPosition, Quaternion.LookRotation(direction));
        Rigidbody dartRb = newDart.GetComponent<Rigidbody>();

        if (dartRb != null)
        {
            dartRb.isKinematic = false;

            dartRb.useGravity = false;
            dartRb.linearVelocity = direction * dartSpeed;

            dartRb.linearDamping = 0f;
            dartRb.angularDamping = 0f;
        }

        Dart dartComponent = newDart.GetComponent<Dart>();
        dartComponent.ChangeState(Dart.DartState.THROWN);

        activeDarts.Add(newDart);

        StartCoroutine(DestroyDartAfterDelay(newDart, dartLifetime));
    }

    private IEnumerator FireCooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(fireDelay);
        canFire = true;
    }

    private IEnumerator DestroyDartAfterDelay(GameObject dart, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dart != null)
        {
            activeDarts.Remove(dart);
            Destroy(dart);
        }
    }

    private void CleanupOldDarts()
    {
        // Remove any null references from the list
        activeDarts.RemoveAll(dart => dart == null);
    }

    private void OnDestroy()
    {
        // Clean up all active darts
        foreach (var dart in activeDarts)
        {
            if (dart != null)
            {
                Destroy(dart);
            }
        }
        activeDarts.Clear();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            // Draw aim position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentAimPosition, 0.5f);

            // Draw line from camera to aim position
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(mainCamera.transform.position, currentAimPosition);
        }
    }
}