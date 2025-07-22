using DG.Tweening;
using UnityEngine;
using System.Collections;

public class DartThrowing : MonoBehaviour
{
    [Header("Charge Settings")]
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float maxChargeForce = 30f;
    [SerializeField] private float minChargeForce = 5f;

    [Header("Charge Juice")]
    [SerializeField] private float minChargeShake = 0.01f;
    [SerializeField] private float maxChargeShake = 0.1f;
    [SerializeField] private int minChargeVibrato = 5;
    [SerializeField] private int maxChargeVibrato = 20;
    [SerializeField] private float pullbackAmount = 0.5f;

    [Header("Pickup Settings")]
    [SerializeField] private Transform dartHolderTransform;
    [SerializeField] private float dartPickupSpeed = 3f;

    // state management
    private Dart currentDart;
    private float currentChargeTime;
    private float currentChargeForce;
    private bool isGrabbing;
    private bool isCharging;

    // cached references
    private Tween vibrationTween;
    private Transform cachedTransform;
    private Camera mainCamera;

    // properties
    public Dart CurrentDart => currentDart;
    public bool IsGrabbing => isGrabbing;
    public bool HasDart => currentDart != null;

    // singleton pattern
    public static DartThrowing Instance { get; private set; }

    private void Awake()
    {
        // singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // cache references
        cachedTransform = transform;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // only process input when we have a throwable dart
        if (currentDart != null && currentDart.CanBeThrown)
        {
            HandleChargeInput();
        }
    }

    // handle charge input
    private void HandleChargeInput()
    {
        // start charging
        if (Input.GetMouseButtonDown(0))
        {
            StartCharging();
        }

        // continue charging
        if (isCharging && Input.GetMouseButton(0))
        {
            UpdateCharge();
            UpdateDartJuice();
        }

        // release dart
        if (isCharging && Input.GetMouseButtonUp(0))
        {
            LaunchDart();
        }
    }

    // start charging dart
    private void StartCharging()
    {
        isCharging = true;
        currentDart.ChangeState(Dart.DartState.CHARGING);
    }

    // update charge values
    private void UpdateCharge()
    {
        currentChargeTime = Mathf.Min(currentChargeTime + Time.deltaTime, maxChargeTime);
        float chargePercent = currentChargeTime / maxChargeTime;

        // calculate force
        currentChargeForce = Mathf.Lerp(minChargeForce, maxChargeForce, chargePercent);
    }

    // launch dart with current charge
    private void LaunchDart()
    {
        // cleanup vibration
        CleanupVibration();

        // store start position
        currentDart.ThrownStartPos = cachedTransform.position;

        // change state and fire
        currentDart.ChangeState(Dart.DartState.THROWN);
        currentDart.Fire(currentChargeForce, mainCamera.transform.forward);

        // release dart
        currentDart.transform.SetParent(null);
        currentDart = null;

        // reset charge
        currentChargeTime = 0f;
        currentChargeForce = minChargeForce;
        isCharging = false;
    }

    // update dart pullback position
    private void UpdateDartPullback(float chargePercent)
    {
        if (currentDart == null) return;

        Vector3 localPos = currentDart.transform.localPosition;
        localPos.z = -pullbackAmount * chargePercent;
        currentDart.transform.localPosition = localPos;
    }

    // update dart juice (pullback and shake)
    private void UpdateDartJuice()
    {
        if (currentDart == null) return;

        float chargePercent = currentChargeTime / maxChargeTime;

        // update pullback
        UpdateDartPullback(chargePercent);

        // create or update vibration
        if (vibrationTween == null || !vibrationTween.IsActive())
        {
            vibrationTween = CreateVibrationTween();
        }
    }

    // start vibration effect
    private void StartVibrationEffect()
    {
        vibrationTween = CreateVibrationTween();
    }

    // create vibration tween
    private Tween CreateVibrationTween()
    {
        if (currentDart == null) return null;

        float chargePercent = currentChargeTime / maxChargeTime;
        float shakeStrength = Mathf.Lerp(minChargeShake, maxChargeShake, chargePercent);
        int vibrato = Mathf.RoundToInt(Mathf.Lerp(minChargeVibrato, maxChargeVibrato, chargePercent));

        return currentDart.transform
            .DOShakePosition(0.1f, shakeStrength, vibrato, 90f, false, false)
            .OnComplete(() => vibrationTween = CreateVibrationTween());
    }

    // cleanup vibration tween
    private void CleanupVibration()
    {
        if (vibrationTween != null)
        {
            vibrationTween.Kill();
            vibrationTween = null;
        }
    }

    // pick up dart
    public void PickupDart(Dart dart)
    {
        if (isGrabbing || currentDart != null) return;

        isGrabbing = true;
        StartCoroutine(PickupCoroutine(dart));
    }

    // pickup animation coroutine
    private IEnumerator PickupCoroutine(Dart dart)
    {
        // calculate initial values
        Vector3 startPos = dart.transform.position;
        float startTime = Time.time;

        // animate dart to holder
        while (dart != null && dart.gameObject.activeSelf)
        {
            // calculate current target position (updates each frame)
            Vector3 targetPos = cachedTransform.position + Vector3.down * 0.5f;

            // calculate progress based on speed and distance
            float distanceCovered = (Time.time - startTime) * dartPickupSpeed;
            float totalDistance = Vector3.Distance(startPos, targetPos);
            float fractionOfJourney = distanceCovered / totalDistance;

            // check if we've reached the destination
            if (fractionOfJourney >= 1f)
            {
                dart.transform.position = targetPos;
                break;
            }

            // lerp to current target position
            dart.transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);

            yield return null;
        }

        // parent dart and reset rotation
        if (dart != null)
        {
            dart.transform.SetParent(dartHolderTransform);
            dart.transform.localRotation = Quaternion.identity;

            // update references
            currentDart = dart;
            currentDart.ChangeState(Dart.DartState.HELD);
        }

        // dart will handle final positioning
        isGrabbing = false;
    }

    private void OnDestroy()
    {
        // cleanup
        CleanupVibration();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}