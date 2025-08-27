using UnityEngine;
using DG.Tweening;

public class ShakeManager : MonoBehaviour
{
    public static ShakeManager Instance { get; private set; }

    private Vector3 originalPosition;
    private Tween shakeTween;
    private bool isShaking = false;

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
            return;
        }
    }

    public void ShakeCamera(float duration = 0.5f, float strength = 1f, int vibrato = 10)
    {
        Debug.Log("shakin that cam cam");

        // kill existing shake
        shakeTween?.Kill();

        // reset position before new shake
        transform.localPosition = originalPosition;

        isShaking = true;

        // start new shake
        shakeTween = transform.DOShakePosition(duration, strength, vibrato)
            .OnComplete(() => {
                transform.localPosition = originalPosition;
                isShaking = false;
            });
    }

    public void StopShake()
    {
        if (isShaking)
        {
            shakeTween?.Kill();
            transform.localPosition = originalPosition;
            isShaking = false;
        }
    }

    public bool IsShaking()
    {
        return isShaking;
    }

    private void OnDestroy()
    {
        shakeTween?.Kill();
    }
}