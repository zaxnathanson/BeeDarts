using System.Collections.Generic;
using UnityEngine;

public class Dartboard : MonoBehaviour
{
    [SerializeField] float distance;
    [SerializeField] SpriteRenderer tooCloseVisualRef;
    Transform player;
    List<Dart> attachedDarts = new List<Dart>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        DetectRange();
        OnStay(attachedDarts.Count);
    }

    void DetectRange()
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

    void RemoveDartFromAttachedList(Dart theDart)
    {
        attachedDarts.Remove(theDart);
        attachedDarts.TrimExcess();
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, distance);
    }
}
