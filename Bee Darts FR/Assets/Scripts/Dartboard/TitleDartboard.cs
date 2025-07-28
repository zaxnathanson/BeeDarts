using UnityEngine;

public class TitleDartboard : Dartboard
{
    [Header("Loading Settings")]

    [SerializeField] private int buildIndex;
    [SerializeField] private bool quitOnHit = false;

    protected override void OnHit(Dart dart)
    {
        base.OnHit(dart);

        if (!quitOnHit)
        {
            GameManager.Instance.LoadScene(buildIndex);
        }
        else
        {
            GameManager.Instance.QuitGame();
        }
    }

    protected override void OnDartsAttached(int dartCount)
    {
        base.OnDartsAttached(dartCount);
    }

}