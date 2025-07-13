using UnityEngine;

public class ChildMaker : MonoBehaviour
{
    [SerializeField] private string targetTag = "Hexagon";
    [SerializeField] private float searchRadius = 10f;

    private void Start()
    {
        ParentToClosest();
    }

    private void ParentToClosest()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, searchRadius);

        Transform closest = null;
        float closestDistance = 999999;

        foreach (var collider in nearby)
        {
            if (collider.CompareTag(targetTag))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = collider.transform;
                }
            }
        }

        if (closest != null)
        {
            transform.SetParent(closest);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " could not find a parent in radius");
        }
    }
}
