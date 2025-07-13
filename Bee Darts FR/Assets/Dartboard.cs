using System.Collections.Generic;
using UnityEngine;

public class Dartboard : MonoBehaviour
{
    [Header("Hex List")]

    public List<GameObject> hexList = new List<GameObject>();

    [Header("Visual Ref Settings")]

    [SerializeField] float distance;
    [SerializeField] SpriteRenderer tooCloseVisualRef;

    Transform player;
    List<Dart> attachedDarts = new List<Dart>();

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        DetectRange();
        OnStay(attachedDarts.Count);
    }

    private void DetectRange()
    {
        if (DartThrowing.instance.currentDart != null)
        {
            tooCloseVisualRef.enabled = Vector3.Distance(player.position, transform.position) <= distance; ;
        }
        else
        {
            tooCloseVisualRef.enabled = false;
        }
    }

    public void CheckHit(Dart theDart)
    {
        if (Vector3.Distance(theDart.thrownStartPos, transform.position) > distance)
        {
            OnHit(theDart);
        }
    }

    public virtual void OnHit(Dart theDart)
    {
        theDart.OnPickedUp += RemoveDartFromAttachedList;
        attachedDarts.Add(theDart);
        Debug.Log(gameObject.name + " was Hit!");
    }


    public virtual void OnStay(int numDarts)
    {
    }

    private void RemoveDartFromAttachedList(Dart theDart)
    {
        attachedDarts.Remove(theDart);
        attachedDarts.TrimExcess();
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, distance);
    }


    // Context menu functions for testing
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
