using System.Collections;
using System.Linq;
using UnityEngine;

public class RocketParticles : MonoBehaviour
{
    [Header("Rocket Settings")]

    [SerializeField] private float delayBetweenFirstAndSecond = 2.75f;
    [SerializeField] private float delayBetweenSecondAndThird = 2.75f;

    private ParticleSystem[] particles;

    private void Awake()
    {
        // grab child particle systems
        particles = GetComponentsInChildren<ParticleSystem>()
        .OrderBy(p => p.transform.GetSiblingIndex())
        .ToArray();
    }

    public void PlaySequence()
    {
        StartCoroutine(PlayParticles());
    }

    private IEnumerator PlayParticles()
    {
        // play first
        particles[0].Play();

        // wait then second
        yield return new WaitForSeconds(delayBetweenFirstAndSecond);
        particles[1].Play();

        // wait then third
        yield return new WaitForSeconds(delayBetweenSecondAndThird);
        particles[2].Play();
    }
}