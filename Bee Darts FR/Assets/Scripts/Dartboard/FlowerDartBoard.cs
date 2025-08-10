using UnityEngine;

public class FlowerDartboard : Dartboard
{
    [Header("Flower Settings")]
    [SerializeField] private ParticleSystem pollenParticles;

    [Header("Gameplay Settings")]
    [Tooltip("Set true for the first flower in the level to control bee spawn points")]
    [SerializeField] private bool isFirstFlower;

    // track if this flower has been hit for points
    private bool hasAwardedPoints;

    private CircleMovement circlingScript;
    private MovingWall movingScript;

    // handle dart hit
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

    // handle darts staying on flower
    protected override void OnDartsAttached(int dartCount)
    {
        base.OnDartsAttached(dartCount);

        // manage pollen particles based on dart presence
        UpdatePollenEffect(dartCount > 0);
    }

    // update pollen particle effect
    private void UpdatePollenEffect(bool shouldPlay)
    {
        if (pollenParticles == null) return;

        if (shouldPlay && !pollenParticles.isPlaying)
        {
            pollenParticles.Play();
        }
        else if (!shouldPlay && pollenParticles.isPlaying)
        {
            pollenParticles.Stop();
        }
    }
}