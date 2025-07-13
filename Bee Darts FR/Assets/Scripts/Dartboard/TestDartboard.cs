using UnityEngine;

public class TestDartboard : Dartboard
{
    private void Start()
    {
        if (hexList.Count > 0)
        {
            LowerList();
        }
    }

    public override void OnHit(Dart theDart)
    {
        base.OnHit(theDart);

        RaiseList();
    }
}
