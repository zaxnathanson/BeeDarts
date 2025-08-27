using UnityEngine;

public class BasketballDartboard : Dartboard
{
    [Header("Reset Settings")]

    [SerializeField] private Basketball script;

    protected override void OnHit(Dart dart)
    {
        if (script == null)
        {
            Debug.LogWarning("The basketball script is not attached to the dartboard.");
            return;
        }

        base.OnHit(dart);

        script.ResetBall();
    }
}
