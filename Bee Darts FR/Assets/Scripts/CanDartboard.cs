using UnityEngine;

public class CanDartboard : Dartboard
{
    private ResetCans script;

    private void Start()
    {
        script = transform.parent.GetComponent<ResetCans>();
    }

    protected override void OnHit(Dart dart)
    {
        base.OnHit(dart);

        script.ReturnCans();
    }
}
