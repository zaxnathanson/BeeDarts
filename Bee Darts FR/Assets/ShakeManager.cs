using UnityEngine;
using DG.Tweening;

public class ShakeManager : MonoBehaviour
{
    public static ShakeManager Instance;

    private Vector3 originalPosition;
    private Tween shakeTween;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            originalPosition = transform.localPosition;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShakeCamera(float duration = 0.5f, float strength = 1f, int vibrato = 10)
    {
        // kill existing shake
        shakeTween?.Kill();

        // reset position before new shake
        transform.localPosition = originalPosition;

        // start new shake
        shakeTween = transform.DOShakePosition(duration, strength, vibrato)
            .OnComplete(() => transform.localPosition = originalPosition);
    }

    private void OnDestroy()
    {
        shakeTween?.Kill();
    }
}
