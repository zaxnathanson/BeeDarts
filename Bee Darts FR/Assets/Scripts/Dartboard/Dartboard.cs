using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Dartboard : MonoBehaviour
{
    [Header("Hex List Settings")]

    public List<GameObject> hexList = new List<GameObject>();
    private float gizmoSize = 0.25f;
    public Color gizmoColorUnselected = new Color(1, 1, 1, 0.9f);
    public Color gizmoColorSelected = new Color(1, 0, 0, 1);

    [Header("Visual Ref Settings")]

    [SerializeField] float distance;
    [SerializeField] SpriteRenderer tooCloseVisualRef;

    [Header("Audio Settings")]

    [SerializeField] AudioClip hitSound;
    [SerializeField][Range(0f, 1f)] float hitSoundVolume = 1f;
    private AudioSource audioSource;

    Transform player;
    List<Dart> attachedDarts = new List<Dart>();
    [SerializeField] ParticleSystem hitParticle;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // for one shots
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void Update()
    {
        DetectRange();
        OnStay(attachedDarts.Count);
    }

    private void DetectRange()
    {
        if (tooCloseVisualRef != null)
        {
            if (DartThrowing.instance.currentDart != null)
            {
                tooCloseVisualRef.enabled = Vector3.Distance(player.position, transform.position) <= distance;
            }
            else
            {
                tooCloseVisualRef.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} does not have a too close visual ref assigned.");
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
        if (hitParticle != null)
        {
            Instantiate(hitParticle, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} does not have a hit particle assigned.");
        }

        // playing a sound on hit only if a sound was attached
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, hitSoundVolume);
        }

        theDart.OnPickedUp += RemoveDartFromAttachedList;
        attachedDarts.Add(theDart);
        RaiseList();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColorUnselected;
        foreach (GameObject hex in hexList)
        {
            if (hex != null)
                Gizmos.DrawSphere(new Vector3(hex.transform.position.x, (hex.transform.position.y + (2.5f * (hex.transform.localScale.y / 2))), hex.transform.position.z), gizmoSize);
        }
    }

    void OnDrawGizmosSelected()
    {
        //zax thing from higher up
        Gizmos.DrawWireSphere(transform.position, distance);

        //drawing highlighted as red
        Gizmos.color = gizmoColorSelected;
        foreach (GameObject hex in hexList)
        {
            if (hex != null)
                Gizmos.DrawSphere(new Vector3(hex.transform.position.x, (hex.transform.position.y + (2.5f * (hex.transform.localScale.y / 2))), hex.transform.position.z), gizmoSize);
        }
    }
}
