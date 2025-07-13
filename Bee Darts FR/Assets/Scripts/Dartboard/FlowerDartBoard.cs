using UnityEngine;

public class FlowerDartBoard : Dartboard
{
    [SerializeField] ParticleSystem stayParticle;

    public override void OnHit(Dart theDart)
    {
        base.OnHit(theDart);
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
