using UnityEngine;
using TMPro;
using DG.Tweening;

public class HiveDartboard : Dartboard
{
    [Header("Hive Settings")]

    [SerializeField] private Transform targetPosition;

    [SerializeField] private Vector3 targetRotation;
    [SerializeField] private float tweenDuration = 2f;

    private TextMeshProUGUI beesText;

    private bool transitioned = false;

    private void Start()
    {
        // shhhhhhhhh no one has to know
        if (gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>() != null)
        {
            beesText = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("man thats awkward.");
        }
    }

    protected override void Update()
    {
        base.Update();
        if (beesText != null)
            beesText.text = AttachedDartCount.ToString() + " / 5";
        if (AttachedDartCount >= 5 && !transitioned)
        {
            transitioned = true;
            // start camera transition and particles
            StartTransition();

            // the line that will actually start the transition
            //GameUIManager.Instance.StartSceneTransition(2, false);
        }
    }

    private void StartTransition()
    {
        // move camera to target position and rotation
        Camera.main.transform.SetParent(null);
        Camera.main.gameObject.GetComponent<FirstPersonCameraRotation>().enabled = false;

        if (Camera.main != null && targetPosition != null)
        {
            Camera.main.transform.DOMove(targetPosition.position, tweenDuration);
            Camera.main.transform.DORotate(targetRotation, tweenDuration);
        }

        // find particles child and play sequence
        GameObject particlesChild = GameObject.Find("Particles");
        if (particlesChild != null)
        {
            RocketParticles rocketParticles = particlesChild.GetComponent<RocketParticles>();
            if (rocketParticles != null)
            {
                rocketParticles.PlaySequence();
            }
        }
    }
    protected override void OnDartsAttached(int dartCount)
    {
        base.OnDartsAttached(dartCount);
    }
}