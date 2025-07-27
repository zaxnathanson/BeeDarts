using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]

    [SerializeField] private AudioClip[] musicTracks;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float fadeTime = 1f;

    [Header("Boombox")]

    [SerializeField] private float boomboxVolume = 0.4f;

    private AudioSource musicSource;
    private AudioSource boomboxSource;
    private int currentMusicIndex = 0;
    private float savedMusicVolume;
    private Coroutine fadeCoroutine;

    private AudioClip[] boomboxPlaylist;
    private int boomboxSongIndex = 0;

    // tracking for auto-play next
    private Coroutine musicCheckCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // setting up sources in script because im lazy
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        boomboxSource = gameObject.AddComponent<AudioSource>();
        boomboxSource.loop = true;
        boomboxSource.volume = boomboxVolume;

        // play first song
        if (musicTracks != null && musicTracks.Length > 0)
        {
            PlayMusic(0);
        }
    }

    private void OnDestroy()
    {
        // cleanup coroutines
        if (musicCheckCoroutine != null) StopCoroutine(musicCheckCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
    }

    #region SFX
    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    public void PlaySFX(AudioClip clip, Transform target, float volume = 1f)
    {
        if (clip == null || target == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = target.position;
        tempGO.transform.SetParent(target);

        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.Play();

        Destroy(tempGO, clip.length);
    }

    public void PlaySFXWithRandomPitch(AudioClip clip, Vector3 position, float volume = 1f, float pitchMin = 0.9f, float pitchMax = 1.1f)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.spatialBlend = 0.9f;
        tempSource.pitch = Random.Range(pitchMin, pitchMax);
        tempSource.Play();

        Destroy(tempGO, clip.length);
    }

    public void PlayFootstep(AudioClip clip, Vector3 position, float volume = 0.5f)
    {
        if (clip == null) return;
        // using pitch variation for footsteps
        PlaySFXWithRandomPitch(clip, position, volume, 0.85f, 1.15f);
    }
    #endregion

    #region Music
    public void PlayMusic(int trackIndex)
    {
        if (musicTracks == null || trackIndex >= musicTracks.Length) return;

        currentMusicIndex = trackIndex;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToNewTrack(musicTracks[trackIndex]));

        // start checking for song end
        if (musicCheckCoroutine != null) StopCoroutine(musicCheckCoroutine);
        musicCheckCoroutine = StartCoroutine(CheckForSongEnd());
    }

    public void PlayNextMusic()
    {
        currentMusicIndex = (currentMusicIndex + 1) % musicTracks.Length;
        PlayMusic(currentMusicIndex);
    }

    public void StopMusic()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (musicCheckCoroutine != null) StopCoroutine(musicCheckCoroutine);
        fadeCoroutine = StartCoroutine(FadeOut(musicSource));
    }

    private IEnumerator FadeToNewTrack(AudioClip newClip)
    {
        if (musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOut(musicSource));
        }

        // switching and fading in new
        musicSource.clip = newClip;
        musicSource.Play();
        yield return StartCoroutine(FadeIn(musicSource, musicVolume));
    }

    private IEnumerator FadeIn(AudioSource source, float targetVolume)
    {
        source.volume = 0;
        float elapsed = 0;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0, targetVolume, elapsed / fadeTime);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator FadeOut(AudioSource source)
    {
        float startVolume = source.volume;
        float elapsed = 0;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeTime);
            yield return null;
        }

        source.volume = 0;
        source.Stop();
    }

    private IEnumerator CheckForSongEnd()
    {
        while (true)
        {
            // wait a bit before checking
            yield return new WaitForSeconds(1f);

            // if music is playing and near the end (or stopped)
            if (musicSource != null && musicSource.clip != null)
            {
                float timeRemaining = musicSource.clip.length - musicSource.time;

                // if less than fade time remaining or already stopped, start next song
                if (timeRemaining <= fadeTime || !musicSource.isPlaying)
                {
                    // only auto-advance if not being overridden by boombox
                    if (boomboxSource == null || !boomboxSource.isPlaying)
                    {
                        PlayNextMusic();
                        yield break;
                    }
                }
            }
        }
    }
    #endregion

    #region Boombox
    public void SetupBoombox(AudioClip[] playlist, float volume = 0.4f)
    {
        boomboxPlaylist = playlist;
        boomboxVolume = volume;
        boomboxSongIndex = 0;

        // preload first song to avoid lag spike
        if (boomboxPlaylist != null && boomboxPlaylist.Length > 0)
        {
            boomboxSource.clip = boomboxPlaylist[0];
        }
    }

    public void StartBoombox()
    {
        if (boomboxPlaylist == null || boomboxPlaylist.Length == 0) return;

        // muting main music
        savedMusicVolume = musicSource.volume;
        musicSource.volume = 0;

        // smooth start to avoid pops
        boomboxSource.clip = boomboxPlaylist[boomboxSongIndex];
        boomboxSource.volume = 0;
        boomboxSource.Play();
        StartCoroutine(QuickFadeIn(boomboxSource, boomboxVolume));
    }

    public void StopBoombox()
    {
        // quick fade out to avoid pops
        StartCoroutine(QuickFadeOut(boomboxSource, () => {
            // advancing song
            boomboxSongIndex = (boomboxSongIndex + 1) % boomboxPlaylist.Length;
            musicSource.volume = savedMusicVolume;
        }));
    }

    private IEnumerator QuickFadeIn(AudioSource source, float targetVolume)
    {
        float elapsed = 0;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0, targetVolume, elapsed / 0.1f);
            yield return null;
        }
        source.volume = targetVolume;
    }

    private IEnumerator QuickFadeOut(AudioSource source, System.Action onComplete)
    {
        float startVolume = source.volume;
        float elapsed = 0;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0, elapsed / 0.1f);
            yield return null;
        }
        source.Stop();
        onComplete?.Invoke();
    }

    public bool IsBoomboxPlaying => boomboxSource != null && boomboxSource.isPlaying;
    #endregion
}