using DG.Tweening;
using UnityEngine;

public class WoodchipperDartboard : Dartboard
{
    [Header("Woodchipper Effects")]

    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip woodchipperClip;

    [SerializeField] private ParticleSystem explosionParticlePrefab;

    [SerializeField] private Vector3 explosionOffset = Vector3.up * 2f;

    [Header("Shake Animation")]

    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeDuration = 2f;
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private float shakeRandomness = 90f;

    // state management
    private bool isProcessingDart;
    private BoxCollider boxCollider;

    protected override void Awake()
    {
        base.Awake();

        boxCollider = GetComponent<BoxCollider>();
    }

    protected override void OnHit(Dart dart)
    {
        // prevent multiple darts being processed simultaneously
        if (isProcessingDart) return;

        isProcessingDart = true;
        ProcessDartDestruction(dart);
    }

    private void ProcessDartDestruction(Dart dart)
    {
        dart.gameObject.SetActive(false);

        transform
            .DOShakePosition(shakeDuration, shakeIntensity, shakeVibrato, shakeRandomness, false, false)
            .OnComplete(() => TriggerExplosion(dart));
    }

    private void TriggerExplosion(Dart dart)
    {
        SpawnExplosionEffect();

        GameManager.Instance.PlaySFX(explosionClip, transform.position);

        DestroyBladeVisual();

        // disable collision
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        dart.transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + 1f, transform.parent.position.z);
        dart.gameObject.SetActive(true);

        dart.GetComponent<Rigidbody>().isKinematic = false;
        dart.GetComponent<Dart>().ChangeState(Dart.DartState.THROWN);

        isProcessingDart = false;
    }

    // spawn explosion particle effect
    private void SpawnExplosionEffect()
    {
        if (explosionParticlePrefab == null) return;

        Vector3 explosionPosition = transform.position + explosionOffset;
        ParticleSystem explosion = Instantiate(explosionParticlePrefab, explosionPosition, Quaternion.identity);

        // auto-destroy particle system
        float duration = explosion.main.duration + explosion.main.startLifetime.constantMax;
        Destroy(explosion.gameObject, duration);
    }

    private void DestroyBladeVisual()
    {
        if (transform.childCount > 0)
        {
            GameObject blade = transform.GetChild(0).gameObject;
            Destroy(blade);
        }
    }
}