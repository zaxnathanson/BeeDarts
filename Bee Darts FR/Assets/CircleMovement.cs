using UnityEngine;

public class CircleMovement : MonoBehaviour
{
    [Header("Circle Variables")]

    [SerializeField] private float speed = 2f;
    [SerializeField] private float radius = 3f;

    private Vector3 centerPosition;
    private float angle = 0f;

    private void Start()
    {
        centerPosition = transform.position;
    }

    private void Update()
    {
        if (transform.position.y > BeeManager.Instance.waterLevel)
        {
            Circle();
        }
    }

    private void Circle()
    {
        // increment angle based on speed and time
        angle += speed * Time.deltaTime;

        // calculate new position on circle
        float y = centerPosition.y + Mathf.Cos(angle) * radius;
        float z = centerPosition.z + Mathf.Sin(angle) * radius;

        transform.position = new Vector3(centerPosition.x, y, z);
    }
}
