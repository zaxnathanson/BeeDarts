using UnityEngine;
using TMPro;

public class HiveDartboard : Dartboard
{
    [Header("Hive Settings")]

    [SerializeField] private ParticleSystem honeyParticles;

    private TextMeshProUGUI beesText;


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

        if (AttachedDartCount >= 5)
        {
            // win scene, temp code
            GameManager.Instance.LoadScene(2);
        }
    }

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