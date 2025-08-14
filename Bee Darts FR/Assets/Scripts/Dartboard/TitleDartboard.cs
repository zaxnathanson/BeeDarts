using UnityEngine;

public class TitleDartboard : Dartboard
{
    [Header("Loading Settings")]

    [SerializeField] private int buildIndex;
    [SerializeField] private bool quitOnHit = false;

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnHit(Dart dart)
    {
        base.OnHit(dart);

        SwitchScene();
    }

    protected override void OnDartsAttached(int dartCount)
    {
        base.OnDartsAttached(dartCount);
    }

    private void SwitchScene()
    {
        if (!quitOnHit)
        {
            GameUIManager.Instance.StartSceneTransition(buildIndex, false);
        }
        else
        {
            GameManager.Instance.QuitGame();
        }
    }

}