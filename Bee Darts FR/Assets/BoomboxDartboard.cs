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

    private void Start()
    {
        boxAudioSource = GetComponent<AudioSource>();

        boxAudioSource.playOnAwake = false;
        boxAudioSource.loop = true;

        boxAudioSource.clip = songsToPlay[currentSong];
    }

    private void Update()
    {
        base.DebugTest();

        if (boxAudioSource.isPlaying && !isAnimating)
        {
            isAnimating = true;
            transform.parent.GetChild(0).DOMoveY(transform.position.y + bounceY, bounceDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            transform.parent.GetChild(0).DOScale(scaleValue, scaleDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }
        else if (!boxAudioSource.isPlaying && isAnimating)
        {
            isAnimating = false;
            transform.parent.GetChild(0).DOKill();
        }
    }

    public override void OnHit(Dart theDart)
    {
        base.OnHit(theDart);
    }

    public override void OnStay(int numDarts)
    {

        base.OnStay(numDarts);
        Debug.Log("staying");

        if (numDarts > 0)
        {
            if (boxAudioSource != null && boxAudioSource.clip != null && !isPlaying)
            {
                boxAudioSource.clip = songsToPlay[currentSong];

                currentSong++;

                isPlaying = true;
                
                if (currentSong > songsToPlay.Length - 1)
                {
                    currentSong = 0;
                }

                boxAudioSource.Play();
            }

            if (!stayParticle.isPlaying)
            {
                stayParticle.Play();
            }
        }
        else if (numDarts <= 0)
        {
            if (boxAudioSource != null && boxAudioSource.clip != null && isPlaying)
            {
                isPlaying = false;
                boxAudioSource.Stop();
            }

            if (!stayParticle.isStopped)
            {
                stayParticle.Stop();
            }
        }
    }
}
