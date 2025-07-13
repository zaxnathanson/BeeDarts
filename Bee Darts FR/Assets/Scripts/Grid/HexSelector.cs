using System.Collections.Generic;
using UnityEngine;

public class HexSelector : MonoBehaviour
{
    [Header("Hex List")]
    public List<GameObject> hexList = new List<GameObject>();

    [Header("Selection Mode")]
    public bool addMode = false;

    /*
    [ContextMenu("Add Selected Hexagons")]
    public void AddSelectedHexagons()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && addMode)
        {
            foreach (GameObject obj in UnityEditor.Selection.gameObjects)
            {
                if (obj.CompareTag("Hexagon") && !hexList.Contains(obj))
                {
                    hexList.Add(obj);
                }
            }
        }
#endif
    }
    */

    [ContextMenu("Clear List")]
    public void ClearList()
    {
        hexList.Clear();
    }

    [ContextMenu("Lower Hexagons From List")]
    public void LowerList()
    {
        HexManager.Instance.LowerHexagonsInList(hexList);
    }

    [ContextMenu("Raise Hexagons From List")]
    public void RaiseList()
    {
        HexManager.Instance.LiftHexagonsInList(hexList);
    }
}