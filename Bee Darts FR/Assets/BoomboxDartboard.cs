using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

public class BoomboxDartboard : Dartboard
{
    [Header("Boombox Specific Settings")]
    [SerializeField] ParticleSystem stayParticle;
    [SerializeField] private AudioClip[] songsToPlay;

    [Header("Bounce Settings")]
    [SerializeField] private float bounceY = 0.3f;
    [SerializeField] private float bounceDuration = 0.5f;
    [SerializeField] private float scaleValue = 1.1f;
    [SerializeField] private float scaleDuration = 0.5f;

    private int currentSong = 0;
    private AudioSource boxAudioSource;
    private bool isAnimating = false;
    private bool isPlaying = false;
    private Transform animationTarget;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private void Start()
    {
        boxAudioSource = GetComponent<AudioSource>();
        if (boxAudioSource == null)
        {
            boxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        boxAudioSource.playOnAwake = false;
        boxAudioSource.loop = true;

        // setting initial song
        if (songsToPlay != null && songsToPlay.Length > 0 && songsToPlay[0] != null)
        {
            boxAudioSource.clip = songsToPlay[currentSong];
        }
        if (transform.parent != null && transform.parent.childCount > 0)
        {
            animationTarget = transform.parent.GetChild(0);
            originalPosition = animationTarget.position;
            originalScale = animationTarget.localScale;
        }
        else
        {
            animationTarget = transform;
            originalPosition = animationTarget.position;
            originalScale = animationTarget.localScale;
        }
    }

    /*
    private void Update()
    {
        HandleAnimation();
    }
    */

    private void HandleAnimation()
    {
        if (boxAudioSource.isPlaying && !isAnimating)
        {
            StartAnimation();
        }
        else if (!boxAudioSource.isPlaying && isAnimating)
        {
            StopAnimation();
        }
    }

    private void StartAnimation()
    {
        if (animationTarget == null) return;

        isAnimating = true;

        animationTarget.DOKill();

        animationTarget.DOMoveY(originalPosition.y + bounceY, bounceDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        animationTarget.DOScale(originalScale * scaleValue, scaleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopAnimation()
    {
        if (animationTarget == null) return;

        isAnimating = false;

        animationTarget.DOKill();
        animationTarget.position = originalPosition;
        animationTarget.localScale = originalScale;
    }

    public override void OnHit(Dart theDart)
    {
        base.OnHit(theDart);
    }

    public override void OnStay(int numDarts)
    {

        Debug.Log("yoooo we in this");
        base.OnStay(numDarts);

        if (numDarts > 0)
        {
            if (!isPlaying)
            {
                StartMusic();
            }
            if (stayParticle != null && !stayParticle.isPlaying)
            {
                stayParticle.Play();
            }
        }
        else
        {
            if (isPlaying)
            {
                StopMusic();
            }

            if (stayParticle != null && stayParticle.isPlaying)
            {
                stayParticle.Stop();
            }
        }
    }

    private void StartMusic()
    {
        boxAudioSource.clip = songsToPlay[currentSong];
        boxAudioSource.volume = 0.4f;
        boxAudioSource.Play();
        isPlaying = true;

        // prepare next song
        currentSong = (currentSong + 1) % songsToPlay.Length;
    }

    private void StopMusic()
    {
        if (boxAudioSource != null)
        {
            boxAudioSource.Stop();
            isPlaying = false;
        }
    }
}