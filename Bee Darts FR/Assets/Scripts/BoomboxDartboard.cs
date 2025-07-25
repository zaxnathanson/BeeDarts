using UnityEngine;

public class BoomboxDartboard : Dartboard
{
    [Header("Boombox Audio")]
    [SerializeField] private AudioClip[] songsToPlay;
    [SerializeField] private float musicVolume = 0.4f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem musicParticles;

    // audio management
    private AudioSource musicSource;
    private int currentSongIndex;
    private bool isPlaying;

    protected override void Awake()
    {
        base.Awake();

        // setup music source
        SetupMusicSource();
    }

    // setup dedicated music audio source
    private void SetupMusicSource()
    {
        // create separate audio source for music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        // set initial song
        if (songsToPlay != null && songsToPlay.Length > 0)
        {
            musicSource.clip = songsToPlay[0];
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

    // start music and visual effects
    private void StartMusicAndEffects()
    {
        // start music if not playing
        if (!isPlaying)
        {
            StartMusic();
        }

        // start particles
        if (musicParticles != null && !musicParticles.isPlaying)
        {
            musicParticles.Play();
        }
    }

    // stop music and visual effects
    private void StopMusicAndEffects()
    {
        // stop music
        if (isPlaying)
        {
            StopMusic();
        }

        // stop particles
        if (musicParticles != null && musicParticles.isPlaying)
        {
            musicParticles.Stop();
        }
    }

    // start playing music
    private void StartMusic()
    {
        if (musicSource == null || songsToPlay == null || songsToPlay.Length == 0) return;

        // set current song
        musicSource.clip = songsToPlay[currentSongIndex];
        musicSource.Play();
        isPlaying = true;

        // prepare next song index
        currentSongIndex = (currentSongIndex + 1) % songsToPlay.Length;
    }

    // stop playing music
    private void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            isPlaying = false;
        }
    }
}