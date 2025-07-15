using UnityEngine;

public class SpinChipperBlades : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
