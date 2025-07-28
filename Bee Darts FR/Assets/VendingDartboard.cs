using UnityEngine;

public class VendingDartboard : Dartboard
{
    [Header("Vending Settings")]

    [SerializeField] private GameObject soldOut;
    [SerializeField] private AudioClip enoughMoney;
    [SerializeField] private AudioClip notEnoughMoney;

    [SerializeField] private int requiredPoints = 10;

    private bool hasPurchased;

    protected override void OnHit(Dart dart)
    {
        // check if player has enough points and hasn't purchased yet
        if (!hasPurchased && BeeManager.Instance != null)
        {
            if (BeeManager.Instance.playerPoints >= requiredPoints)
            {
                ProcessPurchase();
                GameManager.Instance.PlaySFX(enoughMoney, transform.position, 0.75f);
            }
            else
            {
                GameManager.Instance.PlaySFX(notEnoughMoney, transform.position, 0.75f);
            }
        }
        else
        {
            base.OnHit(dart);
        }
    }

    // handle purchase transaction
    private void ProcessPurchase()
    {
        hasPurchased = true;

        if (BeeManager.Instance != null)
        {
            BeeManager.Instance.DecrementPoints(requiredPoints);
        }

        soldOut.SetActive(true);

        BeeManager.Instance.RespawnBee();
    }
}