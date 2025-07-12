using UnityEngine;

public class Grab : MonoBehaviour
{
    [SerializeField] float grabLength;
    [SerializeField] LayerMask layermask;
    [SerializeField] DartThrowing dartThrowingRef;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dartThrowingRef.currentDart == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, grabLength, layermask))
            {
                hit.transform.TryGetComponent(out Dart dart);
                if (dart != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        dartThrowingRef.Pickup(dart);
                    }
                }
                
            }
        }

    }
}
