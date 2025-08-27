using UnityEngine;

public class GongDartboard : Dartboard
{
    [Header("Gong Settings")]

    [SerializeField] private AudioClip gongSound;

    [SerializeField] private float gongShakeDuration;
    [SerializeField] private float gongShakeStrength;
    [SerializeField] private int gongShakeVibrato;



    protected override void OnHit(Dart dart)
    {
        GameManager.Instance.PlaySFXWithRandomPitch(gongSound, transform.position, 1, 0.95f, 1.05f);
        ShakeManager.Instance.ShakeCamera(gongShakeDuration, gongShakeStrength, gongShakeVibrato);
    }

}
