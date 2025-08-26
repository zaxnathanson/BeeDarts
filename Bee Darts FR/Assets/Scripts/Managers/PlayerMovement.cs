using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Ground Check Settings")]

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Audio Settings")]

    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float landingVolume = 0.7f;
    [SerializeField] private float stepInterval = 0.4f;

    [Header("Head Bob Settings")]

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobHorizontalAmplitude = 0.05f;
    [SerializeField] private float bobVerticalAmplitude = 0.03f;
    [SerializeField] private float landingBobIntensity = 0.2f;

    private Rigidbody rb;
    private Vector3 input;
    private bool isGrounded;
    private bool wasGrounded;
    private bool jumpQueued = false;
    private float horizontal;
    private float vertical;

    // footstep tracking
    private float stepTimer;
    private bool isMoving;

    // head bob tracking
    private Vector3 cameraStartPos;
    private float bobTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        wasGrounded = true;

        // store camera start position
        if (cameraTransform != null)
        {
            cameraStartPos = cameraTransform.localPosition;
        }
    }

    private void Update()
    {
        // handling inputs only in update
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Movement();
        Jump();
        HandleFootsteps();
        HandleLanding();
        HandleHeadBob();
    }

    private void Movement()
    {
        input = transform.right * horizontal + transform.forward * vertical;
        Vector3 move = input.normalized * moveSpeed;
        Vector3 velocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        rb.linearVelocity = velocity;

        // check if moving horizontally
        isMoving = input.magnitude > 0.1f && isGrounded;
    }

    private void Jump()
    {
        if (jumpQueued && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            jumpQueued = false;

            GameManager.Instance.PlaySFXWithRandomPitch(jumpSound, groundCheck.position, 1f, 0.5f, 0.7f);
        }
    }

    private void GroundCheck()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    private void HandleFootsteps()
    {
        if (isMoving && footstepSound != null)
        {
            stepTimer += Time.fixedDeltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void HandleLanding()
    {
        // check if just landed
        if (!wasGrounded && isGrounded && rb.linearVelocity.y <= 0)
        {
            PlayLandingSound();
            PlayLandingBob();
        }
    }

    private void HandleHeadBob()
    {
        if (cameraTransform == null) return;

        if (isMoving)
        {
            bobTimer += Time.fixedDeltaTime * bobFrequency;

            // calculate bob offset
            Vector3 offset = new Vector3(
                Mathf.Cos(bobTimer) * bobHorizontalAmplitude,
                Mathf.Sin(bobTimer * 2) * bobVerticalAmplitude,
                0
            );

            cameraTransform.localPosition = cameraStartPos + offset;
        }
        else
        {
            // smoothly return to start position when not walking
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraStartPos, Time.fixedDeltaTime * 5f);
            bobTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        // play footstep with automatic pitch variation
        GameManager.Instance.PlayFootstep(footstepSound, groundCheck.position, footstepVolume);
    }

    private void PlayLandingSound()
    {
        if (landingSound != null)
        {
            GameManager.Instance.PlaySFX(landingSound, groundCheck.position, landingVolume);
        }
    }

    private void PlayLandingBob()
    {
        if (cameraTransform != null)
        {
            cameraTransform.DOPunchPosition(Vector3.down * landingBobIntensity, 0.3f, 5, 0.5f);
        }
    }

    // draw gizmo for ground check in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundDistance);
    }
}