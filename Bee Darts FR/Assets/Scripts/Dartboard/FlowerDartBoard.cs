using UnityEngine;

public class FlowerDartboard : Dartboard
{
    [Header("Dartboard Settings")]

    [SerializeField] private AudioClip invalidHit;

    [Header("Gameplay Settings")]

    [Tooltip("Set true for the first dartboard in the level to control bee spawn points")]
    [SerializeField] private bool isFirstFlower;

    [Header("Outline Settings")]

    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float tooCloseScale = 1.5f;
    private Material outlineMaterialInstance;
    private Renderer outlineRenderer;

    // track if this flower has been hit for points
    private bool hasAwardedPoints;

    private CircleMovement circlingScript;
    private MovingWall movingScript;

    private void Start()
    {
        FindOutlineMaterial();
    }

    protected override void Update()
    {
        base.Update();

        if (base.isShowingTooClose)
        {
            // material edit here
            if (outlineMaterialInstance != null)
            {
                outlineMaterialInstance.SetFloat("_Scale", tooCloseScale);
            }
        }
        else
        {
            if (outlineMaterialInstance != null)
            {
                outlineMaterialInstance.SetFloat("_Scale", normalScale);
            }
        }
    }
    protected override void OnHit(Dart dart)
    {
        base.OnHit(dart);

        // notify bee manager if this is the first flower
        if (isFirstFlower && BeeManager.Instance != null)
        {
            BeeManager.Instance.firstFlower = true;
        }

        // award points on first hit only
        if (!hasAwardedPoints && BeeManager.Instance != null)
        {
            BeeManager.Instance.IncrementPoints(1);
            hasAwardedPoints = true;
        }

        if ((circlingScript = transform.parent.GetComponent<CircleMovement>()) != null)
        {
            circlingScript.StopCircling();
        }

        if ((movingScript = transform.parent.GetComponent<MovingWall>()) != null)
        {
            movingScript.StopMoving();
        }
    }

    protected override void OnInvalidHit(Dart dart, float throwDistance)
    {
        base.OnInvalidHit(dart, throwDistance);

        //GameManager
    }

    private void FindOutlineMaterial()
    {
        Renderer[] allRenderers = transform.parent.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in allRenderers)
        {
            // check all materials on this renderer
            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name.Contains("OutlineMaterial"))
                {
                    outlineRenderer = renderer;
                    // create instance of the material to modify
                    outlineMaterialInstance = new Material(materials[i]);

                    materials[i] = outlineMaterialInstance;
                    renderer.materials = materials;

                    return;
                }
            }
        }

        Debug.LogWarning("OutlineMaterial not found in children!");
    }

    protected override void OnDartsAttached(int dartCount)
    {
        base.OnDartsAttached(dartCount);
    }
}