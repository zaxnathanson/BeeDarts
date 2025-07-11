using DG.Tweening;
using UnityEngine;

public class DartThrowing : MonoBehaviour
{

    [Header("Charge")]
    [SerializeField] float maxChargeTime;
    [SerializeField] float maxChargeForce;
    [SerializeField] float minChargeForce;

    float currentChargeTime;
    float currentChargeForce;

    [Header("Charge Juice")]
    [SerializeField] float minChargeShake;
    [SerializeField] float maxChargeShake;
    [SerializeField] int minChargeVibrato;
    [SerializeField] int maxChargeVibrato;
    [SerializeField] float pullbackAmount;
    Vector3 startPos;
    Tween vibrationTween;
    public Dart currentDart;

    [Header("Pickup")]
    [SerializeField] Transform dartHolderTransform;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentDart != null)
        {
            if (currentDart.canBeThrown)
            {
                Charge();
                DartJuice();
            }
        }
    }

    void Charge()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentDart.ChangeDartState(Dart.DartStates.CHARGING);
        }
        if (Input.GetMouseButton(0))
        {
            currentChargeTime += Time.deltaTime;
            currentChargeForce = Mathf.Lerp(minChargeForce, maxChargeForce, currentChargeTime / maxChargeTime);
        }
        if (Input.GetMouseButtonUp(0))
        {
            LaunchDart();
            currentChargeTime = 0;
        }
    }

    void LaunchDart()
    {
        vibrationTween.Complete();
        vibrationTween = null;

        currentDart.ChangeDartState(Dart.DartStates.THROWN);
        currentDart.Fire(currentChargeForce);
        currentDart.transform.parent = null;
        currentDart = null;
    }

    void DartJuice()
    {
        if (currentDart != null)
        {

            vibrationTween ??= CreateVibrationTween();
            vibrationTween.OnComplete(() => vibrationTween = CreateVibrationTween());

            float zPos = Mathf.Lerp(0, -pullbackAmount, currentChargeTime / maxChargeTime);
            currentDart.transform.localPosition = new Vector3(currentDart.transform.localPosition.x, currentDart.transform.localPosition.y, zPos);
        }
    }

    Tween CreateVibrationTween()
    {
        float chargeShake = Mathf.Lerp(minChargeShake, maxChargeShake, currentChargeTime / maxChargeTime);
        int chargeVibration = (int)Mathf.Lerp(minChargeVibrato, maxChargeVibrato, currentChargeTime / maxChargeTime);
        return currentDart.transform.DOShakePosition(0.1f, chargeShake, chargeVibration, 90, false, false);
    }

    public void Pickup(Dart dartPickup)
    {
        dartPickup.transform.parent = dartHolderTransform;
        dartPickup.transform.localPosition = Vector3.zero;
        dartPickup.transform.localRotation = Quaternion.identity;
        currentDart = dartPickup;
        dartPickup.ChangeDartState(Dart.DartStates.HELD);
    }
}
