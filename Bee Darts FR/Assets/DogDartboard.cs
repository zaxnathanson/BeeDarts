using UnityEngine;
using System.Collections;

public class DogDartboard : Dartboard
{
    [Header("Dog Settings")]
    [SerializeField] private float gifPlaytime = 2f;
    [SerializeField] private AudioClip dogBark;
    [SerializeField] private GameObject gifObj;
    [SerializeField] private float fadeSpeed = 5f; // how fast to fade in/out

    private bool isGifPlaying = false;
    private SpriteRenderer gifRenderer;
    private SpriteRenderer outlineRenderer;

    void Start()
    {
        if (gifObj != null)
        {
            gifRenderer = gifObj.transform.Find("Gif").GetComponent<SpriteRenderer>();
            outlineRenderer = gifObj.transform.Find("GifOutline").GetComponent<SpriteRenderer>();

            // starting transparent
            SetAlpha(0f);
        }
    }

    protected override void OnHit(Dart dart)
    {
        if (dogBark != null)
        {
            GameManager.Instance.PlaySFXWithRandomPitch(dogBark, transform.position, 1.0f, 0.9f, 1.1f);
        }

        // trigger gif playback if not already playing
        if (!isGifPlaying && gifObj != null)
        {
            StartCoroutine(PlayGifSequence());
        }
    }

    private IEnumerator PlayGifSequence()
    {
        isGifPlaying = true;

        // fade in
        yield return StartCoroutine(FadeTo(1f));

        yield return new WaitForSeconds(gifPlaytime - (2f / fadeSpeed));

        // fade out
        yield return StartCoroutine(FadeTo(0f));

        isGifPlaying = false;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = gifRenderer.color.a;
        float elapsed = 0f;

        while (elapsed < 1f / fadeSpeed)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed * fadeSpeed);
            SetAlpha(currentAlpha);
            yield return null;
        }

        // set final value
        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        // apply alpha to both sprite renderers
        if (gifRenderer != null)
        {
            Color color = gifRenderer.color;
            color.a = alpha;
            gifRenderer.color = color;
        }

        if (outlineRenderer != null)
        {
            Color color = outlineRenderer.color;
            color.a = alpha;
            outlineRenderer.color = color;
        }
    }
}