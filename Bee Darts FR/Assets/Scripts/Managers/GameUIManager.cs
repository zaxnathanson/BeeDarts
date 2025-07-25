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

    private Coroutine animationCoroutine;
    private bool isShowingTooClose = false;

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
                // start when showing
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(AnimateTooCloseIndicator());
            }
            else
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                    animationCoroutine = null;
                }
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

        // animating
        while (true)
        {
            tooCloseImage.sprite = tooCloseSprites[currentFrame];

            yield return new WaitForSeconds(animationSpeed);

            currentFrame = (currentFrame + 1) % 2;
        }
    }
}