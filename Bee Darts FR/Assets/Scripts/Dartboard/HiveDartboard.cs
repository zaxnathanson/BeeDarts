using UnityEngine;

public class HiveDartboard : Dartboard
{
    [Header("Hive Settings")]
    [SerializeField] private ParticleSystem honeyParticles;

    // handle darts staying on hive
    protected override void OnDartsAttached(int dartCount)
    {
        base.OnDartsAttached(dartCount);

        // manage honey particles based on dart presence
        UpdateHoneyEffect(dartCount > 0);
    }

    // update honey particle effect
    private void UpdateHoneyEffect(bool shouldPlay)
    {
        if (honeyParticles == null) return;

        if (shouldPlay && !honeyParticles.isPlaying)
        {
            honeyParticles.Play();
        }
        else if (!shouldPlay && honeyParticles.isPlaying)
        {
            honeyParticles.Stop();
        }
    }
}