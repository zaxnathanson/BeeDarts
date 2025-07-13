using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] bool doChangeSize;
    [SerializeField] float size;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        if (doChangeSize)
        {
            transform.localScale = size * Vector3.one * Vector3.Distance(transform.position, Camera.main.transform.position);
        }
    }
}
