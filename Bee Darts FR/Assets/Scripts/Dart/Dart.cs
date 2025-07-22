using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Dart : MonoBehaviour
{
    public enum DartState
    {
        PICKUP,
        HELD,
        CHARGING,
        THROWN,
        HIT
    }

    [Header("Core Components")]
    [SerializeField] private Rigidbody body;
    [SerializeField] private Animator animator;
    [SerializeField] private Image faceImage;

    [Header("Expression Settings")]
    [SerializeField] private ExpressionSet[] idleExpressions;
    [SerializeField] private ExpressionSet[] pullExpressions;
    [SerializeField] private ExpressionSet[] thrownExpressions;
    [SerializeField] private float expressionFps = 10f;

    [Header("Physics Settings")]
    [SerializeField] private LayerMask dartableLayers;
    [SerializeField] private LayerMask undartableLayers;

    // state management
    public DartState currentState = DartState.PICKUP;
    public Vector3 lastVelocity;
    private bool canBeThrown;

    // expression animation
    private Coroutine expressionCoroutine;
    private ExpressionSet currentExpression;
    private int frameIndex;

    // cached references
    private LayerMask originalDartableLayers;
    private Transform cachedTransform;

    // properties
    public DartState CurrentState => currentState;
    public bool CanBeThrown => canBeThrown;
    public bool HasRisen { get; private set; }
    public Vector3 ThrownStartPos { get; set; }

    // events
    public System.Action<Dart> OnPickedUp;
    public System.Action<Dart, Collision> OnHit;

    [System.Serializable]
    public struct ExpressionSet
    {
        public Sprite[] sprites;
    }

    private void Awake()
    {
        // cache references
        cachedTransform = transform;
        originalDartableLayers = dartableLayers;

        // validate components
        if (!body) body = GetComponent<Rigidbody>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // set initial expression
        SetRandomExpression(idleExpressions);
    }

    private void Update()
    {
        // only update when thrown
        if (currentState == DartState.THROWN)
        {
            UpdateFlightRotation();
        }

        // check water level
        if (!HasRisen && BeeManager.Instance && BeeManager.Instance.waterLevel <= cachedTransform.position.y)
        {
            HasRisen = true;
        }
    }

    private void FixedUpdate()
    {
        // track velocity for collision response
        if (currentState == DartState.THROWN)
        {
            lastVelocity = body.linearVelocity;
        }
    }

    // launch dart with given power
    public void Fire(float power, Vector3 direction)
    {
        body.isKinematic = false;
        body.AddForce(direction * power, ForceMode.Impulse);
    }

    // change dart state
    public void ChangeState(DartState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        OnStateChanged(newState);
    }

    // handle state transitions
    private void OnStateChanged(DartState newState)
    {
        switch (newState)
        {
            case DartState.HELD:
                body.isKinematic = true;
                animator.SetBool("IsCharging", false);
                animator.SetBool("IsFlying", false);
                SetRandomExpression(idleExpressions);
                StartCoroutine(AnimatePickup());
                break;

            case DartState.CHARGING:
                animator.SetBool("IsCharging", true);
                SetRandomExpression(pullExpressions);
                break;

            case DartState.THROWN:
                animator.SetBool("IsCharging", false);
                animator.SetBool("IsFlying", true);
                SetRandomExpression(thrownExpressions);
                break;

            case DartState.HIT:
                body.isKinematic = true;
                animator.SetBool("IsFlying", false);
                animator.SetTrigger("Hit");
                SetRandomExpression(idleExpressions);
                canBeThrown = false;
                break;
        }
    }

    // animate pickup motion
    private IEnumerator AnimatePickup()
    {
        OnPickedUp?.Invoke(this);

        // smooth pickup animation
        Vector3 startPos = cachedTransform.localPosition;
        Vector3 targetPos = Vector3.zero;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cachedTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        cachedTransform.localPosition = targetPos;
        canBeThrown = true;
    }

    // update rotation during flight
    private void UpdateFlightRotation()
    {
        if (body.linearVelocity.sqrMagnitude > 0.01f)
        {
            cachedTransform.rotation = Quaternion.LookRotation(body.linearVelocity.normalized);
        }
    }

    // set random expression from array
    private void SetRandomExpression(ExpressionSet[] expressions)
    {
        if (expressions == null || expressions.Length == 0) return;

        currentExpression = expressions[Random.Range(0, expressions.Length)];
        frameIndex = 0;

        // restart animation coroutine
        if (expressionCoroutine != null)
        {
            StopCoroutine(expressionCoroutine);
        }
        expressionCoroutine = StartCoroutine(AnimateExpression());
    }

    // animate face expression
    private IEnumerator AnimateExpression()
    {
        while (currentExpression.sprites != null && currentExpression.sprites.Length > 0)
        {
            faceImage.sprite = currentExpression.sprites[frameIndex];
            yield return new WaitForSeconds(1f / expressionFps);

            frameIndex = (frameIndex + 1) % currentExpression.sprites.Length;
        }
    }

    // handle ground/side collisions
    public void HandleGroundSideCollision()
    {
        StartCoroutine(TemporarilyDisableDartable());
    }

    // temporarily disable dartable collision
    private IEnumerator TemporarilyDisableDartable()
    {
        dartableLayers = undartableLayers;
        animator.enabled = false;

        yield return new WaitForSeconds(0.1f);

        animator.enabled = true;
        dartableLayers = originalDartableLayers;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // only process collisions when thrown
        if (currentState != DartState.THROWN) return;

        // check if hit dartable surface
        if ((dartableLayers & (1 << collision.gameObject.layer)) != 0)
        {
            transform.SetParent(collision.transform.parent);
            ChangeState(DartState.HIT);

            // notify listeners
            OnHit?.Invoke(this, collision);

            // check for dartboard hit
            var dartboard = collision.transform.GetComponent<Dartboard>();
            if (dartboard != null)
            {
                dartboard.CheckHit(this);
            }
        }
    }

    private void OnDestroy()
    {
        // cleanup
        if (expressionCoroutine != null)
        {
            StopCoroutine(expressionCoroutine);
        }
    }
}