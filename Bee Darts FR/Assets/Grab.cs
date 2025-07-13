using UnityEngine;

public class Grab : MonoBehaviour
{
    [SerializeField] float grabLength;
    [SerializeField] LayerMask layermask;
    [SerializeField] DartThrowing dartThrowingRef;
    [SerializeField] Color defaultReticleColor;
    [SerializeField] Gradient hoverGradient;
    float gradientTime;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dartThrowingRef.currentDart == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, grabLength, layermask))
            {
                hit.transform.TryGetComponent(out Dart dart);
                if (dart != null)
                {
                    GameUIManager.instance.ChangeReticleColor(hoverGradient.Evaluate(gradientTime));
                    if (Input.GetMouseButtonDown(0))
                    {
                        dartThrowingRef.Pickup(dart);
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
