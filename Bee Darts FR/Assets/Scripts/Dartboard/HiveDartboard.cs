using UnityEngine;
using TMPro;
using DG.Tweening;

public class HiveDartboard : Dartboard
{
    [Header("Blast Off Settings")]

    [SerializeField] private bool doBlastOff;
    [SerializeField] private Transform cameraTargetPosition;
    [SerializeField] private Vector3 cameraTargetRotation;
    [SerializeField] private float cameraMoveDuration = 2f;

    [SerializeField] private float blastShakeDuration = 15f;
    [SerializeField] private float blastShakeStrength = 1f;
    [SerializeField] private int blastShakeVibrato = 10;

    [Header("Rocket Animation Settings")]

    [SerializeField] private float animationSpeedMultiplier = 1f;
    [SerializeField] private float rocketUpHeight = 10f;
    [SerializeField] private float stutterDistance = 2f;
    [SerializeField] private float stutterDuration = 1f;
    [SerializeField] private float finalLaunchHeight = 50f;
    [SerializeField] private float launchAngle = 15f;
    [SerializeField] private AnimationCurve launchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private TextMeshProUGUI beesText;
    private bool transitioned = false;
    private bool isFollowingRocket = false;

    private void Start()
    {
        // find bees text
        Transform textTransform = transform.GetChild(0).GetChild(0).GetChild(0);
        beesText = textTransform.GetComponent<TextMeshProUGUI>();
        if (beesText == null) Debug.LogWarning("man thats awkward.");
    }

    protected override void Update()
    {
        base.Update();

        // update bees text
        if (beesText != null)
        {
            beesText.text = AttachedDartCount.ToString() + " / 5";
        }

        // camera follow logic during rocket animation
        if (isFollowingRocket)
        {
            Camera.main.transform.position = cameraTargetPosition.position;

            Camera.main.transform.LookAt(transform.position);
        }

        // check for blast off trigger
        if (AttachedDartCount >= 5 && !transitioned)
        {
            transitioned = true;

            if (doBlastOff)
            {
                StartBlastOffSequence();
            }
            else
            {
                GameUIManager.Instance.StartSceneTransition(2, false);
            }
        }
    }

    private void StartBlastOffSequence()
    {
        // disable player controls
        Camera.main.GetComponent<FirstPersonCameraRotation>().enabled = false;
        Camera.main.transform.parent.GetComponent<PlayerMovement>().enabled = false;

        //Camera.main.transform.SetParent(null);

        BeeManager.Instance.HideAllDarts();
        GameUIManager.Instance.HideAllUI();

        // move camera to position then start rocket animation
        if (cameraTargetPosition != null)
        {
            Camera.main.transform.DOMove(cameraTargetPosition.position, cameraMoveDuration).SetEase(Ease.OutQuad);
            Camera.main.transform.DORotate(cameraTargetRotation, cameraMoveDuration).SetEase(Ease.OutQuad)
                .OnComplete(StartRocketAnimation);
        }
        else
        {
            StartRocketAnimation();
        }
    }

    private void StartRocketAnimation()
    {
        GetComponent<MeshCollider>().enabled = false;

        // start particles
        GameObject particlesChild = GameObject.Find("Particles");
        if (particlesChild?.GetComponent<RocketParticles>() != null)
        {
            particlesChild.GetComponent<RocketParticles>().PlaySequence();
        }

        // start camera following
        isFollowingRocket = true;

        // start camera shake
        Debug.Log("calle dshake");
        ShakeManager.Instance.ShakeCamera(blastShakeDuration, blastShakeStrength, blastShakeVibrato);

        // animate rocket blast off
        Vector3 startPos = transform.position;
        Vector3 upPos = startPos + Vector3.up * rocketUpHeight;
        Vector3 stutterPos = upPos - Vector3.up * stutterDistance;
        Vector3 angleDirection = Quaternion.AngleAxis(launchAngle, Vector3.right) * Vector3.up;
        Vector3 finalPos = startPos + angleDirection * finalLaunchHeight;

        // create rocket animation sequence
        Sequence rocketSequence = DOTween.Sequence();

        rocketSequence.Append(transform.DOMove(upPos, 0.8f / animationSpeedMultiplier).SetEase(Ease.OutQuad));
        rocketSequence.Append(transform.DOMove(stutterPos, stutterDuration / animationSpeedMultiplier).SetEase(Ease.InOutSine));
        rocketSequence.AppendInterval(0.1f / animationSpeedMultiplier);
        rocketSequence.Append(transform.DOMove(finalPos, 1.5f / animationSpeedMultiplier).SetEase(launchCurve));
        rocketSequence.Join(transform.DORotate(new Vector3(launchAngle, 0, 0), 1.5f / animationSpeedMultiplier).SetEase(Ease.OutQuad));

        // transition to next scene when done
        rocketSequence.OnComplete(() => {
            GameUIManager.Instance.StartSceneTransition(2, false);
            transform.DOMove(finalPos * 2, 1.5f / animationSpeedMultiplier);
        });

        rocketSequence.Play();
    }

    protected override void OnDartsAttached(int dartCount)
    {
        base.OnDartsAttached(dartCount);
    }
}