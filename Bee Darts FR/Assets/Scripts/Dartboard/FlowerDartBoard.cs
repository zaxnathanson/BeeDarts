using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

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
    [Header("Hit Animation")]
    [SerializeField] private float tiltAngle = 15f;  // max tilt in degrees
    [SerializeField] private float tiltDuration = 0.15f;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private float overshootAngle = 5f;  // overshoot amount
    [SerializeField] private float settleDuration = 0.15f;

    private Material outlineMaterialInstance;
    private Renderer outlineRenderer;
    private Transform visualsTransform;
    // track if this flower has been hit for points
    private bool hasAwardedPoints;
    private CircleMovement circlingScript;
    private MovingWall movingScript;
    // track darts and their relative positions
    private List<Transform> attachedDarts = new List<Transform>();
    private List<Vector3> dartLocalPositions = new List<Vector3>();
    private List<Quaternion> dartLocalRotations = new List<Quaternion>();
    private bool isAnimating = false;

    private void Start()
    {
        FindOutlineMaterial();
        // find visuals child
        visualsTransform = transform.parent.Find("Visuals");
        if (visualsTransform == null)
        {
            Debug.LogWarning("Visuals child not found!");
        }
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

        // only update dart positions during animation
        if (isAnimating)
        {
            UpdateDartPositions();
        }
    }

    protected override void OnHit(Dart dart)
    {
        base.OnHit(dart);

        // store dart's relative position before tilting
        if (visualsTransform != null)
        {
            GameObject.Find("Player").GetComponent<Grab>().canGrab = false;
            StartCoroutine(GrabDelay());

            attachedDarts.Add(dart.transform);
            dartLocalPositions.Add(visualsTransform.InverseTransformPoint(dart.transform.position));
            dartLocalRotations.Add(Quaternion.Inverse(visualsTransform.rotation) * dart.transform.rotation);

            AnimateTilt(dart.transform.position);
        }

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

    private void UpdateDartPositions()
    {
        if (visualsTransform == null) return;

        // update each dart to maintain relative position
        for (int i = attachedDarts.Count - 1; i >= 0; i--)
        {
            if (attachedDarts[i] == null)
            {
                // dart was destroyed, remove from lists
                attachedDarts.RemoveAt(i);
                dartLocalPositions.RemoveAt(i);
                dartLocalRotations.RemoveAt(i);
            }
            else
            {
                // transform dart position based on current visuals rotation
                attachedDarts[i].position = visualsTransform.TransformPoint(dartLocalPositions[i]);
                attachedDarts[i].rotation = visualsTransform.rotation * dartLocalRotations[i];
            }
        }
    }

    private void AnimateTilt(Vector3 dartPosition)
    {
        // get impact position in local space
        Vector3 localImpact = visualsTransform.InverseTransformPoint(dartPosition);

        bool isBackSide = localImpact.z < 0;

        // project onto the XY plane
        Vector2 impactDirection = new Vector2(localImpact.x, localImpact.y).normalized;

        // for a tilt away from the impact point, we rotate around an axis perpendicular to the impact
        Vector3 tiltAxis = new Vector3(impactDirection.y, -impactDirection.x, 0).normalized;

        // calculate tilt magnitude based on distance from center
        float impactDistance = new Vector2(localImpact.x, localImpact.y).magnitude;
        float tiltMagnitude = Mathf.Min(impactDistance * 2f, 1f) * tiltAngle;

        if (!isBackSide)
        {
            tiltMagnitude = -tiltMagnitude;
        }

        Quaternion originalRotation = visualsTransform.rotation;
        Quaternion localTilt = Quaternion.AngleAxis(tiltMagnitude, tiltAxis);
        Quaternion targetRotation = originalRotation * localTilt;

        // calculate overshoot rotation
        Quaternion localOvershoot = Quaternion.AngleAxis(-overshootAngle * (isBackSide ? 1f : -1f), tiltAxis);
        Quaternion overshootRotation = originalRotation * localOvershoot;

        visualsTransform.DOKill();

        isAnimating = true;

        // full animation sequence
        Sequence tiltSequence = DOTween.Sequence();

        tiltSequence.Append(visualsTransform.DORotateQuaternion(targetRotation, tiltDuration)
            .SetEase(Ease.OutCubic));
        tiltSequence.Append(visualsTransform.DORotateQuaternion(overshootRotation, returnDuration)
            .SetEase(Ease.InCubic));
        tiltSequence.Append(visualsTransform.DORotateQuaternion(originalRotation, settleDuration)
            .SetEase(Ease.OutSine));

        tiltSequence.OnComplete(() => {
            isAnimating = false;
            // final position update to ensure darts are in correct spots
            UpdateDartPositions();
            // clear tracking lists after animation completes
            attachedDarts.Clear();
            dartLocalPositions.Clear();
            dartLocalRotations.Clear();
        });
    }

    private IEnumerator GrabDelay()
    {
        yield return new WaitForSeconds(tiltDuration + settleDuration + returnDuration + 0.05f);

        GameObject.Find("Player").GetComponent<Grab>().canGrab = true;
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