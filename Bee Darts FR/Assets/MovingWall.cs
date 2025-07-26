using UnityEngine;
using DG.Tweening;

public class MovingObstacle : MonoBehaviour
{
    [Header("Movement Settings")]

    [SerializeField] private float moveDistance = 5f;
    [SerializeField] private float moveDuration = 2f;
    private Ease easeType = Ease.InOutSine;

    private Vector3 startPosition;
    private Vector3 leftPosition;
    private Vector3 rightPosition;

    private bool started = false;

    private void Start()
    {
        startPosition = transform.localPosition;

        // positions to move to the left and right (local)
        leftPosition = startPosition + transform.forward * (moveDistance / 2f);
        rightPosition = startPosition + -transform.forward * (moveDistance / 2f);
    }

    private void Update()
    {
        if (transform.position.y > BeeManager.Instance.waterLevel && !started)
        {
            started = true;
            StartMovement();
        }
    }

    private void StartMovement()
    {
        transform.localPosition = leftPosition;

        // sequence to reuse
        Sequence moveSequence = DOTween.Sequence();

        moveSequence.Append(transform.DOLocalMove(rightPosition, moveDuration).SetEase(easeType));
        moveSequence.Append(transform.DOLocalMove(leftPosition, moveDuration).SetEase(easeType));

        // infinite tween loop
        moveSequence.SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // local to world for gizmo drawing
            Vector3 worldLeft = transform.parent ? transform.parent.TransformPoint(leftPosition) : leftPosition;
            Vector3 worldRight = transform.parent ? transform.parent.TransformPoint(rightPosition) : rightPosition;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(worldLeft, 0.2f);
            Gizmos.DrawWireSphere(worldRight, 0.2f);
            Gizmos.DrawLine(worldLeft, worldRight);
        }
        else
        {
            Vector3 gizmoStartPosition = transform.localPosition;
            Vector3 left = gizmoStartPosition + transform.forward * (moveDistance / 2f);
            Vector3 right = gizmoStartPosition + -transform.forward * (moveDistance / 2f);

            Vector3 worldLeft = transform.parent ? transform.parent.TransformPoint(left) : left;
            Vector3 worldRight = transform.parent ? transform.parent.TransformPoint(right) : right;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(worldLeft, 0.2f);
            Gizmos.DrawWireSphere(worldRight, 0.2f);
            Gizmos.DrawLine(worldLeft, worldRight);
        }
    }
}