using UnityEngine;

public class FlowerDartBoard : Dartboard
{
    [SerializeField] ParticleSystem stayParticle;

    // for deciding whether to redo certain actions or not
    private bool hasBeenHit = false;

    // IMPORTNAT- only make one flower have this and the fsjdfhnu have it be the firsr flower

    [Header("Important Settings")]

    [Tooltip("Determines whether to respawn bees at the first or second spawn points")]
    [SerializeField] private bool isFirstFlower;

    public override void OnHit(Dart theDart)
    {
        base.OnHit(theDart);

        if (isFirstFlower)
        {
            BeeManager.Instance.firstFlower = true;
        }

        if (!hasBeenHit)
        {
            BeeManager.Instance.IncrementPoints(1);
            hasBeenHit = true;
        }
    }

    public override void OnStay(int numDarts)
    {
        base.OnStay(numDarts);
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
        }
    }
}
