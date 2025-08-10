using UnityEngine;

public class GongDartboard : Dartboard
{
    [SerializeField] private AudioClip gongSound;

    protected override void OnHit(Dart dart)
    {
        GameManager.Instance.PlaySFXWithRandomPitch(gongSound, transform.position, 1, 0.7f, 1.3f);
    }

}
