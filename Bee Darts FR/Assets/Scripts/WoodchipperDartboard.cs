using DG.Tweening;
using UnityEngine;

public class WoodchipperDartboard : Dartboard
{
    [Header("Woodchipper Effects")]
    [SerializeField] private ParticleSystem explosionParticlePrefab;
    [SerializeField] private Vector3 explosionOffset = Vector3.up * 2f;
    [SerializeField] private float explosionDelay = 1.5f;
    [SerializeField] private float dartRespawnDelay = 0.2f;

    [Header("Shake Animation")]
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeDuration = 2f;
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private float shakeRandomness = 90f;

    // state management
    private bool isProcessingDart;
    private Transform cachedTransform;
    private BoxCollider boxCollider;

    protected override void Awake()
    {
        base.Awake();

        // cache references
        cachedTransform = transform;
        boxCollider = GetComponent<BoxCollider>();
    }

    // handle dart hit - woodchipper destroys the dart
    protected override void OnHit(Dart dart)
    {
        // prevent multiple darts being processed simultaneously
        if (isProcessingDart) return;

        isProcessingDart = true;
        ProcessDartDestruction(dart);
    }

    // process dart destruction sequence
    private void ProcessDartDestruction(Dart dart)
    {
        // hide dart immediately
        dart.gameObject.SetActive(false);

        // start shake animation
        cachedTransform
            .DOShakePosition(shakeDuration, shakeIntensity, shakeVibrato, shakeRandomness, false, false)
            .OnComplete(() => TriggerExplosion(dart));
    }

    // trigger explosion effect
    private void TriggerExplosion(Dart dart)
    {
        // calculate timing
        float remainingDelay = Mathf.Max(0, explosionDelay - shakeDuration);

        // delay explosion for comedic effect
        DOVirtual.DelayedCall(remainingDelay, () =>
        {
            // spawn explosion particle
            SpawnExplosionEffect();

            // play sound effect
            PlayAttachedSound();

            // destroy woodchipper blade visual
            DestroyBladeVisual();

            // disable collision
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }

            // bring back bee at explosion location
            DOVirtual.DelayedCall(dartRespawnDelay, () =>
            {
                dart.transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + 1f, transform.parent.position.z);
                dart.gameObject.SetActive(true);
                dart.GetComponent<Rigidbody>().isKinematic = false;
                dart.GetComponent<Dart>().ChangeState(Dart.DartState.THROWN);
            });

            // reset processing flag
            isProcessingDart = false;
        });
    }

    // spawn explosion particle effect
    private void SpawnExplosionEffect()
    {
        if (explosionParticlePrefab == null) return;

        Vector3 explosionPosition = cachedTransform.position + explosionOffset;
        ParticleSystem explosion = Instantiate(explosionParticlePrefab, explosionPosition, Quaternion.identity);

        // auto-destroy particle system
        float duration = explosion.main.duration + explosion.main.startLifetime.constantMax;
        Destroy(explosion.gameObject, duration);
    }

    // destroy woodchipper blade visual
    private void DestroyBladeVisual()
    {
        if (cachedTransform.childCount > 0)
        {
            GameObject blade = cachedTransform.GetChild(0).gameObject;
            Destroy(blade);
        }
    }

    // cleanup
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // kill any active tweens
        DOTween.Kill(cachedTransform);
    }
}