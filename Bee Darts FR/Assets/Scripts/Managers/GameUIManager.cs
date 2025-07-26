using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;

    [Header("Reticle")]
    [SerializeField] Image reticleImage;

    [Header("Too Close Indicator")]
    [SerializeField] private GameObject tooCloseIndicator;
    [SerializeField] private Image tooCloseImage;
    [SerializeField] private Sprite[] tooCloseSprites = new Sprite[2];
    [SerializeField] private float animationSpeed = 0.5f;

    [Header("Pulse Animation")]
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Rock Animation")]
    [SerializeField] private float rockAngle = 10f;
    [SerializeField] private float rockSpeed = 3f;

    private Coroutine animationCoroutine;
    private bool isShowingTooClose = false;
    private bool isAnimationRunning = false;
    private Vector3 originalScale;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        if (tooCloseIndicator != null)
        {
            tooCloseIndicator.SetActive(false);
            // store original scale
            originalScale = tooCloseIndicator.transform.localScale;
        }
    }

    public void ChangeReticleColor(Color newColor)
    {
        reticleImage.color = newColor;
    }

    public void ShowTooCloseIndicator(bool show)
    {
        if (isShowingTooClose == show) return;

        isShowingTooClose = show;

        if (tooCloseIndicator != null)
        {
            tooCloseIndicator.SetActive(show);

            if (show)
            {
                // only start animation if not already running
                if (!isAnimationRunning)
                {
                    if (animationCoroutine != null)
                    {
                        StopCoroutine(animationCoroutine);
                    }
                    animationCoroutine = StartCoroutine(AnimateTooCloseIndicator());
                    isAnimationRunning = true;
                }
            }
            else
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                    animationCoroutine = null;
                }
                isAnimationRunning = false;
                // reset to original state
                tooCloseIndicator.transform.localScale = originalScale;
                tooCloseIndicator.transform.rotation = Quaternion.identity;
            }
        }
    }

    private IEnumerator AnimateTooCloseIndicator()
    {
        if (tooCloseImage == null || tooCloseSprites == null || tooCloseSprites.Length < 2)
        {
            Debug.LogWarning("Too close indicator not properly configured!");
            yield break;
        }

        int currentFrame = 0;
        float pulseTime = 0f;
        float spriteTimer = 0f;

        // animate forever
        while (true)
        {
            // sprite switching based on animation speed
            spriteTimer += Time.deltaTime;
            if (spriteTimer >= animationSpeed)
            {
                tooCloseImage.sprite = tooCloseSprites[currentFrame];
                currentFrame = (currentFrame + 1) % 2;
                spriteTimer = 0f;
            }

            // smooth pulse and rotation
            pulseTime += Time.deltaTime;

            // pulse scale
            float scaleFactor = 1f + (pulseScale - 1f) * Mathf.Sin(pulseTime * pulseSpeed) * 0.5f;
            tooCloseIndicator.transform.localScale = originalScale * scaleFactor;

            // rock rotation
            float rotationZ = Mathf.Sin(pulseTime * rockSpeed) * rockAngle;
            tooCloseIndicator.transform.rotation = Quaternion.Euler(0, 0, rotationZ);

            yield return null;
        }
    }
}