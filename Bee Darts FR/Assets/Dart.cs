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

    public DartStates currentDartState;

    public ExpressionAnimation[] IdleExpression;
    public ExpressionAnimation[] PullExpressions;
    public ExpressionAnimation[] ThrownExpressions;

    [SerializeField] Animator animator;

    ExpressionAnimation currentExpression;

    [SerializeField] float expressionFps;
    [SerializeField] Image faceImage;
    [SerializeField] Rigidbody body;
    [SerializeField] float flightNoiseStrength;
    [SerializeField] float flightNoiseSpeed;

    GameObject player;
    float force;

    int frameIndex;

    public bool canBeThrown;


    [SerializeField] LayerMask dartableLayers;

    [System.Serializable]
    public struct ExpressionAnimation
    {
        public Sprite[] sprites;
    }

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        canBeThrown = false;
    }

    void Start()
    {
        currentExpression = IdleExpression[0];
        StartCoroutine(AnimateCurrentExpression());

    }

    private void Update()
    {
        if (currentDartState == DartStates.THROWN) { Fly(); };
    }


    public void Fire(float power)
    {
        force = power;
        body.isKinematic = false;
        body.AddForce(player.transform.forward * power, ForceMode.Impulse);
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
        float speedInput = flightNoiseSpeed * Time.time * force;
        Vector3 randomForce = new Vector3(flightNoiseStrength * (Mathf.PerlinNoise(speedInput, speedInput) - 0.5f), 0, 0);
        body.position += randomForce * Time.deltaTime * force;

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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentDartState != DartStates.THROWN)
        {
            return;
        }

        if (collision.transform.gameObject.layer == dartableLayers)
        {
            ChangeDartState(DartStates.HIT);
            collision.transform.TryGetComponent(out Dartboard dartboardScript);
            if (dartboardScript != null)
            {
                dartboardScript.OnHit();
            }
        }
    }
}
