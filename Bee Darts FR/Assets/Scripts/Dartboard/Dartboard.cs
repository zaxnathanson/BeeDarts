using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Dartboard : MonoBehaviour
{
    [Header("Hit Behavior")]

    [SerializeField] private bool destroyOnHit;
    [SerializeField] private GameObject targetParent; // renamed from parent
    [SerializeField] private float minimumThrowDistance = 2f;

    [Header("Hex Grid Settings")]

    [SerializeField] private List<GameObject> affectedHexagons = new List<GameObject>();

    [Header("Visual Feedback")]

    [SerializeField] private SpriteRenderer tooCloseIndicator;
    [SerializeField] private ParticleSystem hitParticlePrefab;

    [Header("Audio Settings")]

    [SerializeField] private AudioClip hitSound;
    [SerializeField, Range(0f, 1f)] private float hitSoundVolume = 1f;

    [Header("Gizmo Settings")]

    [SerializeField] private float gizmoSize = 0.25f;
    [SerializeField] private Color gizmoColorUnselected = new Color(1, 1, 1, 0.9f);
    [SerializeField] private Color gizmoColorSelected = new Color(1, 0, 0, 1);

    // component references
    private AudioSource audioSource;
    private Transform cachedTransform;

    // dart tracking
    private readonly List<Dart> attachedDarts = new List<Dart>();
    private Transform playerTransform;

    // optimization
    private float nextRangeCheckTime;
    private const float RANGE_CHECK_INTERVAL = 0.1f; // check 10 times per second

    // properties
    public int AttachedDartCount => attachedDarts.Count;
    public bool HasAttachedDarts => attachedDarts.Count > 0;

    // events
    public System.Action<Dart> OnDartAttached;
    public System.Action<Dart> OnDartDetached;
    public System.Action<Dartboard> OnDestroyed;

    protected virtual void Awake()
    {
        // cache references
        cachedTransform = transform;

        // find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] No player object found with 'Player' tag!");
        }

        // setup audio
        SetupAudioSource();

        // validate references
        ValidateReferences();
    }

    protected virtual void Update()
    {
        // throttle range detection
        if (Time.time >= nextRangeCheckTime)
        {
            nextRangeCheckTime = Time.time + RANGE_CHECK_INTERVAL;
            UpdateRangeIndicator();
        }

        // call virtual method for derived classes
        if (HasAttachedDarts)
        {
            OnDartsAttached(attachedDarts.Count);
        }
    }

    // setup audio source component
    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    // validate required references
    private void ValidateReferences()
    {
        if (targetParent == null)
        {
            targetParent = gameObject;
        }

        if (destroyOnHit && targetParent == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Destroy on hit is enabled but no target parent is set!");
        }
    }

    // update range indicator visibility
    private void UpdateRangeIndicator()
    {
        if (tooCloseIndicator == null || playerTransform == null) return;

        bool shouldShowIndicator = false;

        // only show if player has a dart
        if (DartThrowing.Instance != null && DartThrowing.Instance.HasDart)
        {
            float distanceToPlayer = Vector3.Distance(playerTransform.position, cachedTransform.position);
            shouldShowIndicator = distanceToPlayer <= minimumThrowDistance;
        }

        // always hide if no dart
        if (!shouldShowIndicator)
        {
            tooCloseIndicator.enabled = false;
        }
        else
        {
            tooCloseIndicator.enabled = true;
        }
    }

    // check if dart hit is valid
    public void CheckHit(Dart dart)
    {
        if (dart == null) return;

        // check minimum throw distance
        float throwDistance = Vector3.Distance(dart.ThrownStartPos, cachedTransform.position);
        if (throwDistance > minimumThrowDistance)
        {
            ProcessValidHit(dart);
        }
        else
        {
            OnInvalidHit(dart, throwDistance);
        }
    }

    // process valid hit
    private void ProcessValidHit(Dart dart)
    {
        // play effects
        PlayHitEffects();

        // handle hit behavior
        if (destroyOnHit)
        {
            HandleDestroyOnHit();
        }
        else
        {
            AttachDart(dart);
        }

        // call virtual method for derived classes
        OnHit(dart);
    }

    // play hit effects (particle and sound)
    private void PlayHitEffects()
    {
        // spawn particle effect
        if (hitParticlePrefab != null)
        {
            ParticleSystem particles = Instantiate(hitParticlePrefab, cachedTransform.position, Quaternion.identity);

            // auto-destroy particle system when finished
            float duration = particles.main.duration + particles.main.startLifetime.constantMax;
            Destroy(particles.gameObject, duration);
        }

        // play sound (skip for woodchipper tag)
        if (hitSound != null && audioSource != null && !gameObject.CompareTag("Woodchipper"))
        {
            audioSource.PlayOneShot(hitSound, hitSoundVolume);
        }
    }

    // handle destroy on hit behavior
    private void HandleDestroyOnHit()
    {
        OnDestroyed?.Invoke(this);
        Destroy(targetParent);
    }

    // attach dart to this dartboard
    private void AttachDart(Dart dart)
    {
        // subscribe to dart pickup event
        dart.OnPickedUp += HandleDartPickedUp;

        // add to list
        attachedDarts.Add(dart);

        // raise hexagons if configured
        if (affectedHexagons.Count > 0 && HexManager.Instance != null)
        {
            HexManager.Instance.LiftHexagonsInList(affectedHexagons);
        }

        // notify listeners
        OnDartAttached?.Invoke(dart);
    }

    // handle dart being picked up
    private void HandleDartPickedUp(Dart dart)
    {
        // unsubscribe from event
        dart.OnPickedUp -= HandleDartPickedUp;

        // remove from list
        attachedDarts.Remove(dart);

        // notify listeners
        OnDartDetached?.Invoke(dart);
    }

    // play sound for attached darts
    public void PlayAttachedSound()
    {
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, hitSoundVolume);
        }
    }

    // virtual methods for derived classes
    protected virtual void OnHit(Dart dart)
    {
        // override in derived classes for custom hit behavior
    }

    protected virtual void OnInvalidHit(Dart dart, float throwDistance)
    {
        // override in derived classes to handle too-close hits
        Debug.Log($"[{gameObject.name}] Hit too close! Distance: {throwDistance:F2}, Required: {minimumThrowDistance:F2}");
    }

    protected virtual void OnDartsAttached(int dartCount)
    {
        // override in derived classes for behavior while darts are attached
    }

    // cleanup
    protected virtual void OnDestroy()
    {
        // unsubscribe from all dart events
        foreach (var dart in attachedDarts)
        {
            if (dart != null)
            {
                dart.OnPickedUp -= HandleDartPickedUp;
            }
        }

        attachedDarts.Clear();
    }

    #region Context Menu Functions

    [ContextMenu("Clear Hexagon List")]
    private void ClearHexagonList()
    {
        affectedHexagons.Clear();
    }

    [ContextMenu("Lower Affected Hexagons")]
    private void LowerAffectedHexagons()
    {
        if (HexManager.Instance != null && affectedHexagons.Count > 0)
        {
            HexManager.Instance.LowerHexagonsInList(affectedHexagons);
        }
    }

    [ContextMenu("Raise Affected Hexagons")]
    private void RaiseAffectedHexagons()
    {
        if (HexManager.Instance != null && affectedHexagons.Count > 0)
        {
            HexManager.Instance.LiftHexagonsInList(affectedHexagons);
        }
    }

    [ContextMenu("Debug - Log Attached Darts")]
    private void DebugLogAttachedDarts()
    {
        Debug.Log($"[{gameObject.name}] Attached darts: {attachedDarts.Count}");
        for (int i = 0; i < attachedDarts.Count; i++)
        {
            if (attachedDarts[i] != null)
            {
                Debug.Log($"  - Dart {i}: {attachedDarts[i].name}");
            }
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        // draw hexagon indicators
        DrawHexagonGizmos(gizmoColorUnselected);
    }

    private void OnDrawGizmosSelected()
    {
        // draw minimum throw distance sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minimumThrowDistance);

        // draw hexagon indicators in selected color
        DrawHexagonGizmos(gizmoColorSelected);
    }

    private void DrawHexagonGizmos(Color color)
    {
        if (affectedHexagons == null || affectedHexagons.Count == 0) return;

        Gizmos.color = color;

        foreach (GameObject hex in affectedHexagons)
        {
            if (hex != null)
            {
                // calculate gizmo position above hexagon
                Vector3 hexPos = hex.transform.position;
                float hexHeight = hex.transform.localScale.y * 1.25f; // simplified calculation
                Vector3 gizmoPos = new Vector3(hexPos.x, hexPos.y + hexHeight, hexPos.z);

                Gizmos.DrawSphere(gizmoPos, gizmoSize);
            }
        }
    }

    #endregion
}