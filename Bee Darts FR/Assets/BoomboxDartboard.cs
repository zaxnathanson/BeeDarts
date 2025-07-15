using UnityEngine;
using UnityEngine.Audio;

public class BoomboxDartboard : Dartboard
{
    [Header("Boombox Specific Settings")]

    [SerializeField] ParticleSystem stayParticle;
    [SerializeField] private AudioClip songToPlay;

    private bool keepPlaying = false;
    private AudioSource boxAudioSource;

    private void Start()
    {
        boxAudioSource = GetComponent<AudioSource>();

        boxAudioSource.playOnAwake = false;
        boxAudioSource.loop = true;
        boxAudioSource.clip = songToPlay;
    }

    private void Update()
    {
        if (keepPlaying)
        {
            boxAudioSource.Play();
        }
        else
        {
            boxAudioSource.Stop();
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

        keepPlaying = true;

        if (numDarts > 0)
        {
            if (!stayParticle.isPlaying)
            {
                stayParticle.Play();
            }
        }
        else
        {
            if (!stayParticle.isStopped)
            {
                stayParticle.Stop();
            }
            keepPlaying = false;
        }
    }
}
