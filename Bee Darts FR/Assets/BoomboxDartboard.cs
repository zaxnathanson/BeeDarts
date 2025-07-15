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
    private bool isPlaying = false;

    private void Start()
    {
        boxAudioSource = GetComponent<AudioSource>();

        boxAudioSource.playOnAwake = false;
        boxAudioSource.loop = true;
    }

    private void Update()
    {
        if (isPlaying)
        {
            transform.DOMoveY(transform.position.y + bounceY, bounceDuration).SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo);
            transform.DOScale(scaleValue, scaleDuration).SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo);
        }
    }

    public override void OnHit(Dart theDart)
    {
        base.OnHit(theDart);
    }

    public override void OnStay(int numDarts)
    {
        Debug.Log("staying");

        base.OnStay(numDarts);

        if (numDarts > 0)
        {
            if (boxAudioSource != null && boxAudioSource.clip != null && !isPlaying)
            {
                boxAudioSource.clip = songsToPlay[currentSong];

                currentSong++;
                
                if (currentSong > songsToPlay.Length - 1)
                {
                    currentSong = 0;
                }

                isPlaying = true;
                boxAudioSource.Play();
            }

            if (!stayParticle.isPlaying)
            {
                stayParticle.Play();
            }
        }
        else
        {
            if (boxAudioSource != null && boxAudioSource.clip != null)
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
