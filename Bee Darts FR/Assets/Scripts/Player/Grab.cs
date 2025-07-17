using UnityEngine;

public class Grab : MonoBehaviour
{
    [Header("Grab Settings")]

    [SerializeField] private float grabRadius = 1f;
    [SerializeField] float grabLength;

    [Header("Others")]

    [SerializeField] LayerMask layermask;
    [SerializeField] DartThrowing dartThrowingRef;
    [SerializeField] Color defaultReticleColor;
    [SerializeField] Gradient hoverGradient;
    float gradientTime;

    [SerializeField] private LayerMask ignoreLayer;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dartThrowingRef.currentDart == null && !dartThrowingRef.isGrabbing)
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, layermask))
            {
                //Collider[] colls = Physics.OverlapSphere(hit.transform.position, ) grabbbbbbbbbbbbbbbbb

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

        gradientTime += Time.deltaTime/2;
        if (gradientTime > 1)
        {
            gradientTime -= 1;
        }
    }
}
