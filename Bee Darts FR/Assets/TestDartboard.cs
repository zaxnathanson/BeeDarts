using UnityEngine;

public class TestDartboard : Dartboard
{
    private HexSelector hexSelector;
    private void Start()
    {
        if (TryGetComponent<HexSelector>(out HexSelector script))
        {
            hexSelector = script;
        }
        else
        {
            Debug.LogWarning("No hex selector on this object");
        }

        HexManager.Instance.LowerHexagonsInList(hexSelector.hexList);
    }
    public override void OnHit(Dart theDart)
    {
        base.OnHit(theDart);

        HexManager.Instance.LiftHexagonsInList(hexSelector.hexList);
    }
}
