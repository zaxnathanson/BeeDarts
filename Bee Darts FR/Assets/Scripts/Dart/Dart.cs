using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Dart : MonoBehaviour
{
    public enum DartStates
    {
        PICKUP,
        HELD,
        CHARGING,
        THROWN,
        HIT,
    }

    [Header("Dart References")]

    [SerializeField] Rigidbody body;
    private DartThrowing dartThrowingRef;
    public DartStates currentDartState;
    public Vector3 lastVelocity;

    [Header("Expressions Settings")]

    public ExpressionAnimation[] IdleExpression;
    public ExpressionAnimation[] PullExpressions;
    public ExpressionAnimation[] ThrownExpressions;

    [SerializeField] Animator animator;
    ExpressionAnimation currentExpression;

    [SerializeField] float expressionFps;
    [SerializeField] Image faceImage;

    int frameIndex;
    public bool canBeThrown;
    public Vector3 thrownStartPos;

    [Header("Layer References")]

    [SerializeField] LayerMask dartableLayers;
    [SerializeField] LayerMask dartableLayersCopy;
    [SerializeField] private LayerMask undartableLayer;

    [System.Serializable]
    public struct ExpressionAnimation
    {
        public Sprite[] sprites;
    }

    public delegate void PickedUpEvent(Dart thisDart);
    public event PickedUpEvent OnPickedUp;

    private void Awake()
    {
        canBeThrown = false;
        dartableLayersCopy = dartableLayers;
        dartThrowingRef = GameObject.Find("Player").GetComponent<DartThrowing>();
    }

    void Start()
    {
        currentExpression = IdleExpression[0];
        StartCoroutine(AnimateCurrentExpression());
    }

    private void Update()
    {
        if (currentDartState == DartStates.THROWN)
        {
            Fly();
        }
    }

    private void FixedUpdate()
    {
        lastVelocity = body.linearVelocity;
    }

    public void Fire(float power)
    {
        body.isKinematic = false;
        body.AddForce(Camera.main.transform.forward * power, ForceMode.Impulse);
    }

    IEnumerator AnimateCurrentExpression()
    {
        faceImage.sprite = currentExpression.sprites[frameIndex];
        yield return new WaitForSeconds(1 / expressionFps);
        frameIndex++;
        if (frameIndex >= currentExpression.sprites.Length)
        {
            frameIndex = 0;
        }
        StartCoroutine(AnimateCurrentExpression());
    }

    void Fly()
    {
        if (body.linearVelocity.normalized != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(body.linearVelocity.normalized);
        }
    }

    public void ChangeDartState(DartStates newState)
    {
        currentDartState = newState;

        int newExpressionIndex;
        switch (currentDartState)
        {
            case DartStates.HELD:
                newExpressionIndex = Random.Range(0, IdleExpression.Length);
                currentExpression = IdleExpression[newExpressionIndex];
                body.isKinematic = true;
                StartCoroutine(Pickup());
                break;
            case DartStates.CHARGING:
                animator.SetBool("IsCharging", true);
                newExpressionIndex = Random.Range(0, PullExpressions.Length);
                currentExpression = PullExpressions[newExpressionIndex];
                break;
            case DartStates.THROWN:
                animator.SetBool("IsCharging", false);
                animator.SetBool("IsFlying", true);
                newExpressionIndex = Random.Range(0, ThrownExpressions.Length);
                currentExpression = ThrownExpressions[newExpressionIndex];
                break;
            case DartStates.HIT:
                animator.SetBool("IsFlying", false);
                animator.SetTrigger("Hit");
                body.isKinematic = true;
                newExpressionIndex = Random.Range(0, IdleExpression.Length);
                currentExpression = IdleExpression[newExpressionIndex];
                canBeThrown = false;
                break;
        }
    }

    IEnumerator Pickup()
    {
        OnPickedUp?.Invoke(this);

        transform.localPosition = new Vector3(0, -0.5f, 0);
        Vector3 startPos = transform.localPosition;
        float elapsed = 0;

        while (elapsed < 0.2f)
        {
            transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, elapsed / 0.2f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = Vector3.zero;
        canBeThrown = true;
        dartThrowingRef.isGrabbing = false;
    }

    public void HandleGroundSideCollision()
    {
        dartableLayers = undartableLayer;
        animator.enabled = false;
        Invoke("ResetDartable", 0.1f);
    }

    private void ResetDartable()
    {
        animator.enabled = true;
        dartableLayers = dartableLayersCopy;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentDartState != DartStates.THROWN)
        {
            return;
        }

        if (dartableLayers == (dartableLayers | (1 << collision.gameObject.layer)))
        {
            transform.parent = collision.transform.parent;
            Debug.Log("Hit!!!!");

            ChangeDartState(DartStates.HIT);

            collision.transform.TryGetComponent(out Dartboard dartboardScript);
            if (dartboardScript != null)
            {
                dartboardScript.CheckHit(this);
            }
        }
    }
}