using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class WoodchipperDartboard : Dartboard
{
    [Header("Woodchipper Settings")]

    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private float delayBeforeExplosion = 1.5f;
    [SerializeField] private Vector3 explosionOffset = Vector3.up * 2f;

    [Header("Shake Settings")]

    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeDuration = 2f;
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private float shakeRandomness = 90;


    private bool isProcessing = false;

    public override void OnHit(Dart theDart)
    {
        if (isProcessing)
            return;

        isProcessing = true;

        theDart.gameObject.SetActive(false);
        transform.DOShakePosition(shakeDuration, shakeIntensity, shakeVibrato, shakeRandomness, false, false).OnComplete(() => ProcessExplosion(theDart));
    }

    private void ProcessExplosion(Dart theDart)
    {
        Vector3 explosionPosition = transform.position + explosionOffset;

        // Create explosion after delay
        DOVirtual.DelayedCall(delayBeforeExplosion - shakeDuration, () =>
        {
            if (explosionParticle != null)
            {
                Instantiate(explosionParticle, explosionPosition, Quaternion.identity);
                Destroy(transform.GetChild(0).gameObject);
                GetComponent<BoxCollider>().enabled = false;
            }

            // Respawn bee at explosion location
            DOVirtual.DelayedCall(0.2f, () =>
            {
                theDart.transform.position = transform.position;
                theDart.gameObject.SetActive(true);

                theDart.GetComponent<Rigidbody>().isKinematic = false;
                theDart.GetComponent<Dart>().ChangeDartState(Dart.DartStates.THROWN);
            });
        });
    }
}