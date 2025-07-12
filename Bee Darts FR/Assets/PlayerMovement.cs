using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Ground Check Settings")]

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody rb;
    private Vector3 input;
    private bool isGrounded;
    private bool jumpQueued = false;

    private float horizontal;
    private float vertical;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // handling inputs only in update
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Movement();
        Jump();

        Debug.Log("isGrounded: {" + isGrounded + "}");
    }

    private void Movement()
    {
        input = transform.right * horizontal + transform.forward * vertical;

        Vector3 move = input.normalized * moveSpeed;
        Vector3 velocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        rb.linearVelocity = velocity;
    }

    private void Jump()
    {
        if (jumpQueued && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            jumpQueued = false;
        }
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }


    // draw gizmo for ground check in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundDistance);
    }
}