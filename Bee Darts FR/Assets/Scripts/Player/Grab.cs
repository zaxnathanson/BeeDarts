using UnityEngine;

public class Grab : MonoBehaviour
{
    [Header("Grab Settings")]
    [SerializeField] private float grabRadius = 1f;
    [SerializeField] float grabLength;

    [Header("Others")]
    [SerializeField] private LayerMask dartableLayers;
    [SerializeField] private LayerMask ignoredLayers;
    [SerializeField] private DartThrowing dartThrowingRef;
    [SerializeField] private Color defaultReticleColor;
    [SerializeField] private Gradient hoverGradient;

    private float gradientTime;
    private LayerMask finalMask;

    private void Awake()
    {
        finalMask = dartableLayers & ~ignoredLayers;
    }

    void Update()
    {
        if (dartThrowingRef.currentDart == null && !dartThrowingRef.isGrabbing)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, grabLength, finalMask))
            {
                hit.transform.TryGetComponent(out Dart dart);
                if (dart != null && dart.currentDartState != Dart.DartStates.THROWN)
                {
                    GameUIManager.instance.ChangeReticleColor(hoverGradient.Evaluate(gradientTime));
                    if (Input.GetMouseButtonDown(1))
                    {
                        dartThrowingRef.isGrabbing = true;
                        StartCoroutine(dartThrowingRef.Pickup(dart));
                    }
                }
                else
                {
                    GameUIManager.instance.ChangeReticleColor(defaultReticleColor);
                }
            }
            else
            {
                GameUIManager.instance.ChangeReticleColor(defaultReticleColor);
            }
        }
        else
        {
            GameUIManager.instance.ChangeReticleColor(defaultReticleColor);
        }

        // Animate gradient time
        gradientTime += Time.deltaTime / 2;
        if (gradientTime > 1)
        {
            gradientTime -= 1;
        }
    }
}