using UnityEngine;

public class BoomboxDartboard : Dartboard
{
    [Header("Boombox Audio")]

    [SerializeField] private AudioClip[] songsToPlay;
    [SerializeField] private float musicVolume = 0.4f;

    [Header("Visual Effects")]

    [SerializeField] private ParticleSystem musicParticles;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (songsToPlay != null && songsToPlay.Length > 0)
        {
            GameManager.Instance.SetupBoombox(songsToPlay, musicVolume);
        }
    }

    // handle when darts are attached
    protected override void OnDartsAttached(int dartCount)
    {
        base.OnDartsAttached(dartCount);

        if (dartCount > 0)
        {
            StartMusicAndEffects();
        }
        else
        {
            StopMusicAndEffects();
        }
    }

    private void StartMusicAndEffects()
    {
        if (!GameManager.Instance.IsBoomboxPlaying)
        {
            GameManager.Instance.StartBoombox();
        }

        if (musicParticles != null && !musicParticles.isPlaying)
        {
            musicParticles.Play();
        }
    }

    private void StopMusicAndEffects()
    {
        if (GameManager.Instance.IsBoomboxPlaying)
        {
            GameManager.Instance.StopBoombox();
        }

        if (musicParticles != null && musicParticles.isPlaying)
        {
            musicParticles.Stop();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // ensure music stops if destroyed
        if (HasAttachedDarts && GameManager.Instance.IsBoomboxPlaying)
        {
            GameManager.Instance.StopBoombox();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // ensure music stops if disabled
        if (HasAttachedDarts && GameManager.Instance.IsBoomboxPlaying)
        {
            GameManager.Instance.StopBoombox();
        }
    }
}